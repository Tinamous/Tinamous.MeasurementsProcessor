using System.Collections.Generic;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    // [Channel] -> Fields -> Point in time -> field value
    public class ChannelData
    {
        public int Id { get; set; }

        public List<DynamoChannelField> Fields { get; set; }

        public List<DynamoChannelFieldAggregate> FieldsAggregates { get; set; }

        public List<DynamoTagData> Tags { get; set; }

        public List<DynamoLocationData> Locations { get; set; }
    }
}