using System.Reflection;

namespace Tinamous.MeasurementsProcessor.Domain.Settings
{
    public class ServerSettings
    {
        public string ServerName { get; set; }

        public string ServiceName { get { return "MeasurementsProcessor"; } }

        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public bool IsPrimary { get; set; }
    }
}