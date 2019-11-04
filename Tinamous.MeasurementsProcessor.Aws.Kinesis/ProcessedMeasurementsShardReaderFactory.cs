using Amazon.Kinesis;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    /// <summary>
    /// Shard reader for Processed measurements Kinesis stream.
    /// </summary>
    public class ProcessedMeasurementsShardReaderFactory : IShardReaderFactory
    {
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IHeartbeatService _heartbeatService;       
        private readonly IBus _bus;
        private readonly ILogger _logger;

        public ProcessedMeasurementsShardReaderFactory(ICheckpointRepository checkpointRepository,
            IHeartbeatService heartbeatService, 
            IBus bus,
            ILogger logger)
        {
            _checkpointRepository = checkpointRepository;
            _heartbeatService = heartbeatService;
            _bus = bus;
            _logger = logger;
        }

        public IShardReader Create(IAmazonKinesis client)
        {
            return new ProcessedMeasurementsKinesisShardReader(client,
                _checkpointRepository, 
                _heartbeatService,
                _bus,
                _logger);
        }
    }
}