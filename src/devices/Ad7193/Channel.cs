using System;

namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Channel selection flags
    /// </summary>
    [Flags]
    public enum Channel
    {
        /// <summary>
        /// Channel CH0
        /// (Pseudo Bit = 0) -> +:AIN1 and -:AIN2
        /// (Pseudo Bit = 1) -> +:AIN1 and -:AINCOM
        /// </summary>
        CH00 = 0b00_0000_0001,

        /// <summary>
        /// Channel CH1
        /// (Pseudo Bit = 0) -> +:AIN3 and -:AIN4
        /// (Pseudo Bit = 1) -> +:AIN2 and -:AINCOM
        /// </summary>
        CH01 = 0b00_0000_0010,

        /// <summary>
        /// Channel CH2
        /// (Pseudo Bit = 0) -> +:AIN5 and -:AIN6
        /// (Pseudo Bit = 1) -> +:AIN3 and -:AINCOM
        /// </summary>
        CH02 = 0b00_0000_0100,

        /// <summary>
        /// Channel CH3
        /// (Pseudo Bit = 0) -> +:AIN7 and -:AIN8
        /// (Pseudo Bit = 1) -> +:AIN4 and -:AINCOM
        /// </summary>
        CH03 = 0b00_0000_1000,

        /// <summary>
        /// Channel CH4
        /// (Pseudo Bit = 0) -> +:AIN1 and -:AIN2
        /// (Pseudo Bit = 1) -> +:AIN5 and -:AINCOM
        /// </summary>
        CH04 = 0b00_0001_0000,

        /// <summary>
        /// Channel CH5
        /// (Pseudo Bit = 0) -> +:AIN3 and -:AIN4
        /// (Pseudo Bit = 1) -> +:AIN6 and -:AINCOM
        /// </summary>
        CH05 = 0b00_0010_0000,

        /// <summary>
        /// Channel CH6
        /// (Pseudo Bit = 0) -> +:AIN5 and -:AIN6
        /// (Pseudo Bit = 1) -> +:AIN7 and -:AINCOM
        /// </summary>
        CH06 = 0b00_0100_0000,

        /// <summary>
        /// Channel CH7
        /// (Pseudo Bit = 0) -> +:AIN7 and -:AIN8
        /// (Pseudo Bit = 1) -> +:AIN8 and -:AINCOM
        /// </summary>
        CH07 = 0b00_1000_0000,

        /// <summary>
        /// Temperature sensor
        /// </summary>
        TEMP = 0b01_0000_0000,

        /// <summary>
        /// Short
        /// (Pseudo Bit = 0) -> +:AIN2 and -:AIN2
        /// (Pseudo Bit = 1) -> +:AINCOM and -:AINCOM
        /// </summary>
        Shrt = 0b10_0000_0000
    }
}
