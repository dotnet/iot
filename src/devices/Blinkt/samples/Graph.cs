// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// Graphs a sine wave.
    /// </summary>
    public class Graph : IDisposable
    {
        private readonly IBlinkt _blinkt = new Blinkt();

        private readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal void Run()
        {
            while (!Console.KeyAvailable)
            {
                TimeSpan t = DateTime.UtcNow - _epoch;
                double seconds = t.TotalSeconds;
                double value = (Math.Sin(seconds) + 1) / 2;
                ShowGraph(value, 255, 0, 255);
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }

        private void ShowGraph(double value, byte red, byte green, byte blue)
        {
            value *= Blinkt.NumPixels;

            for (var x = 0; x < Blinkt.NumPixels; x++)
            {
                if (value < 0)
                {
                    red = 0;
                    green = 0;
                    blue = 0;
                }
                else
                {
                    red = (byte)(Math.Min(value, 1) * red);
                    green = (byte)(Math.Min(value, 1) * green);
                    blue = (byte)(Math.Min(value, 1) * blue);
                }

                _blinkt.SetPixel(x, red, green, blue);
                value -= 1;
            }

            _blinkt.Show();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _blinkt.Dispose();
        }
    }
}
