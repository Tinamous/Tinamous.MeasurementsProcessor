using System;
using System.Linq;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;

namespace Tinamous.MeasurementsProcessor.Domain.Extensions
{
    public static class DeviceExtension
    {
        public static FieldDescriptor GetFieldDescriptor(this User user, int channel, string fieldName)
        {
            if (user.FieldDescriptors == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            return user
                .FieldDescriptors
                .FirstOrDefault(x => x.Channel == channel
                                     && fieldName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}