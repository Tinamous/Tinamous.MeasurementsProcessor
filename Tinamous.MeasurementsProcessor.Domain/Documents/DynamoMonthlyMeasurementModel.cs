using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Represents a measurement to be stored in the monthly
    /// measurement table.
    /// 
    /// Copy of DynamoMeasurementModel, but a little more compact to save space
    /// </summary>
    [DynamoDBTable("Measurement.ByMonth")]
    [DebuggerDisplay("Device: {DeviceId}, DateIdKey: {DateIdKey}")]
    public class DynamoMonthlyMeasurementModel
    {
        public DynamoMonthlyMeasurementModel()
        { }

        public DynamoMonthlyMeasurementModel(Guid id, DateTime date)
        {
            Id = id;
            DateIdKey = string.Format("{0}#{1}", date.ToUniversalTime().ToString("O"), id);
        }

        /// <summary>
        /// Unique Id.
        /// NB: To Delete or get by Id, use table scan (unless it becomes
        /// common when add an index)
        /// </summary>
        [DynamoDBGlobalSecondaryIndexHashKey("Id-index")]
        public Guid Id { get; set; }

        [DynamoDBHashKey]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Range key for DynamoDB. Includes the Id as a suffix
        /// to prevent two measurements from a device at identical times
        /// from clashing.
        /// </summary>
        [DynamoDBRangeKey]
        public string DateIdKey { get; set; }

        public DateTime GetDate()
        {
            var dateKey = DateIdKey.Split('#');
            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.Parse(dateKey[0], provider, DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// SampleId
        /// </summary>
        public string SId { get; set; }

        /// <summary>
        /// Channel
        /// </summary>
        public int Chan { get; set; }

        public List<DynamoCompactMeasurementField> Fields { get; set; }

        public DynamoCompactLocation Location { get; set; }

        public List<string> Tags { get; set; }

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

        /// <summary>
        /// DymnamoDB Automatic expiry
        /// </summary>
        public long? DeleteAfter { get; set; }

        /// <summary>
        /// When the measurement is deemed to be expired (See TTL).
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The original source of the measurement.
        /// </summary>
        public int Source { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }
    }
}