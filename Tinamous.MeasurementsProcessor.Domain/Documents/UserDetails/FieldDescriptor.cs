using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    /// <summary>
    /// Cache copy of the User FieldDescriptor
    /// </summary>
    [DebuggerDisplay("Name: {Name}")]
    public class FieldDescriptor
    {
        /// <summary>
        /// Index of the descriptors field within the measurement fields for the old api.
        /// </summary>
        /// <remarks>
        /// Index1 would be Field1 with old api etc.
        /// </remarks>
        public int Index { get; set; }

        /// <summary>
        /// The channel this field descriptor applied to.
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// Field name. Thing friendly version.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Field Label. More human friendly label for the field (e.g. on charts etc)
        /// </summary>
        /// <remarks>
        /// Typically this would be the same as the name (and defaults to Name if not set)
        /// </remarks>
        public string Label { get; set; }

        /// <summary>
        /// Unit
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// If the field is visible in device details lists.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// If the field should be excluded from the default chart.
        /// </summary>
        public bool ExcludeFromChart { get; set; }

        /// <summary>
        /// The colour to use when plotting this field on a chart.
        /// </summary>
        public string ChartColor { get; set; }

        /// <summary>
        /// How many decimal places the field should be rounded to
        /// </summary>
        public int Rounding { get; set; }

        ///// <summary>
        ///// Details to be used when this field is shown by it's self through a twitter card.
        ///// </summary>
        //public TwitterCardDetails TwitterCard { get; set; }

        /// <summary>
        /// The range that normal measurements is expected to be within.
        /// </summary>
        /// <remarks>
        /// Measurements outside this range will trigger a measurement out of range warning.
        /// 
        /// May be null if not defined by the user
        /// </remarks>
        public FieldRange WorkingRange { get; set; }

        /// <summary>
        /// Error range.
        /// </summary>
        /// <remarks>
        /// The maximum permissible range for a field value, below or above
        /// this range triggers a out of range error.
        /// 
        /// May be null if not defined by the user
        /// </remarks>
        public FieldRange ErrorRange { get; set; }

        /// <summary>
        /// Calibration for the field. Transforms the raw value using y=mx + c
        /// to replace the value set.
        /// </summary>
        public FieldCalibration Calibration { get; set; }

        /// <summary>
        /// The Y-axis to plot this field on.
        /// </summary>
        /// <remarks>
        /// Y Axis:
        /// 0: Unknown. Use 'UseSecondAxis' to determine
        /// 1: Left hand primary axis
        /// 2: Right hand secondary axis
        /// 3: Custom axis for this field.
        /// 
        /// A null value indicates this has not been set by the user.
        /// </remarks>
        public ChartAxisOption ChartAxisOption { get; set; }

        /// <summary>
        /// If this is the primary field for the device.
        /// </summary>
        /// <remarks>
        /// Used for showing summary info/chart of the device. 
        /// e.g. A thermometer might have temperature, battery level and RF strngth, the
        /// temperature is the primary field of interest.
        /// 
        /// </remarks>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Determins the type of field (e.g. measured, computed, other...)
        /// </summary>
        /// <remarks>
        /// Populate computed options for computed field.
        /// </remarks>
        public FieldType FieldType { get; set; }

        /// <summary>
        /// When showing just this field, use this chart type.
        /// </summary>
        public ChartType DefaultChartType { get; set; }

        /// <summary>
        /// This is the date the field was created and the earliest date that
        /// measurements for this field should be created.
        /// 
        /// If the user deletes a field and it is then re-created by the device this 
        /// effectively starts a brand new field with no history.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Set the devices state (metatag) with the value from this field
        /// </summary>
        /// <remarks>
        /// State (MetaTag) will be created if it doesn't exist.
        /// Name will match (case insensitive) that of the field name.
        /// </remarks>
        public bool SetDeviceState { get; set; }

        #region Computed Field Options

        /// <summary>
        /// The algorithm to use.
        /// </summary>
        /// <remarks>
        /// Set to "User" to use the UserAlgorithm field.
        /// Source field(s) are references through the Variables.
        /// </remarks>
        public ComputedFieldAlgorithm UseAlgorithm { get; set; }

        /// <summary>
        /// User supplied algorithm to use to compute this field value. Use {{FieldName}} to reference fields.
        /// </summary>
        /// <example>
        /// mx + c
        /// would be m x {{Temperature}} + c
        /// 
        /// compute using the using y=mx + c, 
        /// where the Variables are defiend as:
        /// x is "{{Temperature}}" (i.e. the Temperature field)
        /// m is a constant slope value (e.g. 2.3)
        /// c is a constant offset (e.g. 10)
        /// </example>
        public string UserAlgorithm { get; set; }

        public List<ComputedFieldVariables> Variables { get; set; }

        #endregion

        /// <summary>
        /// Allow manual ordering of fields. Defaults to null which uses
        /// alphabetic positioning.
        /// </summary>
        public int? OrderPosition { get; set; }

        public List<string> Tags { get; set; }
    }
}