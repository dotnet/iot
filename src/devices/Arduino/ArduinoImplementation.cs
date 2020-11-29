#pragma warning disable CS1591

namespace Iot.Device.Arduino
{
    public enum ArduinoImplementation
    {
        None = 0,
        SetPinMode = 1,
        WritePin = 2,
        ReadPin = 3,
        GetTickCount = 4,
        SleepMicroseconds = 5,
        GetMicroseconds = 6,
        Debug = 7,
        ObjectEquals = 8,
        ReferenceEquals = 9,
        GetType = 10,
        GetHashCode = 11,
        ArrayCopy5 = 12,
        StringCtor0,
        StringLength,
        MonitorEnter1,
        MonitorEnter2,
        MonitorExit,
        StringIndexer,
        StringFormat2,
        StringFormat2b,
        BaseTypeEquals,
        EmptyStaticCtor,
        DefaultEqualityComparer,
        ArrayCopy3,
        StringFormat3,
        ArrayClone
    }
}
