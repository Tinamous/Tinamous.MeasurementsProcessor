using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    [DynamoDBTable("Measurement")]
    public class DynamoMeasurementModel
    {
        private Guid _id = Guid.NewGuid();

        // primary Index
        [DynamoDBHashKey]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
       
        public Guid AccountId { get; set; }

        // Secondary Index
        [DynamoDBGlobalSecondaryIndexHashKey("DeviceId-MeasurementDate-index")]
        public Guid DeviceId { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey(new[] { "DeviceId-MeasurementDate-index" })]
        public DateTime MeasurementDate { get; set; }
       
        public string SampleId { get; set; }

        [Obsolete("Not in Monthly")]
        public string OperatorId { get; set; }

        public int Channel { get; set; }

        public DateTime PostedOn { get; set; }

        public DateTime DateAdded { get; set; }

        public List<DynamoMeasurementField> MeasurementFields { get; set; }

        public DynamoLocation Location { get; set; }

        public List<string> Tags { get; set; }

        /// <summary>
        /// Raw battery level (may not be %)
        /// </summary>
        public decimal? BatteryLevel { get; set; }

        /// <summary>
        /// Battery level when converted to %
        /// </summary>
        public int? BatteryLevelPercentage { get; set; }

        /// <summary>
        /// Actually raw RfStrength as db. - can probably rename with out a problem.....
        /// </summary>
        public int? RfStrengthPercentage { get; set; }

        /// <summary>
        /// When the measurement is deemed to be expired (See TTL).
        /// </summary>
        public DateTime? Expires { get; set; }

        public TimeSpan? TTL { get; set; }

        /// <summary>
        /// DynamoDB TTL
        /// </summary>
        /// <remarks>
        /// TTL is a mechanism to set a specific timestamp for expiring items from your table. 
        /// The timestamp should be expressed as an attribute on the items in the table. 
        /// The attribute should be a Number data type containing time in epoch format. 
        /// Once the timestamp expires, the corresponding item is deleted from the table in the background.
        /// 
        /// NB: This attribute will exist in the big measurements table
        /// and the monthly measurements tables.
        /// 
        /// If this is null it will be set by the persistor, otherwise 
        /// set for a long time in the future to override the persistors default
        /// 
        /// The TTL/Expires gives a more precise (down to the second)
        /// expiry, but the record may still exist in the database whilst
        /// the TTL has expired.
        /// </remarks>
        public Int64? DeleteAfter { get; set; }

        /// <summary>
        /// The original source of the measurement.
        /// </summary>
        public string Source { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }

        // New field, old data will not have this property
        public bool Deleted { get; set; }
    }
}