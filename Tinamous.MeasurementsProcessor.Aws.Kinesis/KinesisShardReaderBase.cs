using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;
using ShardIteratorType = Amazon.Kinesis.ShardIteratorType;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    public abstract class KinesisShardReaderBase
    {
        private readonly IAmazonKinesis _client;
        
        // Delay between reads to ensure we are under kinesis limits
        private int _readDelay = 500;

        public KinesisShardReaderBase(IAmazonKinesis client, 
            ICheckpointRepository checkpointRepository, 
            IHeartbeatService heartbeatService)
        {
            _client = client;
            CheckpointRepository = checkpointRepository;
            HeartbeatService = heartbeatService;
        }

        protected ICheckpointRepository CheckpointRepository { get; private set; }

        protected IHeartbeatService HeartbeatService { get; private set; }

        public async Task ProcessShardAsync(Shard shard, Checkpoint checkPoint, CancellationToken token)
        {
            if (shard == null) throw new ArgumentNullException("shard");
            if (checkPoint == null) throw new ArgumentNullException("checkPoint");

            var iteratorResponse = await GetShardIteratorAsync(checkPoint);
            string iterator = iteratorResponse.ShardIterator;

            try
            {
                await ItterateAsync(shard, checkPoint, token, iterator);
            }
            catch (OperationCanceledException)
            {
                // Task has been cancelled (probably in a delay).
                // our work here is done....
                //_logger.LogInformation("OperationCanceledException in ShardReaderBase.");
            }
            catch (AggregateException ex)
            {
                //_logger.LogError(ex, "Shard itteration through exception.");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Shard itteration through exception.");
            }
            finally
            {
                //_logger.LogWarning("Shard reader for shard: {0} has finished.", shard);
            }
        }

        private async Task ItterateAsync(Shard shard, Checkpoint checkPoint, CancellationToken token, string iterator)
        {
            while (iterator != null)
            {
                try
                {
                    iterator = await TryProcessRecordsAsync(checkPoint, iterator, token);
                }
                catch (ProvisionedThroughputExceededException ex)
                {
                    //_logger.LogError(ex,
                    //    "Provisioned throughput exceeded. Sleep for 5 seconds to help things. ReadDelay: " +
                    //    _readDelay.ToString());

                    Task.Delay(TimeSpan.FromMilliseconds(5000), token).Wait(token);
                    _readDelay += 250;
                }
                catch (ExpiredIteratorException ex)
                {
                   // _logger.LogError(ex, "Expired Iterator for shard: " + shard.ShardId);

                    // Get a new iterator to replace the expired one
                    var iteratorResponse = await GetShardIteratorAsync(checkPoint);
                    iterator = iteratorResponse.ShardIterator;
                }
                catch (Exception ex)
                {
                   // _logger.LogError(ex, "Unexpected exception processing shard: " + shard.ShardId);
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }

                Trace.WriteLine("Next Iterator for Shard: " + shard.ShardId + " = " + iterator);
                await Task.Delay(TimeSpan.FromMilliseconds(250), token);

                Trace.WriteLine("Next Iterator for Shard: " + shard.ShardId + " = " + iterator);
            }
        }

        private async Task<string> TryProcessRecordsAsync(Checkpoint checkPoint, string iterator, CancellationToken token)
        {
            GetRecordsRequest request = new GetRecordsRequest
            {
                Limit = 250,
                ShardIterator = iterator,
            };
            GetRecordsResponse response = await _client.GetRecordsAsync(request, token);

            UpdateHeartbeatDelay(response.MillisBehindLatest);

            if (response.Records.Any())
            {
                ProcessRecordsAsync(response, checkPoint).Wait();
                CheckpointRepository.Save(checkPoint);
            }

            if (response.Records.Count < 10)
            {
                Trace.WriteLine("Sleeping as no new data.");
                Task.Delay(1000, token).Wait(token);
            }
            else
            {
              //  _logger.LogInformation("Got {0} records from Kinesis", response.Records.Count);   
            }

            iterator = response.NextShardIterator;

            return iterator;
        }

        protected abstract void UpdateHeartbeatDelay(long responseMillisBehindLatest);

        protected abstract Task ProcessRecordsAsync(GetRecordsResponse recordsResponse, Checkpoint checkPoint);

        private async Task<GetShardIteratorResponse> GetShardIteratorAsync(Checkpoint checkPoint)
        {
            GetShardIteratorRequest iteratorRequest = new GetShardIteratorRequest
            {
                StreamName = checkPoint.StreamName,
                ShardId = checkPoint.ShardId,
                ShardIteratorType = ShardIteratorType.AT_SEQUENCE_NUMBER,
                StartingSequenceNumber = checkPoint.LastSequenceNumber,
            };

            GetShardIteratorResponse iteratorResponse = await _client.GetShardIteratorAsync(iteratorRequest);
            return iteratorResponse;
        }

        protected void LogSteamContents(Record record)
        {
            try
            {
                record.Data.Position = 0;
                StreamReader reader = new StreamReader(record.Data);
                string streamContents = reader.ReadToEnd();
              //  _logger.LogError("Stream contents: {0}", streamContents);
                record.Data.Position = 0;
                reader.Dispose();
            }
            catch (Exception ex)
            {
             //   _logger.LogError(ex, "Failed to log stream contents");
            }
        }

        public void Dispose()
        {
        }
    }
}