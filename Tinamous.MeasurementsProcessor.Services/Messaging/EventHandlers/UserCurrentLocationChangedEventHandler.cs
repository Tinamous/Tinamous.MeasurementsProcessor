using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Membership.Messaging.Events.User;
using AutoMapper;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Settings;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    public class UserCurrentLocationChangedEventHandler : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private readonly IMapper _mapper;
        private IDisposable _consumer;
        private ServerSettings _serverSettings;

        public UserCurrentLocationChangedEventHandler(IBus bus, IMembershipService membershipService, IMapper mapper, IOptions<ServerSettings> serverOptions, ILogger logger)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _membershipService = membershipService;
            _mapper = mapper;
            _serverSettings = serverOptions.Value;
            _logger = logger;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            // Handle as IUserIpdated as it could be device or member.
            // Per server basis to update local cache.
            string subscriptionId = string.Format("MeasurementsProcessor.{0}", _serverSettings.ServerName);
            _consumer = _bus.SubscribeAsync<UserCurrentLocationChangedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(UserCurrentLocationChangedEvent obj)
        {
            try
            {
                LocationDetails location = _mapper.Map<LocationDetails>(obj.Location);
                _membershipService.UpdateCachedUserLocation(obj.User.UserId, location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user location");
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