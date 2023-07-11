// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Threading.Thread;

namespace Iot.Device.Arduino.Sample
{
    internal class RgbLedTest
    {
        private readonly ArduinoBoard _board;

        public RgbLedTest(ArduinoBoard board)
        {
            _board = board;
        }

        public void DoTest()
        {
            using var redChannel = _board.CreatePwmChannel(0, 25, 4000, 0);
            using var greenChannel = _board.CreatePwmChannel(0, 33, 4000, 0);
            using var blueChannel = _board.CreatePwmChannel(0, 32, 4000, 0);

            redChannel.Start();
            greenChannel.Start();
            blueChannel.Start();

            redChannel.DutyCycle = 1.0;
            Sleep(1000);
            redChannel.DutyCycle = 0;
            blueChannel.DutyCycle = 1.0;
            Sleep(1000);
            blueChannel.DutyCycle = 0.0;
            greenChannel.DutyCycle = 1.0;
            Sleep(1000);
            blueChannel.DutyCycle = 1.0;
            redChannel.DutyCycle = 1.0;
            Sleep(1000);
            greenChannel.DutyCycle = 0;
            blueChannel.DutyCycle = 0;
            redChannel.DutyCycle = 0;
            Sleep(1000);

            float angle = 0.0f;
            float hv = 1.0f;
            float hvDelta = 0; // 0.001f;

            // We need to take green and blue a bit back, because the red led is a bit darker than what it should be
            float greenCorrection = 0.7f;
            float blueCorrection = 0.95f;
            while (!Console.KeyAvailable)
            {
                Color rgb = ConvertHsvToRgb(angle, 1.0f, 1.0f);
                redChannel.DutyCycle = rgb.R / 255.0;
                blueChannel.DutyCycle = rgb.B * blueCorrection / 255.0;
                greenChannel.DutyCycle = rgb.G * greenCorrection / 255.0;

                angle = (angle + 1.0f) % 360.0f;
                hv += hvDelta;
                if (hv < 0)
                {
                    hv = 0;
                    hvDelta = -hvDelta;
                }

                if (hv > 1.0f)
                {
                    hv = 1.0f;
                    hvDelta = -hvDelta;
                }

                Sleep(100);
            }

            Console.ReadKey(true);
        }

        private Color ConvertHsvToRgb(float hue, float saturation, float brightness)
        {
            if (hue < 0 || hue >= 360)
            {
                throw new ArgumentOutOfRangeException(nameof(hue));
            }

            if (saturation < 0 || saturation > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(saturation));
            }

            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness));
            }

            double c = brightness * saturation;
            double x = c * (1 - Math.Abs((hue / 60.0 % 2) - 1));
            double m = brightness - c;

            var (r, g, b) = hue switch
            {
                < 60 => (c, x, 0),
                >= 60 and < 120 => (x, c, 0),
                >= 120 and < 180 => (0, c, x),
                >= 180 and < 240 => (0, x, c),
                >= 240 and < 300 => (x, 0, c),
                >= 300 => (c, 0, x),
                _ => (0.0, 0.0, 0.0)
            };

            return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
        }
    }
}
