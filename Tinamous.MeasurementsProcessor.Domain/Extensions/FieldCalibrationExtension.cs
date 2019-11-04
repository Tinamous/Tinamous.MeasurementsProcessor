using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;

namespace Tinamous.MeasurementsProcessor.Domain.Extensions
{
    public static class FieldCalibrationExtension
    {
        public static decimal? Apply(this FieldCalibration calibration, decimal? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value * calibration.Slope + calibration.Offset;
        }
    }
}