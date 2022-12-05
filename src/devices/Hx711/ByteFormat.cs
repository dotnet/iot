namespace Iot.Device.HX711
{
    /// <summary>
    /// Byte order ("endianness") in an architecture
    /// </summary>
    internal enum ByteFormat
    {
        /// <summary>
        /// Less Significant Bit (aka Little-endian) byte format sequence
        /// </summary>
        LSB = 0,

        /// <summary>
        /// Most Significant Bit (aka Big-endian) byte format sequence
        /// </summary>
        MSB = 1,
    }
}
