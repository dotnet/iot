// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using System.Device.I2c.Drivers;
using Iot.Device.Mcp23xxx;

namespace Iot.Device.Lcm1602a1.Samples
{
    class Program
    {
        /// <summary>
        /// This program will print `Hello World`
        /// </summary>
        /// <param name="args">Should be empty</param>
        static void Main(string[] args)
        {

            int[] dataPins = { 3, 4, 5, 6 };
            int registerSelectPin = 1;
            int enablePin = 2;
            using (Lcm1602a1 lcd = new Lcm1602a1(registerSelectPin, enablePin, dataPins))
            {
                lcd.Clear();
                lcd.Print("Hello World");
            }
        }

        /// <summary>
        /// This method will use an mcp gpio extender to connect to the LCM display.
        /// This has been tested on the CrowPi lcd display.
        /// </summary>
        static void UsingMcp()
        {
            UnixI2cDevice i2CDevice = new UnixI2cDevice(new I2cConnectionSettings(1, 0x21));
            Mcp23008 mcpDevice = new Mcp23008(i2CDevice);
            int[] dataPins = { 3, 4, 5, 6 };
            int registerSelectPin = 1;
            int enablePin = 2;
            int backlight = 7;
            using (mcpDevice)
            using (Lcm1602a1 lcd = new Lcm1602a1(mcpDevice, registerSelectPin, -1, enablePin, backlight, dataPins))
            {
                lcd.Clear();
                lcd.Begin(16, 2);

                lcd.Print("Hello World");
                lcd.SetCursor(0, 1);
                lcd.Print(".NET Core");
            }
        }
    }
}
