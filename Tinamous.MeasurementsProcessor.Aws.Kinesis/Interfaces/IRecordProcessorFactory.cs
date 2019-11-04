namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IRecordProcessorFactory
    {
        IRecordProcessor Create();
    }
}