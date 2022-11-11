// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Tca954x
{
    /// <summary>
    /// Available channels
    /// </summary>
    [Flags]
    public enum MultiplexerChannel : byte
    {
        /// <summary>
        /// No channel is selected
        /// </summary>
        None = 0,

        /// <summary>
        /// Channel 0 Byte (2^0 = 1)
        /// </summary>
        Channel0 = 0x01,

        /// <summary>
        /// Channel 1 Byte (2^1 = 2)
        /// </summary>
        Channel1 = 0x02,

        /// <summary>
        /// Channel 2 Byte (2^2 = 4)
        /// </summary>
        Channel2 = 0x04,

        /// <summary>
        /// Channel 3 Byte (2^3 = 8)
        /// </summary>
        Channel3 = 0x08,

        /// <summary>
        /// Channel 4 Byte (2^4 = 16)
        /// </summary>
        Channel4 = 0x10,

        /// <summary>
        /// Channel 5 Byte (2^5 = 32)
        /// </summary>
        Channel5 = 0x20,

        /// <summary>
        /// Channel 6 Byte (2^6 = 64)
        /// </summary>
        Channel6 = 0x40,

        /// <summary>
        /// Channel 7 Byte (2^7 = 128)
        /// </summary>
        Channel7 = 0x80
    }

}
