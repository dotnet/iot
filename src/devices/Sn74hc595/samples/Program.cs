using System;
using System.Device.Gpio;
using System.Device.Spi;
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
        public static void Main(string[] args)
        {
            using var controller = new GpioController();
            var sr = new Sn74hc595(Sn74hc595.PinMapping.Standard, controller, false, 2);
            // var settings = new SpiConnectionSettings(0, 0);
            // using var spiDevice = SpiDevice.Create(settings);
            // var sr = new Sn74hc595(spiDevice, Sn74hc595.PinMapping.Standard);
            var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationSource.Cancel();
            };

            Console.WriteLine("****Information:");
            Console.WriteLine($"Device count: {sr.DeviceCount}");
            Console.WriteLine($"Bit count: {sr.Bits}");
            var interfaceType = sr.UsesSpi ? "SPI" : "GPIO";
            Console.WriteLine($"Using {interfaceType}");

            if (!sr.UsesSpi)
            {
                DemonstrateShiftingBits(sr, cancellationSource);
            }

            DemonstrateShiftingBytes(sr, cancellationSource);
            BinaryCounter(sr, cancellationSource);
        }

        private static void DemonstrateShiftingBits(Sn74hc595 sr, CancellationTokenSource cancellationSource)
        {
            sr.ShiftClear();

            Console.WriteLine("Light up three of first four LEDs");
            sr.ShiftBit(1);
            sr.ShiftBit(1);
            sr.ShiftBit(0);
            sr.ShiftBit(1);
            sr.Latch();
            Console.ReadLine();

            sr.ShiftClear();

            Console.WriteLine($"Light up all LEDs, with {nameof(sr.ShiftBit)}");

            for (int i = 0; i < sr.Bits; i++)
            {
                sr.ShiftBit(1);
            }

            sr.Latch();
            Console.ReadLine();

            sr.ShiftClear();

            Console.WriteLine($"Dim up all LEDs, with {nameof(sr.ShiftBit)}");

            for (int i = 0; i < sr.Bits; i++)
            {
                sr.ShiftBit(0);
            }

            sr.Latch();
            Console.ReadLine();

            if (IsCanceled(sr, cancellationSource))
            {
                return;
            }
        }

        private static void DemonstrateShiftingBytes(Sn74hc595 sr, CancellationTokenSource cancellationSource)
        {
            Console.WriteLine($"Write a set of values with {nameof(sr.ShiftByte)}");
            // this can be specified as ints or binary notation -- its all the same
            var values = new byte[] { 0b1, 23, 56, 127, 128, 170, 0b10101010 };
            foreach (var value in values)
            {
                Console.WriteLine($"Value: {value}");
                sr.ShiftByte(value);
                Console.ReadLine();
                sr.ShiftClear();

                if (IsCanceled(sr, cancellationSource))
                {
                    return;
                }
            }

            byte lit = 0b11111111;
            Console.WriteLine($"Write {lit} to each register with {nameof(sr.ShiftByte)}");
            for (int i = 0; i < sr.DeviceCount; i++)
            {
                sr.ShiftByte(lit);
            }

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
            Console.ReadLine();
            sr.ShiftClear();
        }

        private static void BinaryCounter(Sn74hc595 sr, CancellationTokenSource cancellationSource)
        {
            Console.WriteLine($"Write 0 through 255");
            for (int i = 0; i < 256; i++)
            {
                sr.ShiftByte((byte)i);
                Thread.Sleep(100);
                sr.ClearStorage();

                if (IsCanceled(sr, cancellationSource))
                {
                    return;
                }
            }

            sr.ShiftClear();

            if (sr.DeviceCount > 1)
            {
                Console.WriteLine($"Write 256 through 4095; pick up the pace");
                for (int i = 256; i < 4096; i++)
                {
                    ShiftBytes(sr, i);
                    Thread.Sleep(10);
                    sr.ClearStorage();

                    if (IsCanceled(sr, cancellationSource))
                    {
                        return;
                    }
                }
            }

            Console.WriteLine("done");
            Console.ReadLine();

            sr.ShiftClear();
        }

        private static void ShiftBytes(Sn74hc595 sr, int value, int byteCount = 0)
        {
            if (byteCount > 4)
            {
                throw new ArgumentException($"{nameof(ShiftBytes)}: count must be  1-4.");
            }

            if (byteCount == 0)
            {
                byteCount = sr.DeviceCount;
            }

            for (int i = byteCount - 1; i > 0; i--)
            {
                int shift = i * 8;
                int downShiftedValue = value >> shift;
                sr.ShiftByte((byte)downShiftedValue);
            }

            sr.ShiftByte((byte)value);
        }

        private static bool IsCanceled(Sn74hc595 sr, CancellationTokenSource cancellationSource)
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

        /*
            Using the shift register w/o a binding

            while (!cancellationSource.IsCancellationRequested)
            {
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

            for (int i = 0; i < 8; i++)
            {
                controller.Write(SER,PinValue.Low);
                controller.Write(SRCLK,PinValue.High);
                controller.Write(SRCLK,PinValue.Low);
            }

            controller.Write(RCLK,PinValue.High);
            */
