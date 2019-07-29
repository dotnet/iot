// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        P35 = 0b0010_0000,
        P34 = 0b0001_0000,
        P33 = 0b0000_1000,
        P32 = 0b0000_0100,
        P31 = 0b0000_0010,
        P30 = 0b0000_0001,
    }
}
