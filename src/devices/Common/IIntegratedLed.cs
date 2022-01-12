// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device
{
    public abstract class IntegratedLed : IDisposable
    {
        /// <summary>
        /// Color correction parameter
        /// </summary>
        public double Gamma { get; set; } = 2.2;

        /// <summary>
        /// Colors of LEDs
        /// </summary>
        public Span<Color> Pixels => _pixels;

        public abstract void Flush();

        protected Color[] _pixels;

        protected byte FixGamma(byte v)
        {
            return (byte)(Math.Pow(v/255.0, Gamma)*255);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _pixels = null!;
        }
    }
}