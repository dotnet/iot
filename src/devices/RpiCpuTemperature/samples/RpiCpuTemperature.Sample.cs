using System;
using System.Threading;
using Iot.Device.Rpi;

namespace Iot.Device.RpiCpuTemperature.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            CpuTemperature cpuTemperature = new CpuTemperature();

            while (true)
            {
                double? _temperature = cpuTemperature.ReadTemperature();

                if (_temperature == null)
                {
                    Console.WriteLine("Problem reading CPU Temperature");
                }
                else
                {
                    Console.WriteLine($"CPU Temperature {_temperature} degrees centigrade");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
