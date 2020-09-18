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
            var sr = new Mbi5027(Mbi5027PinMapping.Complete);
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
            int endValue = 4095;
            Console.WriteLine($"Write 0 through {endValue}");
            var delay = 10;
            for (int i = 0; i < endValue; i++)
            {
                sr.ShiftByte((byte)i);
                Thread.Sleep(delay);
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

            foreach (var value in sr.ReadOutputErrorStatus())
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
