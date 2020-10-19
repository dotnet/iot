// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// 5V Power regulator supplies 5V power Rail with up to 2.5A of continuous current
    /// </summary>
    public enum PowerRegulatorMode
    {
        /// <summary>
        /// This mode switches between DCDC switching mode and LDO mode with most of time in DCDC switching mode. This is mode has high efficiency but increased voltage ripple
        /// </summary>
        PowerSourceDetection = 0,

        /// <summary>
        /// This mode regulates 5V Rail voltage to 4.79V. In this mode output voltage has lowest output ripple
        /// </summary>
        LowDropout,

        /// <summary>
        /// This mode 5V Rail voltage is regulated to 5V with 2.5% tolerance, typically 5.07V at mid-loaded conditions. This is the most efficient operation mode
        /// </summary>
        DirectCurrentDirectCurrent
    }
}
