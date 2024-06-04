// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tm16xx
{
    /// <summary>
    /// Specifies the segment of the screen.
    /// </summary>
    public enum LedSegment
    {
        /// <summary>
        /// 7-Segment without dot.
        /// </summary>
        Led7Segment,

        /// <summary>
        /// 8-Segment with dot.
        /// </summary>
        Led8Segment,

        /// <summary>
        /// 16-Segment. Each pair of bytes represent a character to display.
        /// </summary>
        Led16Segment
    }
}
