using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AnalysisUK.Tinamous.Messaging.Common.Dtos.System;
using AnalysisUK.Tinamous.Messaging.Common.Events.System;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Settings;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services
{
    public class HeartbeatService : IHeartbeatService
    {
        private readonly ILogger<HeartbeatService> _logger;
        private readonly IBus _bus;
        private Timer _timer;
        private const int TimerIntervalSeconds = 10;
        private bool _enabled;
        private int _dynamoDBRecordsSaved;
        private int _dynamoDBMonthlyRecordsSaved;
        private readonly ServerSettings _serverSettings;

        public HeartbeatService(IBusFactory busFactory, 
            IOptions<ServerSettings> serverOptions,
            ILogger<HeartbeatService> logger)
        {
            _logger = logger;
            _bus = busFactory.CreateEventsBus();
            _serverSettings = serverOptions.Value;

            _timer = new Timer(OnTimerTick, null, TimerIntervalSeconds * 1000, TimerIntervalSeconds * 1000);
        }

        public long RawStreamMillisBehindLatest { get; set; }
        public int RawRecordsProcessed { get; set; }

        public long ProcessedStreamMillisBehindLatest { get; set; }
        public int ProcessedMeasurementsProcessed { get; set; }

        public long MonthlyProcessedStreamMillisBehindLatest { get; set; }
        public int MonthlyProcessedMeasurementsProcessed { get; set; }

        public void IncrementSavedToDynamoDB()
        {
            _dynamoDBRecordsSaved++;
        }

        public void IncrementSavedToDynamoDBMonthly()
        {
            _dynamoDBMonthlyRecordsSaved++;
        }

        private void OnTimerTick(object state)
        {
            if (!_enabled)
            {
                return;
            }

            ProcessInfo processInfo = GetProcessInfo();

            try
            {
                // Publish a heartbeat for this service.
                var heartBeatEvent = new HeartBeatEvent
                {
                    IntervalSeconds = TimerIntervalSeconds,
                    Server = _serverSettings.ServerName,
                    Service = _serverSettings.ServiceName,
                    SoftwareVersion = _serverSettings.Version,
                    Time = DateTime.UtcNow,
                    IsMaster = _serverSettings.IsPrimary,
                    ProcessInfo = processInfo,
                    MachineInfo = new MachineInfo(),
                    MetaData = "Delay (ms): " + RawStreamMillisBehindLatest,
                    RecordsProcessed = new List<RecordsProcessed>
                    {
                        new RecordsProcessed {Name="RawStreamMillisBehindLatest", Value = Convert.ToInt32(RawStreamMillisBehindLatest)},
                        new RecordsProcessed {Name="RawRecordsProcessed", Value = RawRecordsProcessed},

                        new RecordsProcessed {Name="ProcessedStreamMillisBehindLatest", Value = Convert.ToInt32(ProcessedStreamMillisBehindLatest)},
                        new RecordsProcessed {Name="ProcessedMeasurementsProcesssed", Value = ProcessedMeasurementsProcessed},

                        new RecordsProcessed {Name="MonthlyProcessedStreamMillisBehindLatest", Value = Convert.ToInt32(MonthlyProcessedStreamMillisBehindLatest)},
                        new RecordsProcessed {Name="MonthlyProcessedMeasurementsProcesssed", Value = MonthlyProcessedMeasurementsProcessed},


                        new RecordsProcessed {Name="DynamoRecordsProcessed", Value = _dynamoDBRecordsSaved},
                        new RecordsProcessed {Name="DynamoMonthlyRecordsProcessed", Value = _dynamoDBMonthlyRecordsSaved},
                    }
                };
                _bus.Publish(heartBeatEvent);

                // Reset the records processed.
                RawRecordsProcessed = 0;
                ProcessedMeasurementsProcessed = 0;
                _dynamoDBRecordsSaved = 0;
                _dynamoDBMonthlyRecordsSaved = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing heartbeat. Is the RabbitMQ service available?");
            }
        }

        private ProcessInfo GetProcessInfo()
        {
            try
            {
                Process process = Process.GetCurrentProcess();
                long memory = process.PrivateMemorySize64;
                //long memory = process.TotalProcessorTime
                return new ProcessInfo { PrivateMemorySize64 = memory };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to get process info. Error: {0}", ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Start()
        {
            _enabled = true;
        }

        public void Stop()
        {
            _enabled = false;
        }
    }
}