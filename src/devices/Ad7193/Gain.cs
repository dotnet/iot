namespace Iot.Device.Ad7193
{
    /// <summary>
    /// PGA Gain settings
    /// </summary>
    public enum Gain
    {
        /// <summary>
        /// PGA gain off
        /// </summary>
        X1 = 0b000,

        /// <summary>
        /// 8x PGA gain
        /// </summary>
        X8 = 0b011,

        /// <summary>
        /// 16x PGA gain
        /// </summary>
        X16 = 0b100,

        /// <summary>
        /// 32x PGA gain
        /// </summary>
        X32 = 0b101,

        /// <summary>
        /// 32x PGA gain
        /// </summary>
        X64 = 0b110,

        /// <summary>
        /// 128x PGA gain
        /// </summary>
        X128 = 0b111
    }
}
