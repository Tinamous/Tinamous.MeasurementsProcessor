using System.Threading.Tasks;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IKinesisStreamCreator
    {
        Task CreateStreamsAsync();
    }
}