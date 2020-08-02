using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Multiplexing;

namespace ShiftRegisterDriver
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
            var mapping = Mbi5027PinMapping.Standard;
            mapping.Sdo = 21;
            var sr = new Mbi5027(mapping);
            var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationSource.Cancel();
            };

            Console.WriteLine($"Driver for {nameof(Mbi5027)}");
            Console.WriteLine($"Register bit length: {sr.BitLength}");

            CheckCircuit(sr);
            BinaryCounter(sr, cancellationSource);
        }

        private static void BinaryCounter(ShiftRegister sr, CancellationTokenSource cancellationSource)
        {
            Console.WriteLine($"Write 0 through 4095");
            for (int i = 0; i < 4095; i++)
            {
                sr.ShiftByte((byte)i);
                Thread.Sleep(50);
                sr.ShiftClear();

                if (IsCanceled(sr, cancellationSource))
                {
                    return;
                }
            }
        }

        private static void CheckCircuit(Mbi5027 sr)
        {
            Console.WriteLine("Checking circuit");
            sr.EnableDetectionMode();

            var index = sr.BitLength - 1;

            foreach (var value in sr.ReadErrorStatus())
            {
                Console.WriteLine($"Bit {index--}: {value}");
            }

            sr.EnableNormalMode();
        }

        private static bool IsCanceled(ShiftRegister sr, CancellationTokenSource cancellationSource)
        {
            if (cancellationSource.IsCancellationRequested)
            {
                sr.ShiftClear();
                return true;
            }

            return false;
        }
    }
}
