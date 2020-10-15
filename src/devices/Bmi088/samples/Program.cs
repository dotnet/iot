using System;
using System.Threading;

namespace Iot.Device.Bmi088Device.Samples
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            using (var ag = new Bmi088())
            {
                Console.WriteLine("Press any key to exit");
                Console.WriteLine();
                Console.WriteLine($"                     GYROSCOPE                      ACCELERATION            ");
                Console.WriteLine($"                X         Y         Z     |       X         Y         Z     ");

                while (!Console.KeyAvailable)
                {
                    var rotation = ag.GetGyroscope();
                    var acceleration = ag.GetAccelerometer();

                    Console.WriteLine($"         {rotation.X,8:F2}, {rotation.Y,8:F2}, {rotation.Z,8:F2}     |{acceleration.X,8:F2}, {acceleration.Y,8:F2}, {acceleration.Z,8:F2}            ");

                    Thread.Sleep(500);
                }
            }
        }
    }
}
