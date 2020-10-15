namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Data rates for the gyroscope
    /// </summary>
    public enum GyroscopeOutputDataRate : byte
    {
        /// <summary>See datasheet</summary>
        Odr2000_Bw532 = 0x00,

        /// <summary>See datasheet</summary>
        Odr2000_Bw230 = 0x01,

        /// <summary>See datasheet</summary>
        Odr1000_Bw116 = 0x02,

        /// <summary>See datasheet</summary>
        Odr400_Bw47 = 0x03,

        /// <summary>See datasheet</summary>
        Odr200_Bw23 = 0x04,

        /// <summary>See datasheet</summary>
        Odr100_Bw12 = 0x05,

        /// <summary>See datasheet</summary>
        Odr200_Bw64 = 0x06,

        /// <summary>See datasheet</summary>
        Odr100_Bw32 = 0x07
    }
}
