using System;
using System.Threading;

namespace Iot.Device.Llc200d3sh.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (Llc200d3sh sensor = new Llc200d3sh(23))
            {
                while (true)
                {
                    // read liquid level switch
                    Console.WriteLine($"Detected: {sensor.ReadValue()}");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
