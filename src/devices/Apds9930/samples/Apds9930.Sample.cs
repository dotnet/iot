// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Apds9930.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello apds9930!");

            //bus id on the raspberry pi 3
            const int busId = 1;

            var i2cSettings = new I2cConnectionSettings(busId, Apds9930.DefaultI2cAddress);
            var i2cDevice = new UnixI2cDevice(i2cSettings);
            var i2cApds9930 = new  Apds9930(i2cDevice);
            
            using(i2cApds9930)
            {
                i2cApds9930.EnableProximitySensor();
                i2cApds9930.EnableLightSensor();

                while(true)
                {                    
                    Console.WriteLine($"Prox : {i2cApds9930.GetProximity()}");
                    Console.WriteLine($"Ambient Light : {i2cApds9930.GetAmbientLight():N2} lux");                    
                    Console.WriteLine();
                    Thread.Sleep(100);                                
                }
            }
            
        }
    }
}
