// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// ADC Pin Current Setting
    /// /// </summary>
    public enum AdcPinCurrentSetting
    {
        /// <summary>Off</summary>
        Off = 0b0000_0000,

        /// <summary>When on charge</summary>
        OnCharge = 0b0000_0001,

        /// <summary>When sampling</summary>
        SavingPower = 0b0000_0010,

        /// <summary>Always on</summary>
        AlwaysOn = 0b0000_0011,
    }
}
