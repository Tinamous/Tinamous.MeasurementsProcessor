namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    public enum FieldType
    {
        /// <summary>
        /// The field is a measured/observed measurement type.
        /// </summary>
        Measured,

        /// <summary>
        /// This field is computed from another field or value (Excludes)
        /// </summary>
        Computed,

        ///// <summary>
        ///// Is a copy of a field from another device.
        ///// </summary>
        //Cloned, 

        // Other field types???
    }
}