using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Messaging.Common.Events.System;
using AnalysisUK.Tinamous.Messaging.Common.Requests;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Domain.Settings;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    public class PingEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;
        private IDisposable _consumer;
        private IDisposable _consumer2;
        private ServerSettings _serverSettings;

        public PingEventHandler(IBus bus, IOptions<ServerSettings> serverOptions, ILogger logger)
        {
            _bus = bus;
            _logger = logger;
            _serverSettings = serverOptions.Value;

            string subscriptionName = string.Format("MeasurementsProcessor.{0}", _serverSettings.ServerName);
            _consumer = _bus.SubscribeAsync<PingRequest>(subscriptionName, OnMessageAsync);
            _consumer2 = _consumer = _bus.RespondAsync<PingRequest, PingResponse>(OnRespondAsync);
        }

        private async Task OnMessageAsync(PingRequest arg)
        {
            if (!string.IsNullOrWhiteSpace(arg.ServiceName))
            {
                if (!arg.ServiceName.Equals(_serverSettings.ServiceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(arg.Server))
            {
                if (!arg.Server.Equals(_serverSettings.ServerName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            // Either it's exactly for us, or "wild card" enough to require a response.
            _bus.Publish(new PongEvent
            {
                Id = arg.Id,
                Server = _serverSettings.ServerName,
                ServiceName = _serverSettings.ServiceName,
                SoftwareVersion = _serverSettings.Version,
                DateTime = DateTime.UtcNow
            });
        }

        private async Task<PingResponse> OnRespondAsync(PingRequest arg)
        {
            return new PingResponse
            {
                Id = arg.Id,
                DateTime = DateTime.UtcNow,
                Server = _serverSettings.ServerName,
                ServiceName = _serverSettings.ServiceName,
                SoftwareVersion = _serverSettings.Version,
            };
        }

        public void Dispose()
        {
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }

            if (_consumer2 != null)
            {
                _consumer2.Dispose();
                _consumer2 = null;
            }
        }
    }
}