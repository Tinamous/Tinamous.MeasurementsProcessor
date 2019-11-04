using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces
{
    public interface IRawMeasurementStreamWriter
    {
        Task PushStreamAsync(NewMeasurementEvent newMeasurementEvent);
    }
}