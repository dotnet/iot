// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Tm1637
{
    /// <summary>
    ///
    /// bit 0 = a       _a_
    /// bit 1 = b      |   |
    /// bit 2 = c      f   b
    /// bit 3 = d      |_g_|
    /// bit 4 = e      |   |
    /// bit 5 = f      e   c
    /// bit 6 = g      |_d_|  .dp
    /// bit 7 = dp
    ///
    /// </summary>
    [Flags]
    public enum Character : byte
    {
        /// <summary>
        /// Character representing nothing being displayed
        /// </summary>
        Nothing = 0b0000_0000,

        /// <summary>
        /// Segment a
        /// </summary>
        SegmentTop = 0b0000_0001,

        /// <summary>
        /// Segment b
        /// </summary>
        SegmentTopRight = 0b0000_0010,

        /// <summary>
        /// Segment c
        /// </summary>
        SegmentBottomRight = 0b0000_0100,

        /// <summary>
        /// Segment d
        /// </summary>
        SegmentBottom = 0b0000_1000,

        /// <summary>
        /// Segment e
        /// </summary>
        SegmentBottomLeft = 0b0001_0000,

        /// <summary>
        /// Segment f
        /// </summary>
        SegmentTopLeft = 0b0010_0000,

        /// <summary>
        /// Segment g
        /// </summary>
        SegmentMiddle = 0b0100_0000,

        /// <summary>
        /// Segment dp
        /// </summary>
        Dot = 0b1000_0000,

        /// <summary>
        /// Character 0
        /// </summary>
        Digit0 = SegmentTop | SegmentTopRight | SegmentBottomRight | SegmentBottom | SegmentBottomLeft | SegmentTopLeft,

        /// <summary>
        /// Character 1
        /// </summary>
        Digit1 = SegmentTopRight | SegmentBottomRight,

        /// <summary>
        /// Character 2
        /// </summary>
        Digit2 = SegmentTop | SegmentTopRight | SegmentMiddle | SegmentBottomLeft | SegmentBottom,

        /// <summary>
        /// Character 3
        /// </summary>
        Digit3 = SegmentTop | SegmentTopRight | SegmentMiddle | SegmentBottomRight | SegmentBottom,

        /// <summary>
        /// Character 4
        /// </summary>
        Digit4 = SegmentTopLeft | SegmentMiddle | SegmentTopRight | SegmentBottomRight,

        /// <summary>
        /// Character 5
        /// </summary>
        Digit5 = SegmentTop | SegmentTopLeft | SegmentMiddle | SegmentBottomRight | SegmentBottom,

        /// <summary>
        /// Character 6
        /// </summary>
        Digit6 = SegmentTop | SegmentTopLeft | SegmentMiddle | SegmentBottomRight | SegmentBottom | SegmentBottomLeft,

        /// <summary>
        /// Character 7
        /// </summary>
        Digit7 = SegmentTop | SegmentTopRight | SegmentBottomRight,

        /// <summary>
        /// Character 8
        /// </summary>
        Digit8 = SegmentTop | SegmentTopLeft | SegmentTopRight | SegmentMiddle | SegmentBottom | SegmentBottomLeft | SegmentBottomRight,

        /// <summary>
        /// Character 9
        /// </summary>
        Digit9 = SegmentTop | SegmentTopLeft | SegmentTopRight | SegmentMiddle | SegmentBottom | SegmentBottomRight,

        /// <summary>
        /// Character A
        /// </summary>
        A = SegmentTop | SegmentTopLeft | SegmentTopRight | SegmentMiddle | SegmentBottomLeft | SegmentBottomRight,

        /// <summary>
        /// Character B
        /// </summary>
        B = SegmentTopLeft | SegmentMiddle | SegmentBottom | SegmentBottomLeft | SegmentBottomRight,

        /// <summary>
        /// Character C
        /// </summary>
        C = SegmentTop | SegmentTopLeft | SegmentBottomLeft | SegmentBottom,

        /// <summary>
        /// Character D
        /// </summary>
        D = SegmentTopRight | SegmentMiddle | SegmentBottom | SegmentBottomLeft | SegmentBottomRight,

        /// <summary>
        /// Character E
        /// </summary>
        E = SegmentMiddle | SegmentTopLeft | SegmentBottomLeft | SegmentBottom | SegmentTop,

        /// <summary>
        /// Character F
        /// </summary>
        F = SegmentMiddle | SegmentTopLeft | SegmentBottomLeft | SegmentTop,

        /// <summary>
        /// Character -
        /// </summary>
        Minus = SegmentMiddle,
    }
}
