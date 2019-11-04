using Amazon.Kinesis;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Helpers;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Settings;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    public class AwsKinesisFactory : IAwsKinesisFactory
    {
        private readonly IAwsClientFactory _clientFactory;
        private readonly IHeartbeatService _heartbeatService;
        private readonly ILogger _logger;
        private readonly AwsSettings _awsSettings;

        public AwsKinesisFactory(IAwsClientFactory clientFactory, 
            IOptions<AwsSettings> awsOptions, 
            IHeartbeatService heartbeatService,
            ILogger logger)
        {
            _clientFactory = clientFactory;
            _heartbeatService = heartbeatService;
            _logger = logger;
            _awsSettings = awsOptions.Value;
        }

        /// <summary>
        /// Create the helper object responsible for creating kinesis streams
        /// Note: These should be needed by Development / Staging only - Don't forget to delete them
        /// once you're finished with them.
        /// </summary>
        /// <returns></returns>
        public IKinesisStreamCreator CreateCreator()
        {
            var measurementsStreamName = _awsSettings.ProcessedMeasurementsStreamName;
            var rawStreamName = _awsSettings.RawMeasurementsStreamName;

            return new KinesisStreamCreator(_clientFactory, rawStreamName, measurementsStreamName, _logger);
        }

        /// <summary>
        /// Writer for processed measurements stream
        /// </summary>
        /// <returns></returns>
        public IProcessedMeasurementStreamWriter CreateWriter()
        {
            // Processed measurements go to this stream.
            var measurementsStreamName = _awsSettings.ProcessedMeasurementsStreamName;
            IAmazonKinesis client = _clientFactory.CreateKinesisClient();

            return new ProcessedMeasurementStreamWriter(client, measurementsStreamName, _logger);
        }

        /// <summary>
        /// Writer for raw (unprocessed) measurements stream
        /// Note: Not really needed here, only for handling legacy events from
        /// API/MQTT/Bots.
        /// </summary>
        /// <returns></returns>
        public IRawMeasurementStreamWriter CreateRawMeasurementStreamWriter()
        {
            // Processed measurements go to this stream.
            var streamName = _awsSettings.RawMeasurementsStreamName;
            var client = _clientFactory.CreateKinesisClient();

            return new RawMeasurementStreamWriter(client, streamName, _logger);
        }

        public IKinesisStreamReader CreateReader(IRecordProcessorFactory recordProcessorFactory,
            ICheckpointRepository checkpointRepository,
            IProcessedMeasurementStreamWriter processedMeasurementStreamWriter)
        {
            var streamName = _awsSettings.RawMeasurementsStreamName;
            var client = _clientFactory.CreateKinesisClient();

            RawMeasurementsShardReaderFactory factory = new RawMeasurementsShardReaderFactory(recordProcessorFactory,
                processedMeasurementStreamWriter,
                checkpointRepository,
                _heartbeatService, 
                _logger);

            return new KinesisStreamReader(client,
                streamName,
                "",
                factory,
                checkpointRepository,
                _logger);
        }

        public IKinesisStreamReader CreateProcesssedMeasurementsReader(
            ICheckpointRepository checkpointRepository,
            IBus eventBus)
        {
            var streamName = _awsSettings.ProcessedMeasurementsStreamName;
            var client = _clientFactory.CreateKinesisClient();

            ProcessedMeasurementsShardReaderFactory factory = new ProcessedMeasurementsShardReaderFactory(checkpointRepository,
                _heartbeatService,
                eventBus,
                _logger);

            return new KinesisStreamReader(client,
                streamName,
                "Measurements",
                factory,
                checkpointRepository,
                _logger);
        }

        //private IKinesisStreamReader CreateMonthlyProcesssedMeasurementsReader(
        //    ICheckpointRepository checkpointRepository,
        //    IHeartbeatService heartbeatService,
        //    IDynamoMonthlyMeasurementRepository monthlyMeasurementRepository)
        //{
        //    var streamName = _awsSettings.ProcessedMeasurementsStreamName;
        //    var client = _clientFactory.CreateKinesisClient();

        //    MonthlyProcessedMeasurementsShardReaderFactory factory = new MonthlyProcessedMeasurementsShardReaderFactory(checkpointRepository,
        //        heartbeatService,
        //        monthlyMeasurementRepository);

        //    return new KinesisStreamReader(client,
        //        streamName,
        //        "MonthlyMeasurements",
        //        factory,
        //        checkpointRepository);
        //}
    }
}