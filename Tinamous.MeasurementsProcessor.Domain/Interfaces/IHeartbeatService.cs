using System;

namespace Tinamous.MeasurementsProcessor.Domain.Interfaces
{
    public interface IHeartbeatService : IDisposable
    {
        long RawStreamMillisBehindLatest { get; set; }
        int RawRecordsProcessed { get; set; }

        int ProcessedMeasurementsProcessed { get; set; }
        long ProcessedStreamMillisBehindLatest { get; set; }

        int MonthlyProcessedMeasurementsProcessed { get; set; }
        long MonthlyProcessedStreamMillisBehindLatest { get; set; }

        void IncrementSavedToDynamoDB();
        void IncrementSavedToDynamoDBMonthly();

        void Start();
        void Stop();
    }
}