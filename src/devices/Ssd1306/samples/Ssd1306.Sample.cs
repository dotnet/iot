// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;

namespace Iot.Device.Ssd1306.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Ssd1306 Sample!");

            using (Ssd1306 ssd1306 = GetSsd1306WithI2c())
            {
                InitializeSsd1306(ssd1306);
            }
        }

        private static void InitializeSsd1306(Ssd1306 ssd1306)
        {
            ssd1306.SetDisplayOff();
            ssd1306.SetDisplayClockDivideRatioOscillatorFrequency(0x08, 0x00);
            ssd1306.SetMultiplexRatio(0x1F);
            ssd1306.SetDisplayOffset(0x00);
            ssd1306.SetDisplayStartLine(0x00);
            ssd1306.SetChargePump(true);
            ssd1306.SetSegmentReMap(false);
            ssd1306.SetComOutputScanDirection(false);
            ssd1306.SetContrastControlForBank0(0x8F);
            ssd1306.SetPreChargePeriod(0xF1);
            ssd1306.SetVcomhDeselectLevel();
            ssd1306.EntireDisplayOn(true);
            ssd1306.SetDisplayOn();
        }

        private static Ssd1306 GetSsd1306WithI2c()
        {
            Console.WriteLine("Using I2C protocol");

            var connectionSettings = new I2cConnectionSettings(1, 0x3C);
            var i2cDevice = new UnixI2cDevice(connectionSettings);
            var ssd1306 = new Ssd1306(i2cDevice);
            return ssd1306;
        }
    }
}
