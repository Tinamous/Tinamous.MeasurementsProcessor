using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// A summary document containing the latest measurements (per field)
    /// and the date of the most recent measurement.
    ///
    /// This may be made from multiple measurements, each with it's own different
    /// fields.
    /// </summary>
    /// <remarks>
    /// Note: This ignores the channel as well.
    ///
    /// </remarks>
    [DynamoDBTable("Measurement.LatestMeasurement")]
    [DebuggerDisplay("Device: {UserId}, Date: {Date}")]
    public class DynamoLatestMeasurement
    {
        /// <summary>
        /// The user (member/device/bot) id.
        /// </summary>
        /// <remarks>
        /// This is the hash key for the table. Only one record per device.
        /// </remarks>
        [DynamoDBHashKey]
        public Guid UserId { get; set; }

        /// <summary>
        /// Last measurement date.
        /// </summary>
        /// <remarks>
        /// Fields may have different date/times based on the measurement
        /// they come from.
        /// </remarks>
        public DateTime Date { get; set; }

        /// <summary>
        /// If of the last measurement
        /// </summary>
        public Guid Id { get; set; }

        public List<DynamoSummaryMeasurementField> Fields;

        public DynamoCompactLocation Location { get; set; }

        /// <summary>
        /// Raw battery level (may not be %)
        /// </summary>
        public decimal? Batt { get; set; }

        /// <summary>
        /// Battery level when converted to %
        /// </summary>
        public int? BattPc { get; set; }

        /// <summary>
        /// Actually raw RfStrength as db. - can probably rename with out a problem.....
        /// </summary>
        public int? RfPc { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }

        
    }
}