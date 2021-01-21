// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Power input type for charging and supplying VSYS output
    /// </summary>
    public enum PowerInputType
    {
        /// <summary>
        /// USB micro input
        /// </summary>
        UsbMicro,

        /// <summary>
        /// GPIO 5V
        /// </summary>
        Gpio5Volt
    }
}
