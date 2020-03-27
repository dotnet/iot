namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Averaging mode
    /// </summary>
    public enum AveragingMode
    {
        /// <summary>
        /// Off
        /// </summary>
        Off = 0b00,

        /// <summary>
        /// Average 2 samples
        /// </summary>
        Avg2 = 0b01,

        /// <summary>
        /// Average 8 samples
        /// </summary>
        Avg8 = 0b10,

        /// <summary>
        /// Average 16 samples
        /// </summary>
        Avg16 = 0b11
    }
}
