using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    // // Channel -> Fields -> [Point in time] -> field value
    public class PointInTimeField
    {
        /// <summary>
        /// Time point Index. Derived from (Minute * 60) +Second
        /// </summary>
        public int Index { get; set; }

        public int Minute { get; set; }

        public int Second { get; set; }

        public void SetIndex(DateTime date)
        {
            Minute = date.Minute;
            Second = date.Second;
            Index = (Minute*60) + Second;
        }

        public DynamoPointMeasurementField FieldData { get; set; }
    }
}