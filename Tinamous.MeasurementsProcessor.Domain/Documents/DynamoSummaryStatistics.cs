namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoSummaryStatistics
    {
        public int Count { get; set; }
        public decimal Average { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public double StandardDeviation { get; set; }
        public decimal Sum { get; set; }
    }
}