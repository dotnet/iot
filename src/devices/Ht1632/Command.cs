// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// Command Summary
    /// </summary>
    public enum Command : byte
    {
        // References middle 8-bits of command codes like:
        // 100 0000-0000-X
        //     ~~~~ ~~~~

        /// <summary>
        /// Turn off both system oscillator and LED duty cycle generator (default)
        /// </summary>
        SYS_DIS = 0b_0000_0000,

        /// <summary>
        /// Turn on system oscillator
        /// </summary>
        SYS_EN = 0b_0000_0001,

        /// <summary>
        /// Turn off LED duty cycle generator (default)
        /// </summary>
        LED_Off = 0b_0000_0010,

        /// <summary>
        /// Turn on LED duty cycle generator
        /// </summary>
        LED_On = 0b_0000_0011,

        /// <summary>
        /// Turn off blinking function (default)
        /// </summary>
        BLINK_Off = 0b_0000_1000,

        /// <summary>
        /// Turn on blinking function
        /// </summary>
        BLINK_On = 0b_0000_1001,

        /// <summary>
        /// Set slave mode and clock source from external clock, the system clock input from OSC pin and synchronous signal input from SYN pin
        /// </summary>
        SLAVE_Mode = 0b_0001_0000,

        /// <summary>
        /// Set master mode and clock source from on-chip RC oscillator, the system clock output to OSC pin and synchronous signal output to SYN pin (default)
        /// </summary>
        RC_Master_Mode = 0b_0001_1000,

        /// <summary>
        /// Set master mode and clock source from external clock, the system clock input from OSC pin and synchronous signal output to SYN pin
        /// </summary>
        EXT_CLK_Master_Mode = 0b_0001_1100,

        /// <summary>
        /// N-MOS open drain output and 8 COM option (default)
        /// </summary>
        COM_Option_N8 = 0b_0010_0000,

        /// <summary>
        /// N-MOS open drain output and 16 COM option
        /// </summary>
        COM_Option_N16 = 0b_0010_0100,

        /// <summary>
        /// P-MOS open drain output and 8 COM option
        /// </summary>
        COM_Option_P8 = 0b_0010_1000,

        /// <summary>
        /// P-MOS open drain output and 16 COM option
        /// </summary>
        COM_Option_P16 = 0b_0010_1100,

        /// <summary>
        /// PWM 1/16 duty
        /// </summary>
        PWM_Duty_1 = 0b_1010_0000,

        /// <summary>
        /// PWM 2/16 duty
        /// </summary>
        PWM_Duty_2 = 0b_1010_0001,

        /// <summary>
        /// PWM 3/16 duty
        /// </summary>
        PWM_Duty_3 = 0b_1010_0010,

        /// <summary>
        /// PWM 4/16 duty
        /// </summary>
        PWM_Duty_4 = 0b_1010_0011,

        /// <summary>
        /// PWM 5/16 duty
        /// </summary>
        PWM_Duty_5 = 0b_1010_0100,

        /// <summary>
        /// PWM 6/16 duty
        /// </summary>
        PWM_Duty_6 = 0b_1010_0101,

        /// <summary>
        /// PWM 7/16 duty
        /// </summary>
        PWM_Duty_7 = 0b_1010_0110,

        /// <summary>
        /// PWM 8/16 duty
        /// </summary>
        PWM_Duty_8 = 0b_1010_0111,

        /// <summary>
        /// PWM 9/16 duty
        /// </summary>
        PWM_Duty_9 = 0b_1010_1000,

        /// <summary>
        /// PWM 10/16 duty
        /// </summary>
        PWM_Duty_10 = 0b_1010_1001,

        /// <summary>
        /// PWM 11/16 duty
        /// </summary>
        PWM_Duty_11 = 0b_1010_1010,

        /// <summary>
        /// PWM 12/16 duty
        /// </summary>
        PWM_Duty_12 = 0b_1010_1011,

        /// <summary>
        /// PWM 13/16 duty
        /// </summary>
        PWM_Duty_13 = 0b_1010_1100,

        /// <summary>
        /// PWM 14/16 duty
        /// </summary>
        PWM_Duty_14 = 0b_1010_1101,

        /// <summary>
        /// PWM 15/16 duty
        /// </summary>
        PWM_Duty_15 = 0b_1010_1110,

        /// <summary>
        /// PWM 16/16 duty (default)
        /// </summary>
        PWM_Duty_16 = 0b_1010_1111,
    }
}
