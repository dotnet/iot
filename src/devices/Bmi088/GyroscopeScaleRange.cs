namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Sensitivity of the gyroscope
    /// </summary>
    public enum GyroscopeScaleRange : byte
    {
        /// <summary>2000 degrees/second</summary>
        Range2000 = 0x00,

        /// <summary>1000 degrees/second</summary>
        Range1000 = 0x01,

        /// <summary>500 degrees/second</summary>
        Range500 = 0x02,

        /// <summary>250 degrees/second</summary>
        Range250 = 0x03,

        /// <summary>125 degrees/second</summary>
        Range125 = 0x04
    }
}
