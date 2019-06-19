// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
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
            var i2cDevice = new UnixI2cDevice(i2cSettings);
            var i2CBh1745 = new Bh1745(i2cDevice);
			
			// Init device and wait for first measurement
			i2CBh1745.Init();
			Task.Delay(i2CBh1745.MeasurementTime.ToMilliseconds()).Wait();		
			

            while (true)
            {                
                var color = i2CBh1745.GetCompensatedColor();
                Console.WriteLine("RGB color read: #{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
                Console.WriteLine($"Raw illumination value: {i2CBh1745.ClearDataRegister}");

                Task.Delay(i2CBh1745.MeasurementTime.ToMilliseconds()).Wait();
            }
        }
    }
}
