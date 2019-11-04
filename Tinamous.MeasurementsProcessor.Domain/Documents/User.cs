using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon.DynamoDBv2.DataModel;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Simple lightweight measurement service limited User representation
    /// of a Membership User without the overhead.
    /// </summary>
    [DynamoDBTable("Measurement.UserProperties")]
    [DebuggerDisplay("Name: {FullUserName}")]
    public class User
    {
        private List<FieldDescriptor> _fieldDescriptors = new List<FieldDescriptor>();
        private PublishOptions _publishOptions = new PublishOptions { Mqtt = true };

        public User()
        {
            DateAdded = DateTime.UtcNow;
        }

        /// <summary>
        /// User Id, this should be the same as the membership user id.
        /// </summary>
        [DynamoDBHashKey]
        public Guid AccountId { get; set; }

        /// <summary>
        /// User Id, this should be the same as the membership user id.
        /// </summary>
        /// <remarks>
        /// For table index optimisation, User is queried by AccountId + User Id
        /// and listing all users by AccountId.
        /// </remarks>
        [DynamoDBRangeKey]
        public Guid UserId { get; set; }

        public string UserName { get; set; }
        public string FullUserName { get; set; }
        public string DisplayName { get; set; }

        public Guid OwnerId { get; set; }

        /// <summary>
        /// Cached version (master is on the Membership service)
        /// </summary>
        public List<FieldDescriptor> FieldDescriptors
        {
            get { return _fieldDescriptors; }
            set { _fieldDescriptors = value; }
        }

        public PublishOptions PublishOptions
        {
            get
            {
                return _publishOptions ?? new PublishOptions { Mqtt = true };
            }
            set { _publishOptions = value; }
        }

        public LocationDetails Location { get; set; }

        /// <summary>
        /// Date the owner requested measurements to be purged. Do not report measurements
        /// before this date.
        /// </summary>
        public DateTime? PurgeDate { get; set; }

        public bool Deleted { get; set; }

        public List<string> Tags { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Version of the user at the membership service.
        /// </summary>
        public int? MembershipUserVersion { get; set; }

        /// <summary>
        /// The maximum number of days the measurements 
        /// should be retained for this device/user.
        /// </summary>
        /// <remarks>
        /// Individual measurements may have a shorter TTL
        /// but may not have a longer TTL.
        /// 
        /// User/Device retention time should be limited to a maximum of the 
        /// accounts max retention time (this needs to be validated/set 
        /// as part of the API validation and user update).
        /// 
        /// If it is null, then it is not set, so use the default value.
        /// 
        /// Note: The large measurements table may have a shorter
        /// retention time.
        /// </remarks>
        public int? MeasurementsRetentionTimeDays { get; set; }

        [DynamoDBVersion]
        public int? DynamoDBVersion { get; set; }
    }
}