using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    // [Device] -> Channel -> Fields -> Point in time -> field value
    [DynamoDBTable("HourlyMeasurement")]
    public class DynamoHourlyMeasurementModel
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("UserId-Hour-index")]
        public Guid UserId { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey(new[] { "UserId-Hour-index" })]
        public DateTime Hour { get; set; }

        public List<ChannelData> Channels { get; set; }

        public DateTime DateAdded { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }
    }
}