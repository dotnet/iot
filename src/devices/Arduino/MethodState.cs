#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public enum MethodState
    {
        Stopped = 0,
        Aborted = 1,
        Running = 2,
        Killed = 3,
        ConnectionError
    }
}
