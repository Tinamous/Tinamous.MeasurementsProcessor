using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Daily summary for a particular field (device-channel-field to be exact).
    /// </summary>
    [DynamoDBTable("DailyFieldSummary")]
    [DebuggerDisplay("BaseTime: {Day}, Field: {Field}")]
    public class DynamoDailyFieldSummaryModel
    {
        private Guid _id = Guid.NewGuid();

        // primary Index
        [DynamoDBHashKey]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        // Secondary Index (field name should be lower case)
        [DynamoDBGlobalSecondaryIndexHashKey("Device-Channel-Field-Day-Index")]
        public string DeviceChannelFieldHash { get; set; }

        /// <summary>
        /// The Day this daily statistics is for.
        /// </summary>
        /// <remarks>This is also used as the offset for the field points.</remarks>
        [DynamoDBGlobalSecondaryIndexRangeKey(new[] { "Device-Channel-Field-Day-Index", "Device-Index" })]
        public DateTime Day { get; set; }

        public Guid AccountId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("Device-Index")]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Lowercase field name
        /// </summary>
        public string Field { get; set; }
        public int Channel { get; set; }
        public DateTime Created { get; set; }
        public bool Deleted { get; set; }

        public DynamoSummaryStatistics Statistics { get; set; }

        /// <summary>
        /// List of array of decimals. 
        /// Where 1st entry is time in ms, 2nd entry is value
        /// </summary>
        public List<DynamoTimeValuePoint> Points { get; set; }

        /// <summary>
        /// A list of buckets (24) for each hour with the total number of 
        /// measurement points for the field that have occurred.
        /// The sum of these should be the same as the statistics sum
        /// </summary>
        public List<DynamoTimeValuePoint> CountsPerHour { get; set; }
    }
}