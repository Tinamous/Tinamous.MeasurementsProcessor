using System;

namespace Tinamous.MeasurementsProcessor.Domain.Extensions
{
    public static class DateTimeExtension
    {
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1);

        public static decimal ToUnixSeconds(this DateTime dateTime)
        {
            return Convert.ToDecimal(dateTime.Subtract(EpochDateTime).TotalSeconds);
        }

        public static long ToLongUnixSeconds(this DateTime dateTime)
        {
            return Convert.ToInt64(dateTime.Subtract(EpochDateTime).TotalSeconds);
        }

        public static double ToUnixSecondsDouble(this DateTime dateTime)
        {
            return dateTime.Subtract(EpochDateTime).TotalSeconds;
        }
    }
}