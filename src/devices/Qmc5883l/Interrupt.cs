namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Interrupt enabling is controlled by register INT_ENB in control register 2. Once the interrupt is enabled, it will flag
    /// when new data is in Data Output Registers.
    /// INT_ENB: “0”: enable interrupt PIN, “1”: disable interrupt PIN
    /// </summary>
    public enum Interrupt : byte
    {
        /// <summary>
        /// Enables the inerrupt PIN
        /// </summary>
       ENABLE = 0x00,

        /// <summary>
        /// Disables the inerrupt PIN
        /// </summary>
       DISABLE = 0x01
    }
}
