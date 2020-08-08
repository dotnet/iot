// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Threading;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// Cycles through all the colors.
    /// </summary>
    public class Rainbow : IDisposable
    {
        private readonly IBlinkt _blinkt = new Blinkt();

        internal void Run()
        {
            while (!Console.KeyAvailable)
            {
                double hue = (DateTimeOffset.Now - DateTimeOffset.UnixEpoch).TotalSeconds * 100 % 360.0;

                for (var x = 0; x < Blinkt.NumPixels; x++)
                {
                    double offset = x * 22.5;
                    double h = (hue + offset) % 360 / 360.0;
                    Color color = ColorExtensions.HsvToRgb(h, 1, 1);
                    _blinkt.SetPixel(x, color.R, color.G, color.B);
                }

                _blinkt.Show();
                Thread.Sleep(TimeSpan.FromSeconds(0.001));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _blinkt.Dispose();
        }
    }
}
