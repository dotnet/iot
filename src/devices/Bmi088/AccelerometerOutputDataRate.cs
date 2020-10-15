namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Data rate for the accelerometer
    /// </summary>
    public enum AccelerometerOutputDataRate : byte
    {
        /// <summary>See datasheet</summary>
        Odr12 = 0x05,

        /// <summary>See datasheet</summary>
        Odr25 = 0x06,

        /// <summary>See datasheet</summary>
        Odr50 = 0x07,

        /// <summary>See datasheet</summary>
        Odr100 = 0x08,

        /// <summary>See datasheet</summary>
        Odr200 = 0x09,

        /// <summary>See datasheet</summary>
        Odr400 = 0x0A,

        /// <summary>See datasheet</summary>
        Odr800 = 0x0B,

        /// <summary>See datasheet</summary>
        Odr1600 = 0x0C
    }
}
