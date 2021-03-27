namespace Iot.Device.Arduino
{
    internal enum CommandError
    {
        None = 0,
        EngineBusy = 1,
        InvalidArguments = 2,
        OutOfMemory = 3,
        InternalError = 4,
        Timeout = 5,
        DeviceReset = 6,
        Aborted = 7,
    }
}
