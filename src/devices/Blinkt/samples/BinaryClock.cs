// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// Displays a clock in binary.
    /// </summary>
    public class BinaryClock : IDisposable
    {
        private const int _timeToStayInMode = 3;

        private readonly IBlinkt _blinkt = new Blinkt();

        private int _timeInMode;

        private Mode _mode;

        private int _lastMinute;
        private int _lastHour;

        internal void Run()
        {
            while (!Console.KeyAvailable)
            {
                DateTime dateTimeNow = DateTime.Now;

                if (dateTimeNow.Hour != _lastHour)
                {
                    _mode = Mode.Hour;
                    _timeInMode = 0;
                }
                else if (dateTimeNow.Minute != _lastMinute)
                {
                    _mode = Mode.Minute;
                    _timeInMode = 0;
                }

                _lastMinute = dateTimeNow.Minute;
                _lastHour = dateTimeNow.Hour;

                _blinkt.Clear();

                if (dateTimeNow.Second % 2 == 0)
                {
                    _blinkt.SetPixel(1, 64, 64, 64);
                }

                int val;

                switch (_mode)
                {
                    case Mode.Hour:
                        _blinkt.SetPixel(0, 255, 0, 0);
                        val = dateTimeNow.Hour;
                        break;
                    case Mode.Minute:
                        _blinkt.SetPixel(0, 0, 255, 0);
                        val = dateTimeNow.Minute;
                        break;
                    case Mode.Seconds:
                        _blinkt.SetPixel(0, 0, 0, 255);
                        val = dateTimeNow.Second;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_mode));
                }

                for (var x = 0; x < 6; x++)
                {
                    int bit = (val & (1 << x)) > 0 ? 1 : 0;
                    byte c = (byte)(128 * bit);
                    _blinkt.SetPixel(7 - x, c, c, c);
                }

                _blinkt.Show();

                Console.WriteLine($"{dateTimeNow.ToLongTimeString()}; mode: {_mode} mode; time in mode: {_timeInMode}");

                _timeInMode++;
                if (_timeInMode == _timeToStayInMode)
                {
                    _mode += 1;
                    _mode = (Mode)((int)_mode % 3);
                    _timeInMode = 0;
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _blinkt.Dispose();
        }

        private enum Mode
        {
            Hour,
            Minute,
            Seconds
        }
    }
}
