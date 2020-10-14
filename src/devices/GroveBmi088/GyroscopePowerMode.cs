namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Power modes for the gyroscope
    /// </summary>
    public enum GyroscopePowerMode : byte
    {
        /// <summary>See datasheet</summary>
        GYRO_NORMAL = 0x00,

        /// <summary>See datasheet</summary>
        GYRO_SUSPEND = 0x80,

        /// <summary>See datasheet</summary>
        GYRO_DEEP_SUSPEND = 0x20
    }
}
