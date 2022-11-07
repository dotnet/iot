// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
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

            var converter = new ColorSpaceConverter();
            float angle = 0.0f;
            float hv = 1.0f;
            float hvDelta = 0; // 0.001f;

            // We need to take green and blue a bit back, because the red led is a bit darker than what it should be
            float greenCorrection = 0.7f;
            float blueCorrection = 0.95f;
            while (!Console.KeyAvailable)
            {
                var hsv = new Hsv(angle, 1.0f, 1.0f);
                var rgb = converter.ToRgb(hsv);
                redChannel.DutyCycle = rgb.R;
                blueChannel.DutyCycle = rgb.B * blueCorrection;
                greenChannel.DutyCycle = rgb.G * greenCorrection;

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
    }
}
