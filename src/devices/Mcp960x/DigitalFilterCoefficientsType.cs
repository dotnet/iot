// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the digitial filter type
    /// </summary>
    public enum DigitalFilterCoefficientsType : byte
    {
        /// <summary>
        /// Type Filter off
        /// </summary>
        N0 = 0b0000_0000,

        /// <summary>
        /// Type Filter minimum
        /// </summary>
        N1 = 0b0000_0001,

        /// <summary>
        /// Type Filter minimum  + 1
        /// </summary>
        N2 = 0b0000_0010,

        /// <summary>
        /// Type Filter minimum  + 1
        /// </summary>
        N3 = 0b0000_0011,

        /// <summary>
        /// Type Filter Mid
        /// </summary>
        N4 = 0b0000_0100,

        /// <summary>
        /// Type Filter Mid + 1
        /// </summary>
        N5 = 0b0000_0101,

        /// <summary>
        /// Type Filter Mid + 2
        /// </summary>
        N6 = 0b0000_0110,

        /// <summary>
        /// Type Filter maximum
        /// </summary>
        N7 = 0b0000_0111,
    }
}
