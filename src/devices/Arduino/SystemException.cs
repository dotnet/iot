#pragma warning disable CS1591

namespace Iot.Device.Arduino
{
    public enum SystemException
    {
        None = 0,
        StackOverflow = 1,
        NullReference = 2,
        MissingMethod = 3,
        InvalidOpCode = 4,
        DivideByZero = 5,
        IndexOutOfRange = 6,
        OutOfMemory = 7,
        ArrayTypeMismatch = 8,
        InvalidOperation = 9,
        ClassNotFound = 10,
        InvalidCast = 11,
        NotSupported = 12
    }
}
