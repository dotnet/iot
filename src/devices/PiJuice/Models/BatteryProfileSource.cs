// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery profile source
    /// </summary>
    public enum BatteryProfileSource
    {
        /// <summary>
        /// Host
        /// </summary>
        Host = 0,

        /// <summary>
        /// PiJuice DIP Switch
        /// </summary>
        DIPSwitch,

        /// <summary>
        /// Resistor
        /// </summary>
        Resistor
    }
}
