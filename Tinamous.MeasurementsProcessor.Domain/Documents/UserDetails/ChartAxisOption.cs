namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    public enum ChartAxisOption
    {
        /// <summary>
        /// Unknown.   Use 'UseSecondAxis' to determine
        /// </summary>
        Unknown,

        /// <summary>
        /// Use the shared primary axis.
        /// </summary>
        PrimaryAxis,

        /// <summary>
        /// Use the shared secondary axis
        /// </summary>
        SecondaryAxis,

        /// <summary>
        /// Create a custom axis for this field.
        /// </summary>
        CustomAxis,
    }
}