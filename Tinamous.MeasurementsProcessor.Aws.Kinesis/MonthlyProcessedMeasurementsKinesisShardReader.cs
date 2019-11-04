namespace Tinamous.MeasurementsProcessor.Aws.Kinesis { }
//{
//    public class MonthlyProcessedMeasurementsKinesisShardReader : KinesisShardReaderBase, IShardReader
//    {
//        private readonly IDynamoMonthlyMeasurementRepository _monthlyMeasurementRepository;
//        private readonly XmlSerializer _deserializer = new XmlSerializer(typeof(ProcessedMeasurementEvent));

//        public MonthlyProcessedMeasurementsKinesisShardReader(IAmazonKinesis client,
//            ICheckpointRepository checkpointRepository,
//            IHeartbeatService heartbeatService, 
//            IDynamoMonthlyMeasurementRepository monthlyMeasurementRepository) 
//            : base(client, checkpointRepository, heartbeatService)
//        {
//            _monthlyMeasurementRepository = monthlyMeasurementRepository;
//        }
//        protected override void UpdateHeartbeatDelay(long responseMillisBehindLatest)
//        {
//            // Let the heartbeat service know how far behind we are.
//            HeartbeatService.ProcessedStreamMillisBehindLatest = responseMillisBehindLatest;
//            if (responseMillisBehindLatest > 10000)
//            {
//                _logger.LogWarning("Kinesis ProcessedMeasurements stream MillisBehindLatest as {0}", responseMillisBehindLatest);
//            }
//        }

//        protected override async Task ProcessRecordsAsync(GetRecordsResponse recordsResponse, Checkpoint checkPoint)
//        {
//            await ProcessMeasurements(recordsResponse.Records, checkPoint);
//        }

//        private async Task ProcessMeasurements(List<Record> records, Checkpoint checkPoint)
//        {
//            List<ProcessedMeasurementEvent> processedMeasurementEvents = new List<ProcessedMeasurementEvent>();
            
//            foreach (var record in records)
//            {
//                var processedMeasurementEvent = await ProcessMeasurement(record);

//                if (processedMeasurementEvent != null)
//                {
//                    if (processedMeasurementEvents.All(x => x.Id != processedMeasurementEvent.Id))
//                    {
//                        processedMeasurementEvents.Add(processedMeasurementEvent);
//                    }
//                    else
//                    {
//                        _logger.LogError("Duplicate measurement: {0} from device {1}", 
//                            processedMeasurementEvent.Id,
//                            processedMeasurementEvent.User.UserId);
//                    }
//                }
//            }

//            if (processedMeasurementEvents.Any())
//            {
//                await SaveMonthlyMeasurements(processedMeasurementEvents);
//            }

//            HeartbeatService.MonthlyProcessedMeasurementsProcessed += records.Count;

//            checkPoint.SetCheckpoint(records.Last());
//        }

//        private async Task SaveMonthlyMeasurements(List<ProcessedMeasurementEvent> processedMeasurementEvents)
//        {
//            List<DynamoMonthlyMeasurementModel> monthlyMeasurements =  DynamoMonthlyMeasurementMapper.Map(processedMeasurementEvents);

//            Logger.Trace("Saving {0} measurements to monthly table", monthlyMeasurements.Count);
//            await _monthlyMeasurementRepository.SaveAsync(monthlyMeasurements);
//        }

//        private async Task<ProcessedMeasurementEvent> ProcessMeasurement(Record record)
//        {
//            try
//            {
//                return (ProcessedMeasurementEvent) _deserializer.Deserialize(record.Data);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to deserialize processed measurement record data: " + record.Data);
//                LogSteamContents(record);

//                // How best to handle this. Do we ignore or give up.
//                // ignore and it's lost. Give up and we stop processing.
//                // Could do with an error stream...
//                // Throw for now, hopefully it's just some invalid text in the xml from the device
//                // that is up to the device owner to sort out.
//                // could do with some notification.
//                return null;
//            }
//        }
//    }
//}