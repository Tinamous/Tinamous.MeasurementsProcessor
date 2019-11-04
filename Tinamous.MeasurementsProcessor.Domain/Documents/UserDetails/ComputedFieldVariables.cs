namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    /// <summary>
    /// Variable used in computed fields
    /// </summary>
    public class ComputedFieldVariables
    {
        /// <summary>
        /// The variable name (e.g. x, m, c etc.)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value for the variable. Either numeric or field name.
        /// </summary>
        /// <remarks>
        /// May be a decimal value or a field name
        /// for field name use {{FieldName}}
        /// </remarks>
        public string Value { get; set; }
    }
}