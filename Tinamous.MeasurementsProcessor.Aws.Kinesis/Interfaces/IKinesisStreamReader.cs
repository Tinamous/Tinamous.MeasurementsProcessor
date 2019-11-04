using System;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IKinesisStreamReader : IDisposable
    {
        bool Enabled { get; }
        void Start();
        void Stop();
    }
}