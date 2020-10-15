namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Power modes for the accelerometer
    /// </summary>
    public enum AccelerometerPowerMode : byte
    {
        /// <summary>See datasheet</summary>
        Active = 0x00,

        /// <summary>See datasheet</summary>
        Suspend = 0x03
    }
}
