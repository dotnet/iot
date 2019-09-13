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
    /// <summary>
    /// Base class for SenseHAT LED matrix
    /// </summary>
    public abstract class SenseHatLedMatrix : IDisposable
    {
        /// <summary>
        /// Total number of pixels
        /// </summary>
        public const int NumberOfPixels = 64;

        /// <summary>
        /// Number of pixels per row
        /// </summary>
        public const int NumberOfPixelsPerRow = 8;

        // does not need to be public since it should not be used

        /// <summary>
        /// Number of pixels per column
        /// </summary>
        protected const int NumberOfPixelsPerColumn = 8;

        /// <summary>
        /// Constructs SenseHatLedMatrix instance
        /// </summary>
        protected SenseHatLedMatrix()
        {
        }

        /// <summary>
        /// Write colors to the device
        /// </summary>
        /// <param name="colors">Array of colors</param>
        public abstract void Write(ReadOnlySpan<Color> colors);

        /// <summary>
        /// Fill LED matrix with a specific color
        /// </summary>
        /// <param name="color">Color to fill the device with</param>
        public abstract void Fill(Color color = default(Color));

        /// <summary>
        /// Sets color on specific position of the LED matrix
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="color">Color to be set in the specified position</param>
        public abstract void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Translates position in the buffer to X, Y coordinates
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Tuple of X and Y coordinates</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int x, int y) IndexToPosition(int index)
        {
            if (index < 0 || index >= NumberOfPixelsPerRow * NumberOfPixelsPerRow)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (index % NumberOfPixelsPerRow, index / NumberOfPixelsPerRow);
        }

        /// <summary>
        /// Translate X and Y coordinates to position in the buffer
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Position in the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int x, int y)
        {
            if (x < 0 || x >= NumberOfPixelsPerRow)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= NumberOfPixelsPerColumn)
                throw new ArgumentOutOfRangeException(nameof(x));

            return x + y * NumberOfPixelsPerRow;
        }

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
