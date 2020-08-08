// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// Displays a clock in binary with colors.
    /// </summary>
    public class BinaryClockMeld : IDisposable
    {
        private const byte OnValue = 64;

        private readonly IBlinkt _blinkt = new Blinkt();

        internal void Run()
        {
            Console.WriteLine("Hour = Red, Minute = Green, Second = Blue");

            while (!Console.KeyAvailable)
            {
                DateTime dateTimeNow = DateTime.Now;

                Console.WriteLine($"{dateTimeNow.ToLongTimeString()}");

                _blinkt.Clear();

                var v = (byte)(OnValue * (dateTimeNow.Second % 2));
                _blinkt.SetPixel(0, v, v, v);

                for (var n = 0; n < 6; n++)
                {
                    var bitHour = (byte)((dateTimeNow.Hour & (1 << n)) > 0 ? 1 : 0);
                    var bitMinute = (byte)((dateTimeNow.Minute & (1 << n)) > 0 ? 1 : 0);
                    var bitSecond = (byte)((dateTimeNow.Second & (1 << n)) > 0 ? 1 : 0);

                    _blinkt.SetPixel(7 - n, (byte)(bitHour * OnValue), (byte)(bitMinute * OnValue), (byte)(bitSecond * OnValue));
                }

                _blinkt.Show();

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _blinkt.Dispose();
        }
    }
}
