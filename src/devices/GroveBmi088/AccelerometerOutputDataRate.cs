namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Data rate for the accelerometer
    /// </summary>
    public enum AccelerometerOutputDataRate : byte
    {
        /// <summary>See datasheet</summary>
        ODR_12 = 0x05,

        /// <summary>See datasheet</summary>
        ODR_25 = 0x06,

        /// <summary>See datasheet</summary>
        ODR_50 = 0x07,

        /// <summary>See datasheet</summary>
        ODR_100 = 0x08,

        /// <summary>See datasheet</summary>
        ODR_200 = 0x09,

        /// <summary>See datasheet</summary>
        ODR_400 = 0x0A,

        /// <summary>See datasheet</summary>
        ODR_800 = 0x0B,

        /// <summary>See datasheet</summary>
        ODR_1600 = 0x0C
    }
}
