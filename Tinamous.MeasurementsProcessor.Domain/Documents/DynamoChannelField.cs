using System.Collections.Generic;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    // Channel -> [Field] -> Point in time -> field value
    public class DynamoChannelField
    {
        public string Name { get; set; }
        public List<PointInTimeField> Points { get; set; }
    }
}