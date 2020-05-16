using System;
using System.Collections.Generic;
using System.Text;

namespace Sht1x
{
    /// <summary>
    /// Commands available for sending to the Sht1x
    /// </summary>
    public enum Command : byte
    {
        /// <summary>
        /// Used to reset the status register
        /// </summary>
        NoOp = 0b00000000,

        /// <summary>
        /// Used to request a temperature reading
        /// </summary>
        Temperature = 0b00000011,

        /// <summary>
        /// Used to request a humidity reading
        /// </summary>
        Humidity = 0b00000101,

        /// <summary>
        /// Used to read the status register
        /// </summary>
        ReadStatusRegister = 0b00000111,

        /// <summary>
        /// Used to write to the status register
        /// </summary>
        WriteStatusRegister = 0b00000110,

        /// <summary>
        /// Used to perform a soft reset on the device
        /// </summary>
        SoftReset = 0b00011110
    }
}
