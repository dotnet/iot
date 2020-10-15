namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Available ranges for the accelerometer
    /// </summary>
    public enum AccelerometerScaleRange : byte
    {
        /// <summary>0 to 3g</summary>
        Range3g = 0x00,

        /// <summary>0 to 6g</summary>
        Range6g = 0x01,

        /// <summary>0 to 12g</summary>
        Range12g = 0x02,

        /// <summary>0 to 24g</summary>
        Range24g = 0x03,
    }
}
