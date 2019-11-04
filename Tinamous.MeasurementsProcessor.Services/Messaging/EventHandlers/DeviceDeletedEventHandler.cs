using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Membership.Messaging.Events.Device;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    public class DeviceDeletedEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private readonly ILogger _logger;
        private IDisposable _consumer;

        public DeviceDeletedEventHandler(IBus bus, IMembershipService membershipService, ILogger logger)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _membershipService = membershipService;
            _logger = logger;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            // This is a shared one to update the database. 
            string subscriptionId = "MeasurementsProcessor.UserProperties";
            _consumer = _bus.SubscribeAsync<DeviceDeletedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(DeviceDeletedEvent obj)
        {
            _logger.LogInformation("DeviceDeletedEvent: {0}", obj.Device.UserId);

            try
            {
                await _membershipService.DeleteUserAsync(obj.Device.AccountId, obj.Device.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete device user properties");
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