namespace Tinamous.MeasurementsProcessor.Aws.Kinesis
{
    ///// <summary>
    ///// Shard reader for Processed measurements Kinesis stream.
    ///// </summary>
    //public class MonthlyProcessedMeasurementsShardReaderFactory : IShardReaderFactory
    //{
    //    private readonly ICheckpointRepository _checkpointRepository;
    //    private readonly IHeartbeatService _heartbeatService;
    //    private readonly IDynamoMonthlyMeasurementRepository _monthlyMeasurementRepository;

    //    public MonthlyProcessedMeasurementsShardReaderFactory (ICheckpointRepository checkpointRepository,
    //        IHeartbeatService heartbeatService, 
    //        IDynamoMonthlyMeasurementRepository monthlyMeasurementRepository)
    //    {
    //        _checkpointRepository = checkpointRepository;
    //        _heartbeatService = heartbeatService;
    //        _monthlyMeasurementRepository = monthlyMeasurementRepository;
    //    }

    //    public IShardReader Create(IAmazonKinesis client)
    //    {
    //        return new MonthlyProcessedMeasurementsKinesisShardReader(client,
    //            _checkpointRepository, 
    //            _heartbeatService,
    //            _monthlyMeasurementRepository);
    //    }
    //}
}