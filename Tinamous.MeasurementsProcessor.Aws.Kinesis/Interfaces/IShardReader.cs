using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis.Model;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IShardReader : IDisposable
    {
        Task ProcessShardAsync(Shard shard, Checkpoint checkPoint, CancellationToken token);
    }
}