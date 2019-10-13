// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.Gpio;
using System.Device.I2c;
using System.Globalization;
using Iot.Device.Mcp23xxx;

namespace Iot.Device.CharacterLcd.Samples
{
    class Program
    {
        /// <summary>
        /// This program will print `Hello World`
        /// </summary>
        /// <param name="args">Should be empty</param>
        static void Main(string[] args)
        {
            // Sets up a 16x2 character LCD with a hardwired or no backlight.
            //using (Lcd1602 lcd = new Lcd1602(registerSelectPin: 22, enablePin: 17, dataPins: new int[] { 25, 24, 23, 18 }))
            //{
            //    lcd.Clear();
            //    lcd.Write("Hello World");
            //}
            UsingHd44780OverI2C();
        }

        /// <summary>
        /// This method will use an mcp gpio extender to connect to the LCM display.
        /// This has been tested on the CrowPi lcd display.
        /// </summary>
        static void UsingMcp()
        {
            I2cDevice i2CDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x21));
            Mcp23008 driver = new Mcp23008(i2CDevice);
            int[] dataPins = { 3, 4, 5, 6 };
            int registerSelectPin = 1;
            int enablePin = 2;
            int backlight = 7;
            using (driver)
            using (Lcd1602 lcd = new Lcd1602(registerSelectPin, enablePin, dataPins, backlight, controller: new GpioController(PinNumberingScheme.Logical, driver)))
            {
                lcd.Clear();

                lcd.Write("Hello World");
                lcd.SetCursorPosition(0, 1);
                lcd.Write(".NET Core");
            }
        }

        static void UsingHd44780OverI2C()
        {
            using (I2cDevice i2CDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x27)))
            {
                LcdInterface lcdInterface = LcdInterface.CreateI2c(i2CDevice, false);
                using (Hd44780 hd44780 = new Lcd2004(lcdInterface))
                {
                    hd44780.UnderlineCursorVisible = false;
                    hd44780.BacklightOn = true;
                    hd44780.DisplayOn = true;
                    hd44780.Clear();
                    ExtendedSample.Test(hd44780);
                    
                }
            }

        }
    }
}
