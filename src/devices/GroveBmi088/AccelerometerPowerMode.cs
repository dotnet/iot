namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Power modes for the accelerometer
    /// </summary>
    public enum AccelerometerPowerMode : byte
    {
        /// <summary>See datasheet</summary>
        ACC_ACTIVE = 0x00,

        /// <summary>See datasheet</summary>
        ACC_SUSPEND = 0x03
    }
}
