using System;
using System.Diagnostics;
using Amazon.DynamoDBv2.DataModel;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Table to hold device field value.
    ///
    /// Lookup designed to query by device + key begins with field, then sorted by date (with Id appended for uniqueness)
    ///
    /// For measurement by Id, use FieldDateIdKey "Contains" id query.
    /// </summary>
    [DynamoDBTable("Measurement.DeviceField")]
    [DebuggerDisplay("Device: {DeviceId}, FieldDateIdKey: {FieldDateIdKey}, Value: {V}")]
    public class DynamoDeviceFieldMeasurementModel
    {
        public DynamoDeviceFieldMeasurementModel()
        { }

        public DynamoDeviceFieldMeasurementModel(Guid userId, 
            Guid measurementId, 
            string name,
            decimal? value,
            decimal? rawValue,
            string stringValue,
            bool boolValue,
            decimal? sum,
            int channel,
            DateTime date)
        {
            DeviceId = userId;
            Chan = channel;
            // Field name part of FieldDateId key;
            V = value;
            RV = rawValue;
            SV = stringValue;
            BV = boolValue;
            Sum = sum;

            GenerateKey(name, date, measurementId);
        }

        [DynamoDBHashKey]
        public Guid DeviceId { get; set; }

        [DynamoDBRangeKey]
        public string FieldDateIdKey { get; set; }

        /// <summary>
        /// Channel (most likely 0, might not be supported 
        /// </summary>
        public int Chan { get; set; }


        // TODO: Add GetDate to extract the date from the FieldDateIdKey

        /// <summary>
        /// Field Value
        /// </summary>
        public decimal? V { get; set; }

        /// <summary>
        /// Raw Value
        /// </summary>
        public decimal? RV { get; set; }

        /// <summary>
        /// String Value
        /// </summary>
        public string SV { get; set; }

        /// <summary>
        /// Boolean Value
        /// </summary>
        public bool? BV { get; set; }

        /// <summary>
        /// Sum
        /// </summary>
        public decimal? Sum { get; set; }

        public void GenerateKey(string name, DateTime date, Guid measurementId)
        {
            // Note: Doesn't include channel as that's rarely used and probably being phased out.
            // Note: Case sensitive!
            FieldDateIdKey = string.Format("{0}#{1}#{2}", name, date.ToUniversalTime().ToString("O"), measurementId);
        }
    }
}