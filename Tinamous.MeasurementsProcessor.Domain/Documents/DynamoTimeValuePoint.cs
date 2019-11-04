namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Store chart points as time and value.
    /// Can't use arrays as DynamoDB appears to swap the order of them randomly.
    /// </summary>
    /// <remarks>
    /// T and V property names as there will be 500 of them and that adds up to a lot of bytes and 
    /// effects DynamoDB throughput
    /// </remarks>
    public class DynamoTimeValuePoint
    {
        /// <summary>
        /// Time offset (in seconds) since the parent objects ForDate
        /// or the offset in hours for hourly measurement count
        /// or the offset in minutes for minutly measurement counts
        /// </summary>
        public decimal T { get; set; }

        public decimal V { get; set; }
    }
}