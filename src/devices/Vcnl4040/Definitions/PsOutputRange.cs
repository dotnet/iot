// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS output range settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum PsOutputRange : byte
    {
        /// <summary>
        /// PS output range is 12 bits long
        /// </summary>
        Bits12 = 0b0000_0000,

        /// <summary>
        /// PS output range is 16 bits long
        /// </summary>
        Bits16 = 0b0000_1000
    }
}
