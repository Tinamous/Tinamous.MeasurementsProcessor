using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers
{
    /// <summary>
    /// Handles new measurements from RabbitMQ.
    /// Places the measurement on the Raw measurements Kinesis
    /// stream for processing.
    /// </summary>
    /// <remarks>
    /// TODO: Move away from RabbitMQ measurements and go to Kinesis measurement for
    /// the new measurements.
    /// 
    /// Need to check/update:
    /// * ParticleBot
    /// * MQTT
    /// * API
    /// * SigfoxBox
    /// * TTN Bot
    /// * Virtual Bot
    /// </remarks>
    public class NewDecimalMeasurementProcessor :  IDisposable
    {
        private readonly IBus _bus;
        private readonly IRawMeasurementStreamWriter _rawStreamWriter;
        private readonly ILogger _logger;
        private IDisposable _consumer;

        public NewDecimalMeasurementProcessor(IBus bus, 
            IRawMeasurementStreamWriter rawStreamWriter,
            ILogger logger)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _rawStreamWriter = rawStreamWriter ?? throw new ArgumentNullException(nameof(rawStreamWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            // Using the same queue as the Measurement service.
            // Eventually remove from measurements service.
            _consumer = _bus.SubscribeAsync<NewMeasurementEvent>("Measurements.NewMeasurementsProcessor", OnNewMeasurementAsync);
        }

        public async Task OnNewMeasurementAsync(NewMeasurementEvent newMeasurementEvent)
        {
            // We shouldn't get these any more, so any service publishing them needs
            // to be modified to use the Kinesis stream rather than EasyNetQ.
            _logger.LogWarning("Got new measurement (NewMeasurementEvent) to process. Id: {0}, UserId: {1}, Source:{2} through EasyNetQ! - Change to Kinesis!",
                newMeasurementEvent.Id, 
                newMeasurementEvent.UserId,
                newMeasurementEvent.Source);

            // Push onto the raw stream
            // this should be done by MQTT/API not here
            // to reduce dependency/load on RabbitMQ servers.
            await _rawStreamWriter.PushStreamAsync(newMeasurementEvent);
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