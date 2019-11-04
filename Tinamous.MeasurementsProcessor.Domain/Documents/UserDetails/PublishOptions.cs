namespace Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails
{
    public class PublishOptions
    {
        /// <summary>
        /// If the [measurements|status|...] should be published to the MQTT server.
        /// </summary>
        public bool Mqtt { get; set; }
    }
}