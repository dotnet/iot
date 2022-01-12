// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device
{
    public abstract class LedStrip : IDisposable
    {
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

        public abstract void Flush();

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