using System.Diagnostics;

namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    [DebuggerDisplay("Enabled:{Enabled}. {Min}-{Max}")]
    public class FieldRange
    {
        public bool Enabled { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string Color { get; set; }
    }
}