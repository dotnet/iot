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
            Console.WriteLine("Hello World!");

            var pins = new int[] { 6, 13, 19, 26 };
            var charliePinCount = 8;
            // calling this method helps with determing the correct pin circuit to use
            var charliePins = Charlieplex.GetCharlieLoads(pins, 12);
            var charlie = new Charlieplex(pins, charliePinCount);

            Console.WriteLine("Light 1st LED -- 1s");
            charlie.Write(0, 1, 1000);

            Console.ReadLine();
            charlie.Write(0, 0, 0);

            Console.WriteLine("Light all LEDs -- 50ms");
            for (int i = 0; i < charliePinCount; i++)
            {
                charlie.Write(i, 1, 50);
            }

            Console.WriteLine("Dim all LEDs -- 50ms");
            for (int i = 0; i < charliePinCount; i++)
            {
                charlie.Write(i, 0, 50);
            }
        }
    }
}
