using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    public class VirtualFieldDescriptor : FieldDescriptor
    {
        /// <summary>
        /// The user/device that this virtual field is liked to
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The field to map to.
        /// </summary>
        public string FieldName { get; set; }
    }
}