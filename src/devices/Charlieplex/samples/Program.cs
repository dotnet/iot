using System;
using Iot.Device.Multiplex;

namespace CharlieTest
{
    /// <summary>
    /// Test application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entrypoint
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World6!");

            var pins = new int[] { 6, 13, 19, 26 };
            var charlieSegmentLength = 8;
            // calling this method helps with determing the correct pin circuit to use
            var charlieNodes = Charlieplex.GetCharlieNodes(pins, charlieSegmentLength);
            var charlie = new Charlieplex(pins, charlieSegmentLength);

            for (int j = 0; j < 2; j++)
            {
                var delay = 10 / (j + 1);
                Console.WriteLine($"Light all LEDs -- {delay}ms");
                for (int i = 0; i < charlieSegmentLength; i++)
                {
                    Console.WriteLine($"light pin {i}");
                    charlie.Write(i, 1, delay);
                }

                Console.WriteLine($"Dim all LEDs -- {delay}ms");
                for (int i = 0; i < charlieSegmentLength; i++)
                {
                    Console.WriteLine($"dim pin {i}");
                    charlie.Write(i, 0, delay);
                }
            }
        }
    }
}
