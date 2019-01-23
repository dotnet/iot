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

        while(true)
        {
            bool readret = dht.ReadData();
            if (readret)
                Console.WriteLine($"Temperature: {dht.Temperature.ToString("0.00")} °C, Humidity: {dht.Humidity.ToString("0.00")} %");
            else
                Console.WriteLine("Error reading the sensor");
            Thread.Sleep(1000);
        }
        
    }
}