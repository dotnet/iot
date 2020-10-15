namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Power modes for the gyroscope
    /// </summary>
    public enum GyroscopePowerMode : byte
    {
        /// <summary>See datasheet</summary>
        Normal = 0x00,

        /// <summary>See datasheet</summary>
        Suspend = 0x80,

        /// <summary>See datasheet</summary>
        DeepSuspend = 0x20
    }
}
