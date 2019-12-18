// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The GPIO state of the GPIO located on the P7 port
    /// Those ports can be used as external GPIO ports when SPI is not used
    /// Refer to the detailed documentation page 79 and 80
    /// </summary>
    [Flags]
    public enum Port7
    {
        /// <summary>
        /// P72
        /// </summary>
        P72 = 0b0000_0100,

        /// <summary>
        /// P71
        /// </summary>
        P71 = 0b0000_0010,
    }
}
