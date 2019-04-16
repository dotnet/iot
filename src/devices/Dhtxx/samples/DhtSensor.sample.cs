// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Iot.Device.DHTxx;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Hello DHT!");

        // Init DHT12 through I2C
        //I2cConnectionSettings settings = new I2cConnectionSettings(1, DhtSensor.Dht12DefaultI2cAddress);
        //UnixI2cDevice device = new UnixI2cDevice(settings);
        //DhtSensor dht = new DhtSensor(device);

        using (DhtSensor dht = new DhtSensor(4, DhtType.Dht11))
        {
            while (true)
            {
                Console.WriteLine($"Temperature: {dht.Temperature.Celsius.ToString("0.0")} °C, Humidity: {dht.Humidity.ToString("0.0")} %");

                Thread.Sleep(2000);
            }
        }
    }
}
