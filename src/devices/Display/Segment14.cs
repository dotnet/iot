// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Individual segment bits
    /// </summary>
    /// <remarks>
    ///  -----8-----
    /// |\    |    /|
    /// | \   |   / |
    /// |  0  1  2  |
    /// 13  \ | /   14
    /// |    \|/    |
    /// |-15-----16-|
    /// |    /|\    |
    /// 12   / | \  10
    /// |  3  4  5  |
    /// | /   |   \ |
    ///  ----11----- .6
    ///
    /// Sources:
    /// Derived from /src/devices/Display/FontHelper.cs
    /// </remarks>
    [Flags]
    public enum Segment14 : ushort
    {
        /// <summary>
        /// No segment
        /// </summary>
        None = 0b0000_0000_0000_0000,

        /// <summary>
        /// NorthWest
        /// </summary>
        NorthWest = 0b000_0000_0000_0001,

        /// <summary>
        /// Nort
        /// </summary>
        North = 0b000_0000_0000_0010,

        /// <summary>
        /// NorthEast
        /// </summary>
        NorthEast = 0b000_0000_0000_0100,

        /// <summary>
        /// SouthWest
        /// </summary>
        SouthWest = 0b000_0000_0000_1000,

        /// <summary>
        /// SoutH
        /// </summary>
        South = 0b000_0000_0001_0000,

        /// <summary>
        /// SouthEast
        /// </summary>
        SouthEast = 0b000_0000_0010_0000,

        /// <summary>
        /// Dot
        /// </summary>
        FullStop = 0b0000_0000_0100_0000,

        /// <summary>
        /// Top segment
        /// </summary>
        Top = 0b0000_0001_0000_0000,

        /// <summary>
        /// Top right segment
        /// </summary>
        TopRight = 0b0000_0010_0000_0000,

        /// <summary>
        /// Bottom right segment
        /// </summary>
        BottomRight = 0b0000_0100_0000_0000,

        /// <summary>
        /// Bottom segment
        /// </summary>
        Bottom = 0b0000_1000_0000_0000,

        /// <summary>
        /// Bottom left segment
        /// </summary>
        BottomLeft = 0b0001_0000_0000_0000,

        /// <summary>
        /// Top left segment
        /// </summary>
        TopLeft = 0b0010_0000_0000_0000,

        /// <summary>
        /// West
        /// </summary>
        West = 0b0100_0000_0000_0000,

        /// <summary>
        /// East
        /// </summary>
        East = 0b1000_0000_0000_0000,

        /// <summary>
        /// Middle segment
        /// </summary>
        Middle = East | West,

        /// <summary>
        /// Whole left segment
        /// </summary>
        Left = TopLeft | BottomLeft,

        /// <summary>
        /// Whole right segment
        /// </summary>
        Right = TopRight | BottomRight,

        /// <summary>
        /// Whole right segment
        /// </summary>
        Center = North | South,

        /// <summary>
        /// Forward Slash
        /// </summary>
        ForwardSlash = NorthEast | SouthWest,

        /// <summary>
        /// Back Slash
        /// </summary>
        BackSlash = NorthWest | SouthEast

    }
}
