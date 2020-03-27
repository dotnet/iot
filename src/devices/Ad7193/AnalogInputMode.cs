namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Analog input mode
    /// </summary>
    public enum AnalogInputMode
    {
        /// <summary>
        /// 4 differential analog inputs (pseudo bit = 0)
        /// </summary>
        FourDifferentialAnalogInputs = 0b0,

        /// <summary>
        /// 8 pseudo-differential analog inputs (pseudo bit = 1)
        /// </summary>
        EightPseudoDifferentialAnalogInputs = 0b1
    }
}
