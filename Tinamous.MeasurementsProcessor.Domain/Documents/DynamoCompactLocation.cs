using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoCompactLocation
    {
        public double E { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
    }
}