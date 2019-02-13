namespace Iot.Device.Hmc5883
{
    public enum Registers
    {
        /// <summary>
        /// The configuration register is used to configure the device for setting the data output rate and measurement configuration.
        /// </summary>
        ConfigurationRegisterA = 0x00,
        /// <summary>
        /// The configuration register B for setting the device gain.
        /// </summary>
        ConfigurationRegisterB = 0x01,
        /// <summary>
        /// The mode register is an 8-bit register from which data can be read or to which data can be written. 
        /// This register is used to select the operating mode of the device. 
        /// </summary>
        ModeRegister = 0x02,
        /// <summary>
        /// The start address for data output registers.
        /// </summary>
        DataRegisterBegin = 0x03,
        /// <summary>
        /// The status register is an 8-bit read-only register. This register is used to indicate device status.
        /// </summary>
        StatusRegister = 0x09,
    }
}