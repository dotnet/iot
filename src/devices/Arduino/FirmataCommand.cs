namespace Iot.Device.Arduino
{
    /// <summary>
    /// Primary firmata commands
    /// </summary>
    public enum FirmataCommand : byte
    {
        /// <summary>
        /// Digital pins have changed
        /// </summary>
        DIGITAL_MESSAGE = 144, // 0x00000090

        /// <summary>
        /// Request a report on a changing analog pin
        /// </summary>
        REPORT_ANALOG_PIN = 192, // 0x000000C0

        /// <summary>
        /// Request a report on changing digital ports
        /// Note that digital pins are grouped to 8-byte ports
        /// </summary>
        REPORT_DIGITAL_PIN = 208, // 0x000000D0

        /// <summary>
        /// Analog pin state
        /// </summary>
        ANALOG_MESSAGE = 224, // 0x000000E0

        /// <summary>
        /// Start an extended message
        /// </summary>
        START_SYSEX = 240, // 0x000000F0

        /// <summary>
        /// Set the input/output mode of a pin
        /// </summary>
        SET_PIN_MODE = 244, // 0x000000F4

        /// <summary>
        /// Set the value of a digital pin
        /// </summary>
        SET_DIGITAL_VALUE = 0xF5,

        /// <summary>
        /// End an extended message
        /// </summary>
        END_SYSEX = 247, // 0x000000F7

        /// <summary>
        /// Protocol version request/reply
        /// </summary>
        PROTOCOL_VERSION = 249, // 0x000000F9

        /// <summary>
        /// System was reset
        /// </summary>
        SYSTEM_RESET = 255 // 0x000000FF
    }
}
