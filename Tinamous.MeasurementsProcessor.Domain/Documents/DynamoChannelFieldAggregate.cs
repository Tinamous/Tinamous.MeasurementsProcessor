namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    public class DynamoChannelFieldAggregate
    {
        public string Name { get; set; }
        /// <summary>
        /// Number of points in the collection
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The sum of the field data value.
        /// </summary>
        public double Sum { get; set; }

        /// <summary>
        /// Mean Average of the points that have a value.
        /// </summary>
        public double Mean { get; set; }

        public double Min { get; set; }
        public double Max { get; set; }
        public double StandardDeviation { get; set; }
    }
}