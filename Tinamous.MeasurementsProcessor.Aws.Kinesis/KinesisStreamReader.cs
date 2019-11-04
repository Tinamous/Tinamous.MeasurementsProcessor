using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    public class KinesisStreamReader : IKinesisStreamReader
    {
        private readonly IAmazonKinesis _client;

        private readonly string _streamName;
        private readonly string _readerName;

        private readonly IShardReaderFactory _shardReaderFactory;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly List<IShardReader> _shardReaders = new List<IShardReader>();
        private List<Task> _shardTasks = new List<Task>();


        public KinesisStreamReader(IAmazonKinesis client,
            string streamName,
            string readerName,
            IShardReaderFactory shardReaderFactory,
            ICheckpointRepository checkpointRepository,
            ILogger logger)
        {
            _client = client;
            _streamName = streamName;
            _readerName = readerName;
            _shardReaderFactory = shardReaderFactory;
            _checkpointRepository = checkpointRepository;
            _logger = logger;
        }

        /// <summary>
        /// If client settings have the Kinesis publishing enabled. (Needs to be implemented outside of this).
        /// </summary>
        public bool Enabled { get; set; }

        public void Start()
        {
            DescribeStreamRequest describeStreamRequest = new DescribeStreamRequest
            {
                Limit = 10,
                StreamName = _streamName
            };
            var streamDescription = _client.DescribeStreamAsync(describeStreamRequest).Result;

            var shards = streamDescription.StreamDescription.Shards;
            _logger.LogInformation("Found {0} shards for stream {1}", shards.Count, _streamName);

            _shardReaders.Clear();

            foreach (Shard shard in shards)
            {
                StartReadingShart(shard);
            }
        }

        private void StartReadingShart(Shard shard)
        {
            Shard toProcess = shard;

            _logger.LogInformation("Starting shard processor for shard: {0}", toProcess.ShardId);

            Checkpoint checkPoint = GetCheckpoint(toProcess);
            IShardReader shardReader = _shardReaderFactory.Create(_client);
            _shardReaders.Add(shardReader);

            _shardTasks.Add(Task.Run(() => 
                shardReader.ProcessShardAsync(toProcess, checkPoint, _cancellationTokenSource.Token)));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();

            Task.WaitAll(_shardTasks.ToArray(), TimeSpan.FromSeconds(20));
            _shardTasks.Clear();
        }

        private Checkpoint GetCheckpoint(Shard shard)
        {
            _logger.LogInformation("Loading checkpoint for shard: {0}", shard.ShardId);
            Checkpoint checkPoint = _checkpointRepository.Load(shard.ShardId, _streamName, _readerName);

            if (checkPoint == null)
            {
                _logger.LogWarning("No checkpoint for shard: {0}. Creating one.", shard.ShardId);
                checkPoint = new Checkpoint(shard, _streamName, _readerName);
                _checkpointRepository.Save(checkPoint);
            }

            return checkPoint;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _cancellationTokenSource?.Dispose();

            if (_shardReaders != null)
            {
                foreach (IShardReader kinesisShardReader in _shardReaders)
                {
                    kinesisShardReader.Dispose();
                }
                _shardReaders.Clear();
            }
        }
    }
}
