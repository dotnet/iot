// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// GPIO0 behavior
    /// </summary>
    public enum Gpio0Behavior
    {
        /// <summary>NMOS Leak Open Output</summary>
        NmosLeakOpenOutput = 0,

        /// <summary>Universal Input Function</summary>
        UniversalInputFunction = 1,

        /// <summary>Low Noise LDO</summary>
        LowNoiseLDO = 2,

        /// <summary>Current Absorption</summary>
        CurrentAbsorption = 3,

        /// <summary>ADC Input</summary>
        AdcInput = 4,

        /// <summary>Low Output</summary>
        LowOutput = 5,

        /// <summary>Floating</summary>
        Floating = 6,
    }
}
