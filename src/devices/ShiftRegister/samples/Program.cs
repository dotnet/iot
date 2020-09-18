// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            var sr = new ShiftRegister(ShiftRegisterPinMapping.Standard, 8);

            // Uncomment this code to use SPI (and comment the line above)
            // var settings = new SpiConnectionSettings(0, 0);
            // using var spiDevice = SpiDevice.Create(settings);
            // var sr = new Sn74hc595(spiDevice, Sn74hc595.PinMapping.Standard);
            var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationSource.Cancel();
            };

            Console.WriteLine($"Driver for {nameof(ShiftRegister)}");
            Console.WriteLine($"Register bit length: {sr.BitLength}");
            var interfaceType = sr.UsesSpi ? "SPI" : "GPIO";
            Console.WriteLine($"Using {interfaceType}");

            sr.OutputEnable = true;

            DemonstrateShiftingBits(sr, cancellationSource);
            DemonstrateShiftingBytes(sr, cancellationSource);
            BinaryCounter(sr, cancellationSource);
            Console.WriteLine("done");
        }

        private static void DemonstrateShiftingBits(ShiftRegister sr, CancellationTokenSource cancellationSource)
        {
            if (sr.UsesSpi)
            {
                return;
            }

            int delay = 1000;
            sr.ShiftClear();

            Console.WriteLine("Light up three of first four LEDs");
            sr.ShiftBit(1);
            sr.ShiftBit(1);
            sr.ShiftBit(0);
            sr.ShiftBit(1);
            sr.Latch();
            Thread.Sleep(delay);

            sr.ShiftClear();

            Console.WriteLine($"Light up all LEDs, with {nameof(sr.ShiftBit)}");

            for (int i = 0; i < sr.BitLength; i++)
            {
                sr.ShiftBit(1);
            }

            sr.Latch();
            Thread.Sleep(delay);

            sr.ShiftClear();

            Console.WriteLine($"Dim up all LEDs, with {nameof(sr.ShiftBit)}");

            for (int i = 0; i < sr.BitLength; i++)
            {
                sr.ShiftBit(0);
            }

            sr.Latch();
            Thread.Sleep(delay);

            if (IsCanceled(sr, cancellationSource))
            {
                return;
            }
        }

        private static void DemonstrateShiftingBytes(ShiftRegister sr, CancellationTokenSource cancellationSource)
        {
            int delay = 1000;
            Console.WriteLine($"Write a set of values with {nameof(sr.ShiftByte)}");
            // this can be specified as ints or binary notation -- its all the same
            var values = new byte[] { 0b1, 23, 56, 127, 128, 170, 0b_1010_1010 };
            foreach (var value in values)
            {
                Console.WriteLine($"Value: {value}");
                sr.ShiftByte(value);
                Thread.Sleep(delay);
                sr.ShiftClear();

                if (IsCanceled(sr, cancellationSource))
                {
                    return;
                }
            }

            byte litPattern = 0b_1111_1111; // 255
            Console.WriteLine($"Write {litPattern} to each register with {nameof(sr.ShiftByte)}");
            for (int i = 0; i < sr.BitLength / 8; i++)
            {
                sr.ShiftByte(litPattern);
            }

            Thread.Sleep(delay);

            Console.WriteLine("Output disable");
            sr.OutputEnable = false;
            Thread.Sleep(delay * 2);

            Console.WriteLine("Output enable");
            sr.OutputEnable = true;
            Thread.Sleep(delay * 2);

            Console.WriteLine($"Write 23 then 56 with {nameof(sr.ShiftByte)}");
            sr.ShiftByte(23);
            Thread.Sleep(delay);
            sr.ShiftByte(56);
            sr.ShiftClear();
        }

        private static void BinaryCounter(ShiftRegister sr, CancellationTokenSource cancellationSource)
        {
            Console.WriteLine($"Write 0 through 255");
            for (int i = 0; i < 256; i++)
            {
                sr.ShiftByte((byte)i);
                Thread.Sleep(50);
                sr.ShiftClear();

                if (IsCanceled(sr, cancellationSource))
                {
                    return;
                }
            }

            sr.ShiftClear();

            if (sr.BitLength > 8)
            {
                Console.WriteLine($"Write 256 through 4095; pick up the pace");
                for (int i = 256; i < 4096; i++)
                {
                    ShiftBytes(sr, i);
                    Thread.Sleep(25);
                    sr.ShiftClear();

                    if (IsCanceled(sr, cancellationSource))
                    {
                        return;
                    }
                }
            }

            sr.ShiftClear();
        }

        private static void ShiftBytes(ShiftRegister sr, int value)
        {
            if (sr.BitLength > 32)
            {
                throw new ArgumentException($"{nameof(ShiftBytes)}: bit length must be  8-32.");
            }

            for (int i = (sr.BitLength / 8) - 1; i > 0; i--)
            {
                int shift = i * 8;
                int downShiftedValue = value >> shift;
                sr.ShiftByte((byte)downShiftedValue);
            }

            sr.ShiftByte((byte)value);
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
