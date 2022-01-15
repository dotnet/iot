using System;

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Flags related to the REG_MISC register
    /// </summary>
    [Flags]
    public enum MiscFlags : byte
    {
        /// <summary>
        /// Use internal clock
        /// </summary>
        ClockSourceSelection = 0b00000001,

        /// <summary>
        /// Use external clock
        /// </summary>
        ExternalClockDetection = 0b00000010,

        /// <summary>
        /// Enable PWM cycle power save
        /// </summary>
        PWMCyclePowersaveEnable = 0b00000100,

        /// <summary>
        /// Charge mode gain low bit
        /// </summary>
        ChargeModeGainLowBit = 0b00001000,

        /// <summary>
        /// Charge mode gain high bit
        /// </summary>
        ChargeModeGainHighBit = 0b00010000,

        /// <summary>
        /// Enable power save mode
        /// </summary>
        PowersaveModeEnable = 0b00100000,

        /// <summary>
        /// Enable auto address increment
        /// </summary>
        AddressAutoIncrementEnable = 0b01000000
    }
}
