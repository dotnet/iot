// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using System.Device.I2c.Devices;
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
            using (Lcd1602 lcd = new Lcd1602(registerSelectPin: 22, enablePin: 17, dataPins: new int[] { 25, 24, 23, 18 }))
            {
                lcd.Clear();
                lcd.Write("Hello World");
            }
        }

        /// <summary>
        /// This method will use an mcp gpio extender to connect to the LCM display.
        /// This has been tested on the CrowPi lcd display.
        /// </summary>
        static void UsingMcp()
        {
            I2cDevice i2CDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x21));
            Mcp23008 mcpDevice = new Mcp23008(i2CDevice);
            int[] dataPins = { 3, 4, 5, 6 };
            int registerSelectPin = 1;
            int enablePin = 2;
            int backlight = 7;
            using (mcpDevice)
            using (Lcd1602 lcd = new Lcd1602(registerSelectPin, enablePin, dataPins, backlight, controller: mcpDevice))
            {
                lcd.Clear();

                lcd.Write("Hello World");
                lcd.SetCursorPosition(0, 1);
                lcd.Write(".NET Core");
            }
        }
    }
}
