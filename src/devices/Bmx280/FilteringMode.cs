namespace Iot.Device.Bmx280
{
    public enum FilteringMode : byte
    {
        /// <summary>
        /// Filter off
        /// </summary>
        Off = 0b000,
        /// <summary>
        /// Coefficient x2
        /// </summary>
        X2 = 0b001,
        /// <summary>
        /// Coefficient x4
        /// </summary>
        X4 = 0b010,
        /// <summary>
        /// Coefficient x8
        /// </summary>
        X8 = 0b011,
        /// <summary>
        /// Coefficient x16
        /// </summary>
        X16 = 0b100,
    }
}
