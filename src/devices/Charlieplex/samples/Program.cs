using System;
using System.Device.Gpio;
using System.Threading;
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
            // var charliePins = CharliePlex.GetCharliePins(pins, charliePinCount);
            var charlie = new Charlieplex(pins, charliePinCount);

            for (int i = 0; i < charliePinCount; i++)
            {
                charlie.Write(i, 1);
                Thread.Sleep(500);
                charlie.Write(i, 0);
            }
        }
    }
}
