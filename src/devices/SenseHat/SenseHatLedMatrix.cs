// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices; 

namespace Iot.Device.SenseHat
{
    public abstract class SenseHatLedMatrix : IDisposable
    {
        public const int NumberOfPixels = 64;
        public const int NumberOfPixelsPerRow = 8;

        // does not need to be public since it should not be used
        protected const int NumberOfPixelsPerColumn = 8;

        protected SenseHatLedMatrix()
        {
        }

        public abstract void Write(ReadOnlySpan<Color> colors);

        public abstract void Clear(Color color = default(Color));

        public abstract void SetPixel(int x, int y, Color color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int x, int y) IndexToPosition(int index)
        {
            return (index % NumberOfPixelsPerRow, index / NumberOfPixelsPerRow);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int x, int y)
        {
            Debug.Assert(x >= 0 && x < NumberOfPixelsPerRow);
            Debug.Assert(y >= 0 && y < NumberOfPixelsPerColumn);
            return x + y * NumberOfPixelsPerRow;
        }

        public abstract void Dispose();
    }
}
