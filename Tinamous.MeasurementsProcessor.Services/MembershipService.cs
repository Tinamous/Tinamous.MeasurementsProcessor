using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using AnalysisUK.Tinamous.Membership.Messaging.Dtos;
using AnalysisUK.Tinamous.Membership.Messaging.Dtos.Account;
using AnalysisUK.Tinamous.Membership.Messaging.Events;
using AnalysisUK.Tinamous.Membership.Messaging.Requests.Account;
using AnalysisUK.Tinamous.Membership.Messaging.Requests.User;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Enums;
using AutoMapper;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services
{
    /// <summary>
    /// User (Device/Member/Bot) service to look up device details
    /// (fields, computed fields, location etc) either from cached local
    /// repository or from the actual Membership Service.
    /// </summary>
    public class MembershipService : IMembershipService
    {
        private readonly IBus _bus;
        private readonly IMeasurementUserPropertiesRepository _measurementUserPropertiesRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipService> _logger;
        private static readonly Dictionary<Guid, User> UserCache = new Dictionary<Guid, User>();

        static readonly object LockObject = new object();

        public MembershipService(IBusFactory busFactory, IMeasurementUserPropertiesRepository measurementUserPropertiesRepository, IMapper mapper, ILogger<MembershipService> logger)
        {
            if (busFactory == null) throw new ArgumentNullException("busFactory");

            _bus = busFactory.CreateRpcBus();
            _measurementUserPropertiesRepository = measurementUserPropertiesRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<List<User>> LoadByAccountAsync(Guid accountId)
        {
            return _measurementUserPropertiesRepository.LoadAsync(accountId);
        }

        public async Task<List<AccountDto>> LoadAccountsAsync()
        {
            ListAccountsRequest request = new ListAccountsRequest {RequestSource = Source.Measurements, RequestingUser = new UserSummaryDto()};

            var response = await _bus.RequestAsync<ListAccountsRequest, ListAccountsResponse>(request);
            return response.Accounts;
        }

        public async Task<User> LoadAsync(Guid accountId, Guid userId, bool forceReload = false)
        {
            // Invalid userId.
            if (userId == Guid.Empty)
            {
                _logger.LogInformation("Invalid userId: {0}", userId);
                return null;
            }

            if (!forceReload && UserCache.ContainsKey(userId))
            {
                return UserCache[userId];
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                User user = await GetFromDynamoDBAsync(accountId, userId);
                if (user != null)
                {
                    return Cache(user);
                }

                return await GetUserFromMembership(accountId, userId);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"Load user {userId} took {stopwatch.ElapsedMilliseconds}");
            }
        }

        /// <summary>
        /// User is not in the local cache, get them from the Membership Service.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<User> GetUserFromMembership(Guid accountId, Guid userId)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Cache miss, requesting user from membership service. Id: {0}", userId);

                var request = new GetAccountUserByIdRequest
                {
                    User = new UserSummaryDto { AccountId = accountId, UserId = userId},
                    RequestSource = Source.Measurements
                };
                var response = await _bus.RequestAsync<GetAccountUserByIdRequest, GetAccountUserByIdResponse>(request);

                if (response.User == null)
                {
                    _logger.LogWarning("User not found: {0} (AccountId: {1})", userId, accountId);
                    return null;
                }

                User user = Cache(_mapper.Map<User>(response.User));

                // Clean up the user location.
                if (user.Location != null)
                {
                    if (!user.Location.IsValidLocation())
                    {
                        user.Location = null;
                    }
                }

                // User wasn't stored in the local DB so get them from the Membership service.
                await UpdateUserPropertiesAsync(user);

                return user;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Membership service timeout exception. Failed to load user: " + userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user: " + userId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"Load user from membership service took {stopwatch.ElapsedMilliseconds}");
            }
        }

        /// <summary>
        /// Try to load in the user from the local DynamoDB based cache.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<User> GetFromDynamoDBAsync(Guid accountId, Guid userId)
        {
            try
            {
                return await _measurementUserPropertiesRepository.LoadAsync(accountId, userId);
            }
            catch (Exception ex)
            {
                _logger .LogError(ex, "Failed to load user from DynamoDB");
                return null;
            }
        }

        #region Caching

        private User Cache(User user)
        {
            // If the user was null don't cache.
            if (user == null)
            {
                return null;
            }

            lock (LockObject)
            {
                if (UserCache.ContainsKey(user.UserId))
                {
                    var cachedUser = UserCache[user.UserId];

                    if (cachedUser.MembershipUserVersion > user.MembershipUserVersion)
                    {
                        _logger.LogWarning("Trying to cached user with older version.");
                        // Just return the original user.
                        return user;
                    }

                    // TODO: Check version.
                    UserCache[user.UserId] = user;
                }
                else
                {
                    UserCache.Add(user.UserId, user);
                }
            }

            return user;
        }

        public void UpdateUserCache(User user)
        {
            if (user.Location != null)
            {
                if (!user.Location.IsValidLocation())
                {
                    user.Location = null;
                }
            }

            // Cache will replace existing user object with the new one
            // if the membership version is newer.
            Cache(user);
        }

        public void RemoveFromCache(Guid userId)
        {
            if (UserCache.ContainsKey(userId))
            {
                lock (LockObject)
                {
                    if (UserCache.ContainsKey(userId))
                    {
                        UserCache.Remove(userId);
                    }
                }
            }
        }

        /// <summary>
        /// Notify the membership about any new fields that are on the device.
        ///
        /// Note that this will cause a UserUpdated event that updates the local cached user object.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="processedMeasurementEvent"></param>
        /// <returns></returns>
        public async Task UpdateFieldsAsync(User device, ProcessedMeasurementEvent processedMeasurementEvent)
        {
            await CheckDeviceHasAllFieldsAsync(processedMeasurementEvent, device);
        }

        /// <summary>
        /// Check that the device has all the field descriptors for the measurement fields
        /// if some are missing a event is published which can be used to add those fields
        /// to the user/device.
        ///
        /// TODO: This needs to update the device internally. Currently indicating to Membership service,
        /// that raises and event that this service then uses to update the device!.
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="device"></param>
        private async Task CheckDeviceHasAllFieldsAsync(ProcessedMeasurementEvent measurement, User device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            try
            {
                // Ensure we don't have a null collection
                var fieldDescriptors = device.FieldDescriptors ?? new List<FieldDescriptor>();

                NewUnknownFieldsEvent newEvent = new NewUnknownFieldsEvent
                {
                    Fields = new List<UnknownField>(),
                    Source = Source.Measurements
                };

                foreach (var measurementField in measurement.MeasurementFields)
                {
                    if (!HasFieldDescriptor(fieldDescriptors, measurement.Channel, measurementField.Name))
                    {
                        _logger.LogInformation("Device: {0} does not have a field descriptor for {1}",
                            device.FullUserName,
                            measurementField.Name);

                        newEvent.Fields.Add(new UnknownField
                        {
                            Name = measurementField.Name,
                            Channel = measurement.Channel,
                            Unit = measurementField.Unit,
                            Visible = true
                        });
                    }
                }

                if (newEvent.Fields.Count > 0)
                {
                    newEvent.User = new UserSummaryDto
                    {
                        AccountId = device.AccountId,
                        UserId = device.UserId,
                        FullUserName = device.FullUserName,
                        UserName = device.UserName,
                    };

                    _logger.LogInformation("Found {0} new fields for {1}.", newEvent.Fields.Count, device.FullUserName);
                    await _bus.PublishAsync(newEvent);

                    _logger .LogInformation("Local User not updated with changed fields. Must wait for Membership service to update and fire UserUpdated event.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to determine new fields");
            }
        }

        private bool HasFieldDescriptor(IEnumerable<FieldDescriptor> fieldDescriptors, int channel, string name)
        {
            return fieldDescriptors.Any(x => x.Channel == channel
                                             && x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }


        public void UpdateCachedUserLocation(Guid userId, LocationDetails location)
        {
            if (UserCache.ContainsKey(userId))
            {
                var user = UserCache[userId];

                if (ShouldUpdateLocation(user, location))
                {
                    user.Location = location;
                }
            }
        }

        #endregion

        #region Stored User properties

        public async Task UpdateUserPropertiesAsync(User updatedUser)
        {
            if (updatedUser.Deleted)
            {
                
                _logger.LogWarning("Updated User is Deleted.");
                await DeleteUserAsync(updatedUser.AccountId, updatedUser.UserId);
                return;
            }

            User user = await _measurementUserPropertiesRepository.LoadAsync(updatedUser.AccountId, updatedUser.UserId);

            if (user == null)
            {
                _logger.LogInformation("New user! Adding: {0}", updatedUser.FullUserName);
                await _measurementUserPropertiesRepository.SaveAsync(updatedUser);
                Cache(updatedUser);
                return;
            }

            if (updatedUser.MembershipUserVersion <= user.MembershipUserVersion)
            {
                _logger.LogWarning("Attempting to update local user with older membership version. Current Version: {0}, received version: {1}",
                    user.MembershipUserVersion,
                    updatedUser.MembershipUserVersion);

                // Ensure the cache has the latest (loaded) version.
                Cache(user);

                return;
            }

            _logger.LogWarning("Update local user membership version. Current Version: {0}, received version: {1}",
                user.MembershipUserVersion,
                updatedUser.MembershipUserVersion);

            // Update the stored user properties
            user.FieldDescriptors = updatedUser.FieldDescriptors;
            user.PublishOptions = updatedUser.PublishOptions ?? new PublishOptions { Mqtt = true };
            user.DisplayName = updatedUser.DisplayName;
            user.Location = updatedUser.Location;
            user.Tags = updatedUser.Tags;
            user.LastUpdated = updatedUser.LastUpdated;
            user.MeasurementsRetentionTimeDays = updatedUser.MeasurementsRetentionTimeDays;
            user.OwnerId = updatedUser.OwnerId;

            user.MembershipUserVersion = updatedUser.MembershipUserVersion;

            await _measurementUserPropertiesRepository.SaveAsync(user);

            // Update the Cached version
            Cache(user);
        }

        /// <summary>
        /// Track user (device) location so that this can be assigned to measurements
        /// if the measurement does not include it's own location.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="id"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task UpdateUserPropertiesLocationAsync(Guid accountId, Guid id, LocationDetails location)
        {
            User userProperties = await _measurementUserPropertiesRepository.LoadAsync(accountId, id);
            if (userProperties == null)
            {
                return;
            }

            if (ShouldUpdateLocation(userProperties, location))
            {
                userProperties.Location = location;
                await _measurementUserPropertiesRepository.SaveAsync(userProperties);
            }
        }

        public async Task DeleteUserAsync(Guid accountId, Guid userId)
        {
            var user = await _measurementUserPropertiesRepository.LoadAsync(accountId, userId);
            if (user != null)
            {
                await _measurementUserPropertiesRepository.DeleteAsync(user);
                RemoveFromCache(user.UserId);
            }
        }

        #endregion

        private bool ShouldUpdateLocation(User user, LocationDetails location)
        {
            if (user == null)
            {
                return false;
            }

            if (user.Location == null)
            {
                return true;
            }

            if (location == null)
            {
                return false;
            }

            if (!location.IsValidLocation())
            {
                return false;
            }

            if (!user.Location.IsValidLocation())
            {
                return true;
            }

            if (!user.Location.LastUpdated.HasValue)
            {
                return true;
            }

            if (!location.LastUpdated.HasValue)
            {
                return false;
            }

            if (user.Location.LastUpdated.Value < location.LastUpdated.Value)
            {
                return true;
            }

            Trace.WriteLine("Not updating user location");
            return false;
        }
    }
}