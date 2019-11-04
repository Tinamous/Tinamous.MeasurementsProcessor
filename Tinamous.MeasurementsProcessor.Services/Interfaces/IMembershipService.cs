using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using AnalysisUK.Tinamous.Membership.Messaging.Dtos.Account;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.Services.Interfaces
{
    public interface IMembershipService
    {
        // TODO: Store user/device fields in the membership service
        // rather than against the user.
        Task<User> LoadAsync(Guid accountId, Guid userId, bool forceReload = false);

        Task UpdateUserPropertiesAsync(User updatedUser);

        void UpdateCachedUserLocation(Guid userUserId, LocationDetails location);
        Task UpdateUserPropertiesLocationAsync(Guid accountId, Guid userUserId, LocationDetails location);
        Task DeleteUserAsync(Guid accountId, Guid userId);

        void UpdateUserCache(User user);
        void RemoveFromCache(Guid userUserId);
        Task UpdateFieldsAsync(User device, ProcessedMeasurementEvent processedMeasurementEvent);
        Task<List<User>> LoadByAccountAsync(Guid accountId);

        // Migration use
        Task<List<AccountDto>> LoadAccountsAsync();
    }
}