namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Communications Register Bit Designations
    /// </summary>
    public enum CommunicationsRegisterBits : byte
    {
        WriteEnable = (1 << 7),
        WriteOperation = (0 << 6),
        ReadOperation = (1 << 6),
        ContinuousDataRead = (1 << 2)
    }
}
