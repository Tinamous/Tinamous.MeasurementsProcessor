using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.Services.Interfaces;
using Tinamous.MeasurementsProcessor.Services.Processors;

namespace Tinamous.MeasurementsProcessor.Services.RecordProcessors
{
    public class RecordProcessorFactory : IRecordProcessorFactory
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger _logger;

        public RecordProcessorFactory(IMembershipService membershipService, ILogger logger)
        {
            _membershipService = membershipService;
            _logger = logger;
        }

        public IRecordProcessor Create()
        {
            return new NewMeasurementRecordProcessor(_membershipService, _logger);
        }
    }
}