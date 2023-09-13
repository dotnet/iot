// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Power Status
    /// </summary>
    [Flags]
    public enum PowerStatus
    {
        /// <summary>Acin Exists</summary>
        AcinExists = 0b1000_0000,

        /// <summary>Acin Available</summary>
        AcinAvailable = 0b0100_0000,

        /// <summary>Vbus Exists</summary>
        VbusExists = 0b0010_0000,

        /// <summary>Vbus Available</summary>
        VbusAvailable = 0b0001_0000,

        /// <summary>Vbus Large Vhold</summary>
        VbusLargeVhold = 0b0000_1000,

        /// <summary>Battery Charged</summary>
        BatteryCharged = 0b0000_0100,

        /// <summary>Acin Vbus Shorted</summary>
        AcinVbusShorted = 0b0000_0010,

        /// <summary>Acin Vbus Startup Source</summary>
        AcinVbusStartupSource = 0b0000_0001,
    }
}
