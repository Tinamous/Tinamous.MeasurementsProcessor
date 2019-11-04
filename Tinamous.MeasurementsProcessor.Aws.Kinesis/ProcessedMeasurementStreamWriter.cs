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
    public class ProcessedMeasurementStreamWriter : IProcessedMeasurementStreamWriter
    {
        private readonly IAmazonKinesis _client;
        private readonly string _streamName;
        private readonly ILogger _logger;

        public ProcessedMeasurementStreamWriter(IAmazonKinesis client, string streamName, ILogger logger)
        {
            _client = client;
            _streamName = streamName;
            _logger = logger;
        }

        public async Task PushStreamAsync(ProcessedMeasurementEvent processedMeasurementEvent)
        {
            await Push(processedMeasurementEvent, processedMeasurementEvent.User.AccountId);
        }

        private async Task Push(Object o, Guid partitionKey)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(o.GetType());
                serializer.Serialize(stream, o);

                stream.Position = 0;

                await PushStreamAsync($"ProcessedMeasurement-Account-{partitionKey}", stream);
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
                await _client.PutRecordAsync(request, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push to Kinesis stream");
                // Sink.
            }
        
        }


    }
}