// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.SenseHat.Samples
{
    internal class LedMatrix
    {
        public static void Run()
        {
            // another implementation which can be used is: SenseHatLedMatrixSysFs
            // I2C implementation does not require installing anything
            // SysFs implementation is faster and has arguably better colors
            using (var m = new SenseHatLedMatrixI2c())
            {
                WriteDemo(m);
                // SetPixelDemo(m);
            }
        }

        // Not used by default but much simpler to understand
        static void SetPixelDemo(SenseHatLedMatrix m)
        {
            m.Clear(Color.Purple);

            m.SetPixel(0, 0, Color.Red);
            m.SetPixel(1, 0, Color.Green);
            m.SetPixel(2, 0, Color.Blue);

            for (int i = 1; i <= 7; i++)
            {
                m.SetPixel(i, i, Color.White);
            }
        }

        static void WriteDemo(SenseHatLedMatrix m)
        {
            Color[] colors = new Color[SenseHatLedMatrix.NumberOfPixels];

            Stopwatch sw = Stopwatch.StartNew();

            int frames = 0;
            while (true)
            {
                float time = sw.ElapsedMilliseconds / 1000.0f;
                Frame(colors, time);
                m.Write(colors);

                frames++;
                if (frames % 200 == 0 && time > 1.0)
                {
                    Console.WriteLine($"Average FPS: {frames / time}");
                }

                Thread.Sleep(30);
            }
        }

        static void Frame(Span<Color> colors, float time)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                (int x, int y) = SenseHatLedMatrix.IndexToPosition(i);
                colors[i] = Pixel(new Vector2(x / 8.0f, y / 8.0f), time);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Color Pixel(Vector2 uv, float time)
        {
            return Color.FromArgb(
                Col(0.5 + 0.5 * Math.Cos(time + uv.X)),
                Col(0.5 + 0.5 * Math.Cos(time + uv.Y + 2.0)),
                Col(0.5 + 0.5 * Math.Cos(time + uv.X + 4.0))
            );
        }

        static byte Col(double x)
        {
            x = Math.Clamp(x, 0.0f, 1.0f);
            return (byte)(x * 255);
        }
    }
}
