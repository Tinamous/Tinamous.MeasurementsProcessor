using System;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    /// <summary>
    /// Represents either a well-known location or a user set location.
    /// </summary>
    /// <remarks>
    /// For user set location the Id if set would point to a well-known location, however Lat, Long and Elavation would 
    /// be set from the users corrent location or left blank.
    /// </remarks>
    public class LocationDetails
    {
        /// <summary>
        /// This is the well known location id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Friendly name of the user/devices current location.
        /// </summary>
        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Elevation { get; set; }

        public DateTime? LastUpdated { get; set; }

        public bool IsValidLocation()
        {
            if (!LastUpdated.HasValue)
            {
                return false;
            }

            if (Latitude > 0.00001D || Latitude < -0.00001D)
            {
                return true;
            }

            if (Longitude > 0.00001D || Longitude < -0.00001D)
            {
                return true;
            }

            if (Id != Guid.Empty)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Name))
            {
                return true;
            }

            return false;
        }
    }
}