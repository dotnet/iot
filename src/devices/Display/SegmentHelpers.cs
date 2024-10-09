// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using Iot.Device.Display;

namespace Display
{
    internal static class SegmentHelpers
    {
        // Dictionary allows key-based lookup
        private static readonly Dictionary<Segment, int> SegIndex = new Dictionary<Segment, int>()
        {
            { Segment.Top, 0 },
            { Segment.TopRight, 1 },
            { Segment.BottomRight, 2 },
            { Segment.Bottom, 3 },
            { Segment.BottomLeft, 4 },
            { Segment.TopLeft, 5 },
            { Segment.Middle, 6 },
            { Segment.Dot, 7 }, // DP is required for each digit but set independently
        };

        public static ReadOnlySpan<PinValue> GetPinValuesFromSegment(Segment segments)
        {
            var pinValues = Enumerable.Repeat(PinValue.Low, 8).ToArray();
            foreach (var kvp in SegIndex)
            {
                if (segments.HasFlag(kvp.Key))
                {
                    pinValues[kvp.Value] = 1;
                }
            }

            return pinValues;
        }
    }
}
