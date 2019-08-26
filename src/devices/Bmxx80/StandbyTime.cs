// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Controls the inactive duration in normal mode.
    /// </summary>
    public enum StandbyTime : byte
    {
        /// <summary>
        /// 0.5 ms.
        /// </summary>
        Ms0_5 = 0b000,

        /// <summary>
        /// 62.5 ms.
        /// </summary>
        Ms62_5 = 0b001,

        /// <summary>
        /// 125 ms.
        /// </summary>
        Ms125 = 0b010,

        /// <summary>
        /// 250 ms.
        /// </summary>
        Ms250 = 0b011,

        /// <summary>
        /// 500 ms.
        /// </summary>
        Ms500 = 0b100,

        /// <summary>
        /// 1,000 ms.
        /// </summary>
        Ms1000 = 0b101,

        /// <summary>
        /// 10 ms.
        /// </summary>
        Ms10 = 0b110,

        /// <summary>
        /// 20 ms.
        /// </summary>
        Ms20 = 0b111,
    }
}
