namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoCompactMeasurementField
    {
        public string N { get; set; }
        public string U { get; set; }
        public decimal? V { get; set; }
        public decimal? RV { get; set; }
        public string SV { get; set; }
        public bool? BV { get; set; }
        public decimal? Sum { get; set; }
    }
}