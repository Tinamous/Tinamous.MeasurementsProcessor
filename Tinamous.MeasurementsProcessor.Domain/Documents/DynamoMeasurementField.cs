using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoMeasurementField
    {
        public string Name { get; set; }
        public DateTime? Time { get; set; }
        public string Unit { get; set; }
        public decimal? Value { get; set; }
        public decimal? RawValue { get; set; }
        public string StringValue { get; set; }
        public bool? BooleanValue { get; set; }
        public decimal? Sum { get; set; }

        /// <summary>
        /// If this field is a computed (virtual) field.
        /// </summary>
        public bool IsComputed { get; set; }
        public bool IsCalibrated { get; set; }
    }
}