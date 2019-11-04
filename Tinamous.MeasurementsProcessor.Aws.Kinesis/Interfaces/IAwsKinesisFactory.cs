using EasyNetQ;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IAwsKinesisFactory
    {
        IKinesisStreamCreator CreateCreator();
        IProcessedMeasurementStreamWriter CreateWriter();
        IRawMeasurementStreamWriter CreateRawMeasurementStreamWriter();

        IKinesisStreamReader CreateReader(IRecordProcessorFactory recordProcessorFactory,
            ICheckpointRepository checkpointRepository,
            IProcessedMeasurementStreamWriter processedMeasurementStreamWriter);

        IKinesisStreamReader CreateProcesssedMeasurementsReader(
            ICheckpointRepository checkpointRepository,
            IBus eventBus);
    }
}