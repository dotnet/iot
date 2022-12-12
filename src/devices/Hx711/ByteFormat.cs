// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Byte order ("endianness") in an architecture
    /// </summary>
    internal enum ByteFormat
    {
        /// <summary>
        /// Less Significant Bit (aka Little-endian) byte format sequence
        /// </summary>
        Lsb = 0,

        /// <summary>
        /// Most Significant Bit (aka Big-endian) byte format sequence
        /// </summary>
        Msb = 1,
    }
}
