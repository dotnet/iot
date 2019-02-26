namespace Iot.Device.Mfrc522
{
    public enum RequestMode : byte
    {
        RequestIdle = 0x26,
        RequestAll = 0x52,
        AntiCollision = 0x93,
        SelectTag = 0x93,
        Authenticate1A = 0x60,
        Authenticate1B = 0x61,
        Read = 0x30,
        Write = 0xA0,
        Decrement = 0xC0,
        Increment = 0xC1,
        Restore = 0xC2,
        Transfer = 0xB0,
        Halt = 0x50,
    }
}
