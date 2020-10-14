namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Available ranges for the accelerometer
    /// </summary>
    public enum AccelerometerMeasurementRange : byte
    {
        /// <summary>0 to 3g</summary>
        RANGE_3G = 0x00,

        /// <summary>0 to 6g</summary>
        RANGE_6G = 0x01,

        /// <summary>0 to 12g</summary>
        RANGE_12G = 0x02,

        /// <summary>0 to 24g</summary>
        RANGE_24G = 0x03,
    }
}
