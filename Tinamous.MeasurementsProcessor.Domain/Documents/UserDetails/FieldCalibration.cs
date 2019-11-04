using System.Diagnostics;

namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    [DebuggerDisplay("Slope: {Slope}, Offset: {Offset}")]
    public class FieldCalibration
    {
        private decimal _slope = 1.0M;

        /// <summary>
        /// If this field should have a calibration applied to it.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Calibration offset (c in y = mx + c)
        /// </summary>
        public decimal Offset { get; set; }

        /// <summary>
        /// Calibration slopt (m in y = mx + c)
        /// </summary>
        public decimal Slope
        {
            get { return _slope; }
            set { _slope = value; }
        }
    }
}