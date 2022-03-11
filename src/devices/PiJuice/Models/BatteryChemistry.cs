// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Type of Battery Chemistry
    /// </summary>
    public enum BatteryChemistry
    {
        /// <summary>
        /// LIPO
        /// </summary>
        Lipo = 0,

        /// <summary>
        /// LiFePO4
        /// </summary>
        LiFePO4,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 1000
    }
}
