namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Over sample Rate (OSR) registers are used to control bandwidth of an internal digital filter.
    /// Larger OSR valueleads to smaller filter bandwidth, less in-band noise and higher power consumption.It could be used to reach a
    /// good balance between noise and power. Four over sample ratio can be selected, 64, 128, 256 or 512.
    /// </summary>
    public enum Oversampling : byte
    {
        /// <summary>
        /// Over sample rate os 512
        /// </summary>
        OS512 = 0x00,

        /// <summary>
        /// Over sample rate os 256
        /// </summary>
        OS256 = 0x40,

        /// <summary>
        /// Over sample rate os 128
        /// </summary>
        OS128 = 0x80,

        /// <summary>
        /// Over sample rate os 64
        /// </summary>
        OS64 = 0xC0
    }
}
