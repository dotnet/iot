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
        SystemDisabled = 0b_0000_0000,

        /// <summary>
        /// Turn on system oscillator
        /// </summary>
        SystemEnabled = 0b_0000_0001,

        /// <summary>
        /// Turn off LED duty cycle generator (default)
        /// </summary>
        LedOff = 0b_0000_0010,

        /// <summary>
        /// Turn on LED duty cycle generator
        /// </summary>
        LedOn = 0b_0000_0011,

        /// <summary>
        /// Turn off blinking function (default)
        /// </summary>
        BlinkOff = 0b_0000_1000,

        /// <summary>
        /// Turn on blinking function
        /// </summary>
        BlinkOn = 0b_0000_1001,

        /// <summary>
        /// Set secondary mode and clock source from external clock, the system clock input from OSC pin and synchronous signal input from SYN pin
        /// </summary>
        Secondary = 0b_0001_0000,

        /// <summary>
        /// Set primary mode and clock source from on-chip RC oscillator, the system clock output to OSC pin and synchronous signal output to SYN pin (default)
        /// </summary>
        RcPrimary = 0b_0001_1000,

        /// <summary>
        /// Set primary mode and clock source from external clock, the system clock input from OSC pin and synchronous signal output to SYN pin
        /// </summary>
        ExternalClockPrimary = 0b_0001_1100,

        /// <summary>
        /// N-MOS open drain output and 8 COM option (default)
        /// </summary>
        NMos8Com = 0b_0010_0000,

        /// <summary>
        /// N-MOS open drain output and 16 COM option
        /// </summary>
        NMos16Com = 0b_0010_0100,

        /// <summary>
        /// P-MOS open drain output and 8 COM option
        /// </summary>
        PMos8Com = 0b_0010_1000,

        /// <summary>
        /// P-MOS open drain output and 16 COM option
        /// </summary>
        PMos16Com = 0b_0010_1100,

        /// <summary>
        /// PWM 1/16 duty
        /// </summary>
        PwmDuty1 = 0b_1010_0000,

        /// <summary>
        /// PWM 2/16 duty
        /// </summary>
        PwmDuty2 = 0b_1010_0001,

        /// <summary>
        /// PWM 3/16 duty
        /// </summary>
        PwmDuty3 = 0b_1010_0010,

        /// <summary>
        /// PWM 4/16 duty
        /// </summary>
        PwmDuty4 = 0b_1010_0011,

        /// <summary>
        /// PWM 5/16 duty
        /// </summary>
        PwmDuty5 = 0b_1010_0100,

        /// <summary>
        /// PWM 6/16 duty
        /// </summary>
        PwmDuty6 = 0b_1010_0101,

        /// <summary>
        /// PWM 7/16 duty
        /// </summary>
        PwmDuty7 = 0b_1010_0110,

        /// <summary>
        /// PWM 8/16 duty
        /// </summary>
        PwmDuty8 = 0b_1010_0111,

        /// <summary>
        /// PWM 9/16 duty
        /// </summary>
        PwmDuty9 = 0b_1010_1000,

        /// <summary>
        /// PWM 10/16 duty
        /// </summary>
        PwmDuty10 = 0b_1010_1001,

        /// <summary>
        /// PWM 11/16 duty
        /// </summary>
        PwmDuty11 = 0b_1010_1010,

        /// <summary>
        /// PWM 12/16 duty
        /// </summary>
        PwmDuty12 = 0b_1010_1011,

        /// <summary>
        /// PWM 13/16 duty
        /// </summary>
        PwmDuty13 = 0b_1010_1100,

        /// <summary>
        /// PWM 14/16 duty
        /// </summary>
        PwmDuty14 = 0b_1010_1101,

        /// <summary>
        /// PWM 15/16 duty
        /// </summary>
        PwmDuty15 = 0b_1010_1110,

        /// <summary>
        /// PWM 16/16 duty (default)
        /// </summary>
        PwmDuty16 = 0b_1010_1111,
    }
}
