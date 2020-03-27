namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Communications Register Bit Designations
    /// </summary>
    public enum CommunicationsRegisterBits : byte
    {
        /// <summary>
        /// CR7/!WEN: Write enable bit. For a write to the communications register to occur, 0 must be written to this bit. If a 1 is the first bit written, the part does not clock onto subsequent bits in the register; rather, it stays at this bit location until a 0 is written to this bit.
        /// </summary>
        WriteEnable = (1 << 7),

        /// <summary>
        /// CR6: 0 in this bit location indicates that the next operation is a write to a specified register.
        /// </summary>
        WriteOperation = (0 << 6),

        /// <summary>
        /// CR6: 1 in this bit position indicates that the next operation is a read from the designated register.
        /// </summary>
        ReadOperation = (1 << 6),

        /// <summary>
        /// CR2/CREAD: Continuous read of the data register. When this bit is set to 1 (and the data register is selected), the serial interface is configured so that the data register can be continuously read; that is, the contents of the data register are automatically placed on the DOUT pin when the SCLK pulses are applied after the RDY pin goes low to indicate that a conversion is complete.
        /// </summary>
        ContinuousDataRead = (1 << 2)
    }
}
