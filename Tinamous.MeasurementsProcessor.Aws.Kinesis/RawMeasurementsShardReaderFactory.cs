using Amazon.Kinesis;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    public class RawMeasurementsShardReaderFactory : IShardReaderFactory
    {
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IHeartbeatService _heartbeatService;
        private readonly ILogger _logger;
        private readonly IRecordProcessorFactory _recordProcessorFactory;
        private readonly IProcessedMeasurementStreamWriter _processedMeasurementStreamWriter;

        public RawMeasurementsShardReaderFactory(IRecordProcessorFactory recordProcessorFactory,
            IProcessedMeasurementStreamWriter processedMeasurementStreamWriter,
            ICheckpointRepository checkpointRepository,
            IHeartbeatService heartbeatService,
            ILogger logger)
        {
            _recordProcessorFactory = recordProcessorFactory;
            _processedMeasurementStreamWriter = processedMeasurementStreamWriter;
            _checkpointRepository = checkpointRepository;
            _heartbeatService = heartbeatService;
            _logger = logger;
        }

        public IShardReader Create(IAmazonKinesis client)
        {
            var recordProcessor = _recordProcessorFactory.Create();

            return new RawMeasurementsKinesisShardReader(client, 
                _checkpointRepository, 
                _heartbeatService, 
                _processedMeasurementStreamWriter, 
                recordProcessor,
                _logger);
        }
    }
}