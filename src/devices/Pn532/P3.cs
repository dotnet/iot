// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The GPIO state of the GPIO located on the P3 port
    /// Most of those ports can be used as external GPIO ports
    /// Refer to the detailed documentation page 79 and 80
    /// </summary>
    [Flags]
    public enum Port3
    {
        /// <summary>
        /// P35
        /// </summary>
        P35 = 0b0010_0000,

        /// <summary>
        /// P34
        /// </summary>
        P34 = 0b0001_0000,

        /// <summary>
        /// P33
        /// </summary>
        P33 = 0b0000_1000,

        /// <summary>
        /// P32
        /// </summary>
        P32 = 0b0000_0100,

        /// <summary>
        /// P31
        /// </summary>
        P31 = 0b0000_0010,

        /// <summary>
        /// P30
        /// </summary>
        P30 = 0b0000_0001,
    }
}
