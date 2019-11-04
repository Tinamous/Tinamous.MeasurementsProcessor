namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    /// <summary>
    /// Well known algorithms for converting specified field (x) to this field (y)
    /// </summary>
    public enum ComputedFieldAlgorithm
    {
        /// <summary>
        /// Convert Kelvin to °C
        /// </summary>
        KelvinToCelcius = 0,

        /// <summary>
        /// Convert Kelvin to °F
        /// </summary>
        KelvinToFahrenheit = 1,

        /// <summary>
        /// Convert °F to °C
        /// </summary>
        FahrenheitToCelsius = 2,

        /// <summary>
        /// Convert °F to Kelvin
        /// </summary>
        FahrenheitToKelvin = 3,

        /// <summary>
        /// Convert °C to °F
        /// </summary>
        CelsiusToFahrenheit = 4,

        /// <summary>
        /// Convert °C to Kelvin
        /// </summary>
        CelsiusToKelvin = 5,

        /// <summary>
        /// y = x + c - used for simple calibration
        /// </summary>
        Offset,

        /// <summary>
        /// y=mx - used for simple calibration
        /// </summary>
        Slope,

        /// <summary>
        /// y=mx+c - used for calibration
        /// </summary>
        SlopeAndOffset,

        /// <summary>
        /// Base 10 Log.
        /// </summary>
        Log10,

        /// <summary>
        /// 10x ten to the power of x. // Inverse Log10
        /// </summary>
        TenX,

        /// <summary>
        /// Natural log
        /// </summary>
        Ln,

        /// <summary>
        /// E to the x - Inverse Ln
        /// </summary>
        Exponential,

        /// <summary>
        /// 1/x
        /// </summary>
        Inverse,

        /// <summary>
        /// x^2
        /// </summary>
        Squared,

        /// <summary>
        /// x^3
        /// </summary>
        Cubed,

        /// <summary>
        /// Square root
        /// </summary>
        SquareRoot,

        /// <summary>
        /// Cube root.
        /// </summary>
        CubeRoot,

        /// <summary>
        /// Simple =. Sets this field equal to the other field.
        /// </summary>
        Equals,

        UserDefined = 65535,
    }
}