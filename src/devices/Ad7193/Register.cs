namespace Iot.Device.Ad7193
{
    /// <summary>
    /// AD7193 Register Map
    /// </summary>
    public enum Register : byte
    {
        /// <summary>
        /// Communications Register (WO, 8-bit)
        /// </summary>
        Communications = 0,

        /// <summary>
        /// Status Register         (RO, 8-bit)
        /// </summary>
        Status = 0,

        /// <summary>
        /// Mode Register           (RW, 24-bit)
        /// </summary>
        Mode = 1,

        /// <summary>
        /// Configuration Register  (RW, 24-bit)
        /// </summary>
        Configuration = 2,

        /// <summary>
        /// Data Register           (RO, 24/32-bit)
        /// </summary>
        Data = 3,

        /// <summary>
        /// ID Register             (RO, 8-bit)
        /// </summary>
        ID = 4,

        /// <summary>
        /// GPOCON Register         (RW, 8-bit)
        /// </summary>
        GPOCON = 5,

        /// <summary>
        /// Offset Register         (RW, 24-bit)
        /// </summary>
        Offset = 6,

        /// <summary>
        /// Full-Scale Register     (RW, 24-bit)
        /// </summary>
        FullScale = 7,
    }
}
