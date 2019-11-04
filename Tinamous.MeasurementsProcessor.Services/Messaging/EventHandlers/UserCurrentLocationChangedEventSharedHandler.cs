using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Membership.Messaging.Events.User;
using AutoMapper;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    public class UserCurrentLocationChangedEventSharedHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private IDisposable _consumer;

        public UserCurrentLocationChangedEventSharedHandler(IBus bus, IMembershipService membershipService, IMapper mapper, ILogger logger)
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
            // Shared databaae updater. Keep Measurements service name as it's shared with the measurements service.
            string subscriptionId = "Measurements.UserProperties";
            _consumer = _bus.SubscribeAsync<UserCurrentLocationChangedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(UserCurrentLocationChangedEvent obj)
        {
            try
            {
                LocationDetails location = _mapper.Map<LocationDetails>(obj.Location);
                await _membershipService.UpdateUserPropertiesLocationAsync(obj.User.AccountId, obj.User.UserId, location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user properties location");
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