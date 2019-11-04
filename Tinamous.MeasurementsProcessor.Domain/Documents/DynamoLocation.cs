using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoLocation
    {
        public double Elevation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}