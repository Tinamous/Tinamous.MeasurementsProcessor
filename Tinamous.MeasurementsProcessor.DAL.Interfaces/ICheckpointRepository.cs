using System.Threading.Tasks;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.DAL.Interfaces
{
    public interface ICheckpointRepository
    {
        Checkpoint Load(string shardShardId, string rawMeasurementsStreamName, string name);
        void Save(Checkpoint checkpoint);
        Task CreateTableAsync();
    }
}