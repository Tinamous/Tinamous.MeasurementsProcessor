using System.Linq;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Helpers
{
    public class KinesisStreamCreator : IKinesisStreamCreator
    {
        private readonly IAmazonKinesis _client;
        private readonly string _rawStreamName;
        private readonly string _measurementsStreamName;
        private readonly ILogger _logger;

        public KinesisStreamCreator(IAwsClientFactory clientFactory, string rawStreamName, string measurementsStreamName, ILogger logger)
        {
            _client = clientFactory.CreateKinesisClient();
            _rawStreamName = rawStreamName;
            _measurementsStreamName = measurementsStreamName;
            _logger = logger;
        }

        public async Task CreateStreamsAsync()
        {
            _logger.LogInformation("Creating Kinesis Streams...");

            var response = await _client.ListStreamsAsync();

            if (response.StreamNames.Any())
            {
                foreach (var responseStreamName in response.StreamNames)
                {
                    _logger.LogInformation("Stream {0} already exists.", responseStreamName);
                }
            }

            if (!response.StreamNames.Contains(_rawStreamName))
            {
                await CreateRawStreamAsync();
            }

            if (!response.StreamNames.Contains(_measurementsStreamName))
            {
                await CreateMeasurementsStreamAsync();
            }

            _logger.LogInformation("Done Creating Kinesis Streams...");
        }

        private async Task CreateRawStreamAsync()
        {
            _logger.LogWarning("Creating Raw Measurements Stream: {0}", _rawStreamName);
            CreateStreamRequest request = new CreateStreamRequest
            {
                ShardCount = 1,
                StreamName = _rawStreamName
            };
            await _client.CreateStreamAsync(request);
        }

        private async Task CreateMeasurementsStreamAsync()
        {
            _logger.LogWarning("Creating Measurements Stream: {0}", _measurementsStreamName);
            CreateStreamRequest request = new CreateStreamRequest
            {
                ShardCount = 1,
                StreamName = _measurementsStreamName
            };
            await _client.CreateStreamAsync(request);
        }
    }
}