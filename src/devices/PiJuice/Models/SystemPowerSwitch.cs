// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// VSYS on J3 to provide power to external devices
    /// </summary>
    public enum SystemPowerSwitch
    {
        /// <summary>
        /// VSYS pin is off
        /// </summary>
        Off = 0,

        /// <summary>
        /// VSYS pin provides up to 500 milliampere of power
        /// </summary>
        Power500mA = 500,

        /// <summary>
        /// VSYS pin provides up to 2100 milliampere of power
        /// </summary>
        Power2100mA = 2100
    }
}
