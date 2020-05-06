namespace Iot.Device.Adg731
{
    /// <summary>
    /// ADG731 Bit-masks
    /// </summary>
    public enum BitMask : byte
    {
        /// <summary>
        /// Chip Enable
        /// </summary>
        EN = 0b1000_0000,

        /// <summary>
        /// Chip Select
        /// </summary>
        CS = 0b0100_0000,

        /// <summary>
        /// Multiplexer Channel
        /// </summary>
        Ch = 0b0001_1111,
    }
}
