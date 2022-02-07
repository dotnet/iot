// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device
{
    /// <summary>
    /// Basic class for LED strips
    /// </summary>
    public abstract class LedStrip : IDisposable
    {
        /// <summary>
        /// Initializes LED strip with specified count of LEDs
        /// </summary>
        /// <param name="length">count of LEDs</param>
        public LedStrip(int length)
        {
            Pixels = new Color[length];
        }

        /// <summary>
        /// Color correction parameter
        /// </summary>
        public double Gamma { get; set; } = 2.2;

        /// <summary>
        /// Colors of LEDs
        /// </summary>
        public Color[] Pixels { get; private set; }

        /// <summary>
        /// Update color data to LEDs
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Calculate corrected color value using <seealso cref="Gamma"/>
        /// </summary>
        /// <param name="v">raw value</param>
        /// <returns>corrected value</returns>
        internal byte FixGamma(byte v)
        {
            return (byte)Math.Round(Math.Pow(v / 255.0, Gamma) * 255);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Pixels = null!;
        }
    }
}
