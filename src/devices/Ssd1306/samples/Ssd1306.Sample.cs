// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
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
            ssd1306.SendCommand(new SetDisplayOff());
            ssd1306.SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            ssd1306.SendCommand(new SetMultiplexRatio(0x1F));
            ssd1306.SendCommand(new SetDisplayOffset(0x00));
            ssd1306.SendCommand(new SetDisplayStartLine(0x00));
            ssd1306.SendCommand(new SetChargePump(true));
            ssd1306.SendCommand(new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Horizontal));
            ssd1306.SendCommand(new SetSegmentReMap(false));
            ssd1306.SendCommand(new SetComOutputScanDirection(false));
            ssd1306.SendCommand(new SetComPinsHardwareConfiguration(false, false));
            ssd1306.SendCommand(new SetContrastControlForBank0(0x8F));
            ssd1306.SendCommand(new SetPreChargePeriod(0xF1));
            ssd1306.SendCommand(new SetVcomhDeselectLevel(SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            ssd1306.SendCommand(new EntireDisplayOn(false));
            ssd1306.SendCommand(new SetNormalDisplay());
            ssd1306.SendCommand(new SetDisplayOn());
            ssd1306.SendCommand(new SetColumnAddress());
            ssd1306.SendCommand(new SetPageAddress(PageAddress.Page1, PageAddress.Page3));
            
            ClearScreen(ssd1306);

            ssd1306.SendCommand(new SetColumnAddress());
            ssd1306.SendCommand(new SetPageAddress(PageAddress.Page1, PageAddress.Page3));
            ssd1306.SendData(new byte[] { 0x42, 0x7E, 0x7E, 0x42, 0x00, 0x42, 0x7E, 0x7E, 0x52, 0x1E, 0x0C, 0x00, 0x00, 0x48, 0x48, 0x00 });
        }

        private static void ClearScreen(Ssd1306 ssd1306)
        {
            for (int cnt = 0; cnt < 32; cnt++)
            {
                byte[] data = new byte[16];
                ssd1306.SendData(data);
            }
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
