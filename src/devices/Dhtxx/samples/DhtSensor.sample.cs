// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.DHTxx;
using System.Diagnostics;
using System.Device.Gpio;
using System.Threading;
class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Hello DHT!");

        DHTSensor dht = new DHTSensor(26, DhtType.Dht22);

        while (true)
        {
            // You have 2 ways to read the data, both are equivalent
            // First way to read the data
            bool readret = dht.ReadData();
            if (readret)
                Console.WriteLine($"Temperature: {dht.TemperatureInCelsius.ToString("0.00")} °C, Humidity: {dht.Humidity.ToString("0.00")} %");
            else
                Console.WriteLine("Error reading the sensor");
            Thread.Sleep(1000);
            
            // Second way to read the data
            double Temp;
            double Hum;
            if (dht.TryGetTemperatureInCelsiusHumidity(out Temp, out Hum))
                Console.WriteLine($"Temperature: {Temp.ToString("0.00")} °C, Humidity: {Hum.ToString("0.00")} %");
            else
                Console.WriteLine("Error reading the sensor");
            Thread.Sleep(1000);
        }

    }
}
