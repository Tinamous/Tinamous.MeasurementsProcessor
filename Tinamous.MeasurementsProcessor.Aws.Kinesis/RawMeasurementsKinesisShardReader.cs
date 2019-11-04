using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    public class RawMeasurementsKinesisShardReader : KinesisShardReaderBase, IShardReader
    {
        private readonly XmlSerializer _deserializer = new XmlSerializer(typeof(NewMeasurementEvent));
        private readonly IProcessedMeasurementStreamWriter _processedMeasurementStreamWriter;
        private readonly IRecordProcessor _recordProcessor;
        private readonly ILogger _logger;

        public RawMeasurementsKinesisShardReader(IAmazonKinesis client, 
            ICheckpointRepository checkpointRepository, 
            IHeartbeatService heartbeatService,
            IProcessedMeasurementStreamWriter processedMeasurementStreamWriter,
            IRecordProcessor recordProcessor,
            ILogger logger) 
            : base(client, checkpointRepository, heartbeatService)
        {
            _processedMeasurementStreamWriter = processedMeasurementStreamWriter;
            _recordProcessor = recordProcessor;
            _logger = logger;
        }

        protected override void UpdateHeartbeatDelay(long responseMillisBehindLatest)
        {
            // Let the heartbeat service know how far behind we are.
            HeartbeatService.RawStreamMillisBehindLatest = responseMillisBehindLatest;
            if (responseMillisBehindLatest > 10000)
            {
                _logger.LogWarning("Kinesis RawMeasurements stream MillisBehindLatest as {0}", responseMillisBehindLatest);
            }
        }

        protected override async Task ProcessRecordsAsync(GetRecordsResponse recordsResponse, Checkpoint checkPoint)
        {
            await ProcessMeasurements(recordsResponse.Records, checkPoint);
        }

        private async Task ProcessMeasurements(List<Record> records, Checkpoint checkPoint)
        {
            foreach (var record in records)
            {
                var processedMeasurement = await ProcessMeasurement(record);

                if (processedMeasurement != null)
                {                    
                    // Push the processed records to Kinesis ProcessedMeasurements Stream
                    await PublishProcessedMeasurements(processedMeasurement);
                }

                checkPoint.SetCheckpoint(record);
            }

            HeartbeatService.RawRecordsProcessed += records.Count;
        }

        private async Task<ProcessedMeasurementEvent> ProcessMeasurement(Record record)
        {
            try
            {
                var measurement = GetMeasurement(record);

                // Ignore invalid measurement.
                if (measurement != null)
                {
                    return await _recordProcessor.ProcessAsync(measurement);
                }
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Error processing raw measurement");
                // Sink the exception
            }

            return null;
        }

        private async Task PublishProcessedMeasurements(ProcessedMeasurementEvent processedMeasurementEvent)
        {
            await _processedMeasurementStreamWriter.PushStreamAsync(processedMeasurementEvent);
        }

        private NewMeasurementEvent GetMeasurement(Record record)
        {
            try
            {
                return (NewMeasurementEvent) _deserializer.Deserialize(record.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize raw measurement record data: " + record.Data);
                LogSteamContents(record);

                // How best to handle this. Do we ignore or give up.
                // ignore and it's lost. Give up and we stop processing.
                // Could do with an error stream...
                // Throw for now, hopefully it's just some invalid text in the xml from the device
                // that is up to the device owner to sort out.
                // could do with some notification.
                return null;
            }
        }
    }
}