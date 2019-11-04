using System;
using System.Diagnostics;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Represents a single measurement field/sensor reading in a measurement.
    /// </summary>
    [DebuggerDisplay("Field: {Name}, Value: {Value} {Unit}")]
    public class MeasurementField
    {
        public string Name { get; set; }

        /// <summary>
        /// Field decimal value. If this field has been calibrated this is the calibrated value.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Raw original decimal value. If this field has been calibrated this is the original value
        /// </summary>
        public decimal? RawValue { get; set; }

        /// <summary>
        /// String representation of the field value. 
        /// May not be numeric
        /// If field is calibrated this will hold the raw value.
        /// </summary>
        public string StringValue { get; set; }

        public bool? BoolValue { get; set; }

        public decimal? Sum { get; set; }

        public string Unit { get; set; }

        public DateTime? Time { get; set; }

        /// <summary>
        /// If this field is a computed (virtual) field.
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// If this field has been calibrated
        /// </summary>
        public bool IsCalibrated { get; set; }
    }
}