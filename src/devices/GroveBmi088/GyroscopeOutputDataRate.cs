namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Data rates for the gyroscope
    /// </summary>
    public enum GyroscopeOutputDataRate : byte
    {
        /// <summary>See datasheet</summary>
        ODR_2000_BW_532 = 0x00,

        /// <summary>See datasheet</summary>
        ODR_2000_BW_230 = 0x01,

        /// <summary>See datasheet</summary>
        ODR_1000_BW_116 = 0x02,

        /// <summary>See datasheet</summary>
        ODR_400_BW_47 = 0x03,

        /// <summary>See datasheet</summary>
        ODR_200_BW_23 = 0x04,

        /// <summary>See datasheet</summary>
        ODR_100_BW_12 = 0x05,

        /// <summary>See datasheet</summary>
        ODR_200_BW_64 = 0x06,

        /// <summary>See datasheet</summary>
        ODR_100_BW_32 = 0x07
    }
}
