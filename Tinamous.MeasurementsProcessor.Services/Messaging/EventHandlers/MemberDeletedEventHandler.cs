using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Membership.Messaging.Events.User;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    public class MemberDeletedEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private readonly ILogger _logger;
        private IDisposable _consumer;

        public MemberDeletedEventHandler(IBus bus, IMembershipService membershipService, ILogger logger)
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
            _consumer = _bus.SubscribeAsync<MemberDeletedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(MemberDeletedEvent obj)
        {
            _logger.LogInformation("MemberDeleted update: {0}", obj.User.UserId);

            try
            {
                await _membershipService.DeleteUserAsync(obj.User.AccountId, obj.User.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete member properties");
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