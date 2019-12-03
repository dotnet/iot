// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.TimeOfFlight;

namespace Iot.Device.TimeOfFlight.LidarLiteV3Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
            {
                // Take 10 measurements, each one second apart.
                for(int i = 0; i < 10; i++) {
                    ushort currentDistance = llv3.MeasureDistance();
                    Console.WriteLine($"Current Distance: {currentDistance} cm");
                    Thread.Sleep(1000);
                }
            }
        }

        private static I2cDevice CreateI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, LidarLiteV3.DefaultI2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
