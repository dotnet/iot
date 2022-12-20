// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Interface for basic LED matrix functionality.
    /// </summary>
    public interface IMatrix
    {
        /// <summary>
        /// Width (x-axis) of LED matrix.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height (y-axis) of LED matrix.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Indexer for matrix.
        /// </summary>
        int this[int x, int y] { get; set; }

        /// <summary>
        /// Fill LEDs (0 is dark; 255 is all lit).
        /// </summary>
        public void Fill(int value);
    }
}
