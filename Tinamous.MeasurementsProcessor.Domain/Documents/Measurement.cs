using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Represents one or more date grouped sensor measurements from a user/device.
    /// </summary>
    /// <remarks>
    /// Copied from Main Web Domain project. Used for DB persistence.
    /// </remarks>
    [DebuggerDisplay("M.Date: {MeasurementDate}")]
    public class Measurement
    {
        private List<decimal?> _fields = new List<decimal?>();
        private Guid _mongoDbId = Guid.NewGuid();

        public Measurement()
        {
            PostedOn = DateTime.UtcNow;
        }

        public Guid MongoDbId
        {
            get { return _mongoDbId; }
            set { _mongoDbId = value; }
        }

        public int Version { get; set; }

        public bool Deleted { get; set; }

        public bool Private { get; set; }

        /// <summary>
        /// Account for the user.
        /// </summary>
        /// <remarks>
        ///     Here to make querying simple only. Not that useful.
        /// </remarks>
        public Guid AccountMongoId { get; set; }

        /// <summary>
        /// Identifies the user (Member/Device/Bot) this measurement was from
        /// </summary>
        public Guid UserMongoId { get; set; }

        /// <summary>
        /// Each device can have multiple channels allowing a group of fields to be 
        /// pushed separately.
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// Date the measurement was posted to Tinamous.
        /// </summary>
        public DateTime PostedOn { get; set; }

        /// <summary>
        /// Date the measurement was added to the system
        /// </summary>
        /// <remarks>
        /// Most likely the same as PostedOn.
        /// </remarks>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Measurement date. For list of values where all values are the same field
        /// </summary>
        public DateTime MeasurementDate { get; set; }

        public List<MeasurementField> MeasurementFields { get; set; }

        #region Meta Data

        public List<string> Tags { get; set; }

        public string SampleId { get; set; }

        public string OperatorId { get; set; }

        /// <summary>
        /// Raw battery level.
        /// </summary>
        public decimal? BatteryLevel { get; set; }

        /// <summary>
        /// Battery level as percentage. Need to figure out how to convert it.
        /// </summary>
        public int? BatteryLevelPercentage { get; set; }

        /// <summary>
        /// RfStrength (in db) or whatever the user chooses.
        /// </summary>
        public int? RfStrength { get; set; }

        public LocationDetails Location { get; set; }

        #endregion

        public DateTime? Expires { get; set; }

        public TimeSpan? TTL { get; set; }

        /// <summary>
        /// When the measurement should be deleted from the database.
        /// </summary>
        public Int64? DeleteAfter { get; set; }     

        /// <summary>
        /// Where the measurement originated from
        /// </summary>
        public string Source { get; set; }
    }
}