// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// ADC Pin output current setting
    /// </summary>
    public enum AdcPinCurrent
    {
        /// <summary>20 uA</summary>
        MicroAmperes20 = 0b0000_0000,

        /// <summary>40 uA</summary>
        MicroAmperes40 = 0b0001_0000,

        /// <summary>60 uA</summary>
        MicroAmperes60 = 0b0010_0000,

        /// <summary>80 uA</summary>
        MicroAmperes80 = 0b0011_0000,
    }
}
