namespace Tinamous.MeasurementsProcessor.Domain.Settings
{
    public class AwsSettings
    {
        public string ProfileName { get; set; }
        
        public string Region { get; set; }

        public string RawMeasurementsStreamName { get; set; }

        public string ProcessedMeasurementsStreamName { get; set; }
        
        /// <summary>
        /// Table prefix for DynamoDB tables.
        /// </summary>
        public string DynamoDbTablePrefix { get; set; }
    }
}