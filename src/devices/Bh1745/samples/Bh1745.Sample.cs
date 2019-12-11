// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading.Tasks;

namespace Iot.Device.Bh1745.Samples
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            // bus id on the raspberry pi 3
            const int busId = 1;

            // create device
            var i2cSettings = new I2cConnectionSettings(busId, Bh1745.DefaultI2cAddress);
            var i2cDevice = I2cDevice.Create(i2cSettings);

            using (var i2cBh1745 = new Bh1745(i2cDevice))
            {
                // wait for first measurement
                Task.Delay(i2cBh1745.MeasurementTime.ToMilliseconds()).Wait();

                while (true)
                {
                    var color = i2cBh1745.GetCompensatedColor();
                    Console.WriteLine("RGB color read: #{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
                    Console.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

                    Task.Delay(i2cBh1745.MeasurementTime.ToMilliseconds()).Wait();
                }
            }
        }
    }
}
