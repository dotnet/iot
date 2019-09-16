// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading.Tasks;

namespace Iot.Device.Bh1745.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // bus id on the raspberry pi 3
            const int busId = 1;

            // create device
            var i2cSettings = new I2cConnectionSettings(busId, Bh1745.DefaultI2cAddress);
            var i2cDevice = I2cDevice.Create(i2cSettings);

            using (var i2cBh1745 = new Bh1745(i2cDevice))
            {

                // multipliers affect the compensated values
                i2cBh1745.ChannelCompensationMultipliers.Red = 2.5;
                i2cBh1745.ChannelCompensationMultipliers.Green = 0.9;
                i2cBh1745.ChannelCompensationMultipliers.Blue = 1.9;
                i2cBh1745.ChannelCompensationMultipliers.Clear = 9.5;

                // set custom  measurement time
                i2cBh1745.MeasurementTime = MeasurementTime.Ms1280;

                // interrupt functionality is detailed in the datasheet
                // Reference: https://www.mouser.co.uk/datasheet/2/348/bh1745nuc-e-519994.pdf (page 13)
                i2cBh1745.LowerInterruptThreshold = 0xABFF;
                i2cBh1745.HigherInterruptThreshold = 0x0A10;

                i2cBh1745.LatchBehavior = LatchBehavior.LatchEachMeasurement;
                i2cBh1745.InterruptPersistence = InterruptPersistence.UpdateMeasurementEnd;
                i2cBh1745.InterruptIsEnabled = true;


                // wait for first measurement
                Task.Delay(i2cBh1745.MeasurementTime.ToMilliseconds()).Wait();

                while (true)
                {
                    var color = i2cBh1745.GetCompensatedColor();

                    if (!i2cBh1745.ReadMeasurementIsValid())
                    {
                        Console.WriteLine("Measurement was not valid!");
                        continue;
                    }

                    Console.WriteLine("RGB color read: #{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
                    Console.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

                    Task.Delay(i2cBh1745.MeasurementTime.ToMilliseconds()).Wait();
                }
            }
        }
    }
}
