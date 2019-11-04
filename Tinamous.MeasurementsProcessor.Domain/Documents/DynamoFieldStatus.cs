using System;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    [DynamoDBTable("FieldStatus")]
    public class DynamoFieldStatus
    {
        // primary Index
        [DynamoDBHashKey]
        public string Key { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("AccountId-index")]
        public Guid AccountId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("UserId-index")]
        public Guid UserId { get; set; }

        public int Channel { get; set; }

        /// <summary>
        /// Lowercase field name.
        /// </summary>
        public string LowerFieldName { get; set; }
        
        public bool BelowWorkingRange { get; set; }
        public bool AboveWorkingRange { get; set; }
        public bool BelowErrorRange { get; set; }
        public bool AboveErrorRange { get; set; }

        public DateTime LastUpdated { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }
    }
}