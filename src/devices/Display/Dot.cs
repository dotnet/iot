// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Dot display setup
    /// </summary>
    /// <remarks>Specific to device <see cref="Large4Digit7SegmentDisplay"/></remarks>
    [Flags]
    public enum Dot : byte
    {
        /// <summary>
        /// Disable all dots
        /// </summary>
        Off = 0b0000_0000,

        /// <summary>
        /// Center colon
        /// </summary>
        CenterColon = 0b0000_0010,

        /// <summary>
        /// Left colon
        /// </summary>
        LeftColon = LeftUpper | LeftLower,

        /// <summary>
        /// Left upper dot
        /// </summary>
        LeftUpper = 0b0000_0100,

        /// <summary>
        /// Left lower dot
        /// </summary>
        LeftLower = 0b0000_1000,

        /// <summary>
        /// Decimal point (between third and fourth digits)
        /// </summary>
        DecimalPoint = 0b0001_0000
    }
}
