using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoSummaryMeasurementField
    {
        public string N { get; set; }
        public string U { get; set; }
        public decimal? V { get; set; }
        public decimal? RV { get; set; }
        public string SV { get; set; }
        public bool? BV { get; set; }
        public decimal? Sum { get; set; }

        /// <summary>
        /// Measurement date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Channel identifier.
        /// </summary>
        public int Chan { get; set; }
    }
}