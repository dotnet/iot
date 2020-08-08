// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Threading;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// Graphs a rainbow sin wave.
    /// </summary>
    public class GradientGraph : IDisposable
    {
        private const int HueRange = 270;
        private const int HueStart = 0;
        private const double MaxBrightness = 0.2;

        private readonly IBlinkt _blinkt = new Blinkt();

        internal void Run()
        {
            _blinkt.SetBrightness(MaxBrightness);
            while (!Console.KeyAvailable)
            {
                double t = (DateTimeOffset.Now - DateTimeOffset.UnixEpoch).TotalSeconds;
                double seconds = t * 2;
                double v = (Math.Sin(seconds) + 1) / 2;

                ShowGraph(v);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
            }
        }

        private void ShowGraph(double value)
        {
            value *= Blinkt.NumPixels;
            for (var x = 0; x < Blinkt.NumPixels; x++)
            {
                double hue = ((HueStart + (x / (double)Blinkt.NumPixels) * HueRange) % 360) / 360.0;
                Color color = ColorExtensions.HsvToRgb(hue, 1.0, 1.0);

                double brightness = value < 0 ? 0 : Math.Min(value, 1.0) * MaxBrightness;

                _blinkt.SetPixel(x, color.R, color.G, color.B, brightness);
                value--;
            }

            _blinkt.Show();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _blinkt?.Dispose();
        }
    }
}
