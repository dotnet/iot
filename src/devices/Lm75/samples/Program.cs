using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Lm75.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Lm75.I2cAddress);
            UnixI2cDevice device = new UnixI2cDevice(settings);

            using(Lm75 sensor=new Lm75(device))
            {
                while (true)
                {
                    Console.WriteLine($"Temperature: {sensor.Temperature} ℃");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
