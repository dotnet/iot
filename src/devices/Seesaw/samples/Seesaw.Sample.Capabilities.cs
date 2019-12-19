// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Seesaw.Samples
{
    internal class Program
    {
        public static void Main()
        {
            const byte AdafruitSeesawBreakoutI2cAddress = 0x49;
            const byte AdafruitSeesawBreakoutI2cBus = 0x1;

            using (I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawBreakoutI2cBus, AdafruitSeesawBreakoutI2cAddress)))
            using (Seesaw ssDevice = new Seesaw(i2cDevice))
            {
                Console.WriteLine();
                Console.WriteLine($"Seesaw Version: {ssDevice.Version}");
                Console.WriteLine();

                foreach (Seesaw.SeesawModule module in Enum.GetValues(typeof(Seesaw.SeesawModule)))
                {
                    Console.WriteLine($"Module: {Enum.GetName(typeof(Seesaw.SeesawModule), module)} - {(ssDevice.HasModule(module) ? "available" : "not-available")}");
                }

                Console.WriteLine();
            }
        }
    }
}
