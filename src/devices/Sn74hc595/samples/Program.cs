using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.ShiftRegister;

namespace ShiftRegister
{
    /// <summary>
    /// Test application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entrypoint
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("Hello World!");

            var controller = new GpioController();
            var sr = new Sn74hc595(Sn74hc595.PinMapping.Standard, controller, true);

            var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationSource.Cancel();
            };

            Console.WriteLine("****Information:");
            Console.WriteLine($"Bit count: {sr.Bits}");
            sr.ShiftClear();

            Console.WriteLine("Light up three of first four LEDs");
            sr.Shift(1);
            sr.Shift(1);
            sr.Shift(0);
            sr.Shift(1);
            sr.Latch();
            Console.ReadLine();

            sr.ShiftClear();
            Console.WriteLine($"Light up all LEDs, with {nameof(sr.Shift)}");

            for (int i = 0; i < sr.Bits; i++)
            {
                sr.Shift(1);
            }

            sr.Latch();
            Console.ReadLine();

            sr.ShiftClear();

            Console.WriteLine($"Dim up all LEDs, with {nameof(sr.Shift)}");

            for (int i = 0; i < sr.Bits; i++)
            {
                sr.Shift(0);
            }

            sr.Latch();
            Console.ReadLine();

            if (cancellationSource.IsCancellationRequested)
            {
                return;
            }

            Console.WriteLine($"Write a set of values with {nameof(sr.ShiftByte)}");
            var values = new byte[] { 23, 56, 127, 128, 250 };
            foreach (var value in values)
            {
                Console.WriteLine($"Value: {value}");
                sr.ShiftByte(value);
                sr.Latch();
                Console.ReadLine();
                sr.ShiftClear();

                if (cancellationSource.IsCancellationRequested)
                {
                    sr.ShiftClear();
                    return;
                }
            }

            Console.WriteLine($"Write 255 to each register with {nameof(sr.ShiftByte)}");
            for (int i = 0; i < sr.Count; i++)
            {
                sr.ShiftByte(255);
            }

            sr.Latch();
            Console.ReadLine();

            Console.WriteLine("Output disable");
            sr.OutputDisable();
            Console.ReadLine();

            Console.WriteLine("Output enable");
            sr.OutputEnable();
            Console.ReadLine();

            Console.WriteLine($"Write 23 then 56 with {nameof(sr.ShiftByte)}");
            sr.ShiftByte(23);
            sr.ShiftByte(56);
            sr.Latch();
            Console.ReadLine();
            sr.ShiftClear();

            Console.WriteLine($"Clear storage with {nameof(sr.ClearStorage)} and then latch");
            sr.ClearStorage();
            sr.Latch();
            Console.ReadLine();

            Console.WriteLine($"Write 0 through 255-1");
            for (var i = 0; i < 255; i++)
            {
                sr.ShiftByte((byte)i);
                sr.Latch();
                Thread.Sleep(100);
                sr.ClearStorage();

                if (cancellationSource.IsCancellationRequested)
                {
                    sr.ShiftClear();
                    return;
                }
            }

            sr.ShiftClear();

            if (sr.Count > 1)
            {
                Console.WriteLine($"Write 256 through 4096-1; pick up the pace");
                for (var i = 256; i < 4096; i++)
                {
                    var downShiftedValue = i >> 8;
                    sr.ShiftByte((byte)downShiftedValue);
                    sr.ShiftByte((byte)i);
                    sr.Latch();
                    Thread.Sleep(10);
                    sr.ClearStorage();

                    if (cancellationSource.IsCancellationRequested)
                    {
                        sr.ShiftClear();
                        return;
                    }
                }
            }

            Console.WriteLine("done");
            Console.ReadLine();

            sr.ShiftClear();

            /*
            Using the shift register w/o a binding

            while (!cancellationSource.IsCancellationRequested)
            {
                // set bits high, one at a time
                for (int i = 0; i < 8; i++)
                {
                    controller.Write(SER,PinValue.High);
                    controller.Write(SRCLK,PinValue.High);
                    controller.Write(SER,PinValue.Low);
                    controller.Write(SRCLK,PinValue.Low);

                    controller.Write(RCLK,PinValue.High);
                    controller.Write(RCLK,PinValue.Low);

                    Thread.Sleep(100);
                }
                Thread.Sleep(500);
            }

            // set bits low, all at once
            for (int i = 0; i < 8; i++)
            {
                controller.Write(SER,PinValue.Low);
                controller.Write(SRCLK,PinValue.High);
                controller.Write(SRCLK,PinValue.Low);
            }

            controller.Write(RCLK,PinValue.High);
            */
        }
    }
}
