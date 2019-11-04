using EasyNetQ;

namespace Tinamous.MeasurementsProcessor.Services.Interfaces
{
    public interface IBusFactory
    {
        IBus CreateEventsBus();
        IBus CreateRpcBus();
    }
}