using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    [DynamoDBTable("HourlyFieldSummary")]
    [DebuggerDisplay("BaseTime: {BaseTime}, Field: {Field}")]
    public class DynamoHourlyFieldSummaryModel
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
        [DynamoDBGlobalSecondaryIndexHashKey("Device-Channel-Field-BaseTime-Index")]
        public string DeviceChannelFieldHash { get; set; }

        /// <summary>
        /// The BaseTime this daily statistics is for. (yyyy/m/dd/hh only)
        /// </summary>
        /// <remarks>This is also used as the offset for the field points.</remarks>
        [DynamoDBGlobalSecondaryIndexRangeKey(new[] { "Device-Channel-Field-BaseTime-Index", "Device-Index" })]
        public DateTime BaseTime { get; set; }

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
        /// A list of buckets (60) for each hour with the total number of 
        /// measurement points for the field that have occurred.
        /// The sum of these should be the same as the statistics sum
        /// </summary>
        public List<DynamoTimeValuePoint> CountsPerMinute { get; set; }
    }
}