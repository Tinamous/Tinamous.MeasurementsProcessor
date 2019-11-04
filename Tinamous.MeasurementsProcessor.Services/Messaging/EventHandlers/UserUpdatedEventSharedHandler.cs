using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Membership.Messaging.Events;
using AutoMapper;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    /// <summary>
    /// Update the user stored in the database to the change published
    /// by the membership service.
    ///
    /// This uses a common queue and is processed by any one of the
    /// measurement services.
    ///
    /// This publishes a UserMeasurementPropertiesUpdatedEvent that should
    /// be handled by other Measurement services to clear their cache
    /// and other services that may have cached field propertie information etc.
    /// </summary>
    public class UserUpdatedEventSharedHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private IDisposable _consumer;

        public UserUpdatedEventSharedHandler(IBus bus, IMembershipService membershipService, IMapper mapper, ILogger logger)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _membershipService = membershipService;
            _mapper = mapper;
            _logger = logger;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            // This is a shared one to update the database. Heep service name as it's shared with Measurements service.
            string subscriptionId = "Measurements.UserProperties";
            _consumer = _bus.SubscribeAsync<IUserUpdatedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(IUserUpdatedEvent obj)
        {
            _logger.LogInformation("UserProperties update: {0}", obj.User.UserId);

            try
            {
                // TODO: Determine if it has actually changed in a way 
                // that affects the service, otherwise it causes an updated event.
                User user = _mapper.Map<User>(obj.UserDetails);
                await _membershipService.UpdateUserPropertiesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user properties. User: {0}, Version: {1}",
                    obj.UserDetails.FullUserName,
                    obj.Version);
                // Sink.
            }
        }

        public void Dispose()
        {
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        } 
    }
}