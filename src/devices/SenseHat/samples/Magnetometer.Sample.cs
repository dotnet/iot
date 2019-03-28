// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Threading;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

namespace Iot.Device.SenseHat.Samples
{
    internal class Magnetometer
    {
        public static void Run()
        {
            using (var magnetometer = new SenseHatMagnetometer())
            using (var ledMatrix = new SenseHatLedMatrixI2c())
            {
                Console.WriteLine("Move SenseHat around in every direction until dot on the LED matrix stabilizes when not moving.");
                ledMatrix.Fill();
                Stopwatch sw = Stopwatch.StartNew();
                Vector3 min = magnetometer.MagneticInduction;
                Vector3 max = magnetometer.MagneticInduction;
                while (min == max)
                {
                    Vector3 sample = magnetometer.MagneticInduction;
                    min = Vector3.Min(min, sample);
                    max = Vector3.Max(max, sample);
                    Thread.Sleep(50);
                }

                const int intervals = 8;
                Color[] data = new Color[64];

                while (true)
                {
                    Vector3 sample = magnetometer.MagneticInduction;
                    min = Vector3.Min(min, sample);
                    max = Vector3.Max(max, sample);
                    Vector3 size = max - min;
                    Vector3 pos = Vector3.Divide(Vector3.Multiply((sample - min), intervals - 1), size);
                    int x = Math.Clamp((int)pos.X, 0, intervals - 1);

                    // reverse y to match magnetometer coordinate system
                    int y = intervals - 1 - Math.Clamp((int)pos.Y, 0, intervals - 1);
                    int idx = SenseHatLedMatrix.PositionToIndex(x, y);

                    // fading
                    for (int i = 0; i < 64; i++)
                    {
                        data[i] = Color.FromArgb((byte)Math.Clamp(data[i].R - 1, 0, 255), data[i].G, data[i].B);;
                    }

                    Color col = data[idx];
                    col = Color.FromArgb(Math.Clamp(col.R + 20, 0, 255), col.G, col.B);
                    Vector2 pos2 = new Vector2(sample.X, sample.Y);
                    Vector2 center2 = Vector2.Multiply(new Vector2(min.X + max.X, min.Y + max.Y), 0.5f);
                    float max2 = Math.Max(size.X, size.Y);
                    float distFromCenter = (pos2 - center2).Length();

                    data[idx] = Color.FromArgb(0, 255, (byte)Math.Clamp(255 * distFromCenter / max2, 0, 255));

                    ledMatrix.Write(data);
                    data[idx] = col;

                    Thread.Sleep(50);
                }
            }
        }
    }
}
