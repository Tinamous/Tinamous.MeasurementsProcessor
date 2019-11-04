using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    /// <summary>
    /// Represents the status of a field
    /// </summary>
    public class FieldStatus
    {
        private bool _belowWorkingRange;
        private bool _aboveWorkingRange;
        private bool _belowErrorRange;
        private bool _aboveErrorRange;

        public FieldStatus()
        { }

        public FieldStatus(Guid accountId, Guid userOrDeviceId, int channel, string fieldName)
        {
            AccountId = accountId;
            UserId = userOrDeviceId;
            Channel = channel;
            LowerFieldName = fieldName.ToLower();
            Key = GetKey();
        }

        public string Key { get; set; }

        public Guid AccountId { get; set; }

        public Guid UserId { get; set; }

        public int Channel { get; set; }

        /// <summary>
        /// Lowercase field name.
        /// </summary>
        public string LowerFieldName { get; set; }

        public bool BelowWorkingRange
        {
            get { return _belowWorkingRange; }
            set
            {
                if (_belowWorkingRange != value)
                {
                    _belowWorkingRange = value;
                    IsDirty = true;
                }
            }
        }

        public bool AboveWorkingRange
        {
            get { return _aboveWorkingRange; }
            set
            {
                if (_aboveWorkingRange != value)
                {
                    _aboveWorkingRange = value;
                    IsDirty = true;
                }
            }
        }

        public bool BelowErrorRange
        {
            get { return _belowErrorRange; }
            set
            {
                if (_belowErrorRange != value)
                {
                    _belowErrorRange = value;
                    IsDirty = true;
                }
            }
        }

        public bool AboveErrorRange
        {
            get { return _aboveErrorRange; }
            set
            {
                if (_aboveErrorRange != value)
                {
                    _aboveErrorRange = value;
                    IsDirty = true;
                }
            }
        }

        public bool IsDirty { get; set; }

        public DateTime LastUpdated { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// User by AutoMapper to create the Key field.
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
            return GenerateKey(UserId, Channel, LowerFieldName);
        }

        public static string GenerateKey(Guid userId, int channel, string fieldName)
        {
            return string.Format("{0}-{1}-{2}", userId, channel, fieldName.ToLower());
        }
    }
}