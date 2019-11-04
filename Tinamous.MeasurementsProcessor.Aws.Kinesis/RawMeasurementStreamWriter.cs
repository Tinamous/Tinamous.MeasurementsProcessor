using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    public class RawMeasurementStreamWriter : IRawMeasurementStreamWriter
    {
        private readonly IAmazonKinesis _client;
        private readonly string _streamName;
        private readonly ILogger _logger;

        public RawMeasurementStreamWriter(IAmazonKinesis client, string streamName, ILogger logger)
        {
            _client = client;
            _streamName = streamName;
            _logger = logger;
        }

        public async Task PushStreamAsync(NewMeasurementEvent newMeasurementEvent)
        {
            await Push(newMeasurementEvent, newMeasurementEvent.AccountId);
        }

        private async Task Push(Object o, Guid partitionKey)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(o.GetType());
                serializer.Serialize(stream, o);

                stream.Position = 0;

                await PushStreamAsync($"RawMeasurement-Account-{partitionKey}", stream);
            }
        }
        
        private async Task PushStreamAsync(string partitionKey, MemoryStream stream)
        {
            try
            {
                var request = new PutRecordRequest
                {
                    Data = stream,
                    PartitionKey = partitionKey,
                    StreamName = _streamName
                };
                PutRecordResponse result = await _client.PutRecordAsync(request, CancellationToken.None);

                _logger.LogInformation("Put Sequence Number: {0}. ShardId: {1}", result.SequenceNumber, result.ShardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push to Kinesis stream");
                // Sink.
            }
        }
    }
}