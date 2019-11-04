using Amazon.Kinesis;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IShardReaderFactory
    {
        IShardReader Create(IAmazonKinesis client);
    }
}