using EasyNetQ;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Domain.Settings;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services
{
    public class BusFactory : IBusFactory
    {
        private readonly MessagingSettings _messagingSettings;
        private IBus _eventBus;
        private IBus _rpcBus;

        public BusFactory(IOptions<MessagingSettings> messagingOptions)
        {
            _messagingSettings = messagingOptions.Value;
        }

        public IBus CreateRpcBus()
        {
            return _rpcBus ?? (_rpcBus = CreateBus());
        }

        public IBus CreateEventsBus()
        {
            return _eventBus ?? (_eventBus = CreateBus());
        }

        private IBus CreateBus()
        {
            return RabbitHutch.CreateBus(_messagingSettings.ConnectionString, reg => reg.EnableLegacyTypeNaming());
        }
    }
}