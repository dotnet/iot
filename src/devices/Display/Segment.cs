// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Individual segment bits
    /// </summary>
    /// <remarks>
    ///  --0--
    /// |     |
    /// 5     1
    /// |     |
    ///  --6--
    /// |     |
    /// 4     2
    /// |     |
    ///  --3--   . 7
    /// </remarks>
    [Flags]
    public enum Segment : byte
    {
        /// <summary>
        /// No segment
        /// </summary>
        None = 0b0000_0000,

        /// <summary>
        /// Top segment
        /// </summary>
        Top = 0b0000_0001,

        /// <summary>
        /// Top right segment
        /// </summary>
        TopRight = 0b0000_0010,

        /// <summary>
        /// Bottom right segment
        /// </summary>
        BottomRight = 0b0000_0100,

        /// <summary>
        /// Bottom segment
        /// </summary>
        Bottom = 0b0000_1000,

        /// <summary>
        /// Bottom left segment
        /// </summary>
        BottomLeft = 0b0001_0000,

        /// <summary>
        /// Top left segment
        /// </summary>
        TopLeft = 0b0010_0000,

        /// <summary>
        /// Middle segment
        /// </summary>
        Middle = 0b0100_0000,

        /// <summary>
        /// Dot
        /// </summary>
        Dot = 0b1000_0000
    }
}
