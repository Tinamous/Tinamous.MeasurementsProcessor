using System;
using System.Threading.Tasks;

namespace Tinamous.MeasurementsProcessor.Services.Interfaces
{
    public interface IStreamProcessor : IDisposable
    {
        Task CreateTables();
        Task CreateKinesisStreamsAsync();
        void SetupProcessors();

        void SetupEventWatchers();
    }
}