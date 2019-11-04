using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    /// <summary>
    /// Reads measurements on the Processsed Measurements Stream and puts
    /// them onto the EasynetQ bus for existing services that are not reading the stream
    /// to process
    /// </summary>
    public class ProcessedMeasurementsKinesisShardReader : KinesisShardReaderBase, IShardReader
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;
        private readonly XmlSerializer _deserializer = new XmlSerializer(typeof(ProcessedMeasurementEvent));

        public ProcessedMeasurementsKinesisShardReader(IAmazonKinesis client,
            ICheckpointRepository checkpointRepository,
            IHeartbeatService heartbeatService, 
            IBus bus,
            ILogger logger) 
            : base(client, checkpointRepository, heartbeatService)
        {
            _bus = bus;
            _logger = logger;
        }

        protected override void UpdateHeartbeatDelay(long responseMillisBehindLatest)
        {
            // Let the heartbeat service know how far behind we are.
            HeartbeatService.MonthlyProcessedStreamMillisBehindLatest = responseMillisBehindLatest;
            if (responseMillisBehindLatest > 10000)
            {
               _logger.LogWarning("Kinesis ProcessedMeasurements stream MillisBehindLatest as {0}", responseMillisBehindLatest);
            }
        }

        protected override async Task ProcessRecordsAsync(GetRecordsResponse recordsResponse, Checkpoint checkPoint)
        {
            await ProcessMeasurements(recordsResponse.Records, checkPoint);
        }

        private async Task ProcessMeasurements(List<Record> records, Checkpoint checkPoint)
        {
            List<ProcessedMeasurementEvent> processedMeasurementEvents = new List<ProcessedMeasurementEvent>();
            
            foreach (var record in records)
            {
                var processedMeasurementEvent = await ProcessMeasurement(record);

                if (processedMeasurementEvent != null)
                {
                    if (processedMeasurementEvents.All(x => x.Id != processedMeasurementEvent.Id))
                    {
                        processedMeasurementEvents.Add(processedMeasurementEvent);

                        await PublishProcessedMeasurementEvent(processedMeasurementEvent);
                    }
                    else
                    {
                        _logger.LogError("Duplicate measurement: {0} from device {1}", 
                            processedMeasurementEvent.Id,
                            processedMeasurementEvent.User.UserId);
                    }
                }
            }

            HeartbeatService.ProcessedMeasurementsProcessed += records.Count;

            checkPoint.SetCheckpoint(records.Last());
        }

        private async Task<ProcessedMeasurementEvent> ProcessMeasurement(Record record)
        {
            try
            {
                //Logger.Trace("Processing ProcessedMeasurement: {0} - PartitionKey: {1}", record.SequenceNumber , record.PartitionKey);
                return (ProcessedMeasurementEvent) _deserializer.Deserialize(record.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize processed measurement record data: " + record.Data);
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

        
        /// <summary>
        /// Publish the ProcessedMeasurementEvent from the Measurements service
        /// (i.e. the new event)
        /// </summary>
        private async Task PublishProcessedMeasurementEvent(ProcessedMeasurementEvent processedMeasurementEvent)
        {
            await _bus.PublishAsync(processedMeasurementEvent);
        }
    }
}