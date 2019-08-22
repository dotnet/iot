using System;
using System.Device.I2c;
using Iot.Device.Kt0803;

namespace Kt0803Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Kt0803.I2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Kt0803 radio = new Kt0803(device, 106.6, Country.China))
            {
                Console.WriteLine($"The radio is running on FM {radio.Channel.ToString("0.0")}MHz");

                Console.ReadKey();
            }
        }
    }
}
