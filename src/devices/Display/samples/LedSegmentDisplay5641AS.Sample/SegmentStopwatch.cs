// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Display;
using Iot.Device.Button;
using Iot.Device.Display;

namespace LedSEgmentDisplay5641AS.Sample
{
    internal class SegmentStopwatch
    {
        private readonly object _lockObj = new object();
        private readonly Stopwatch _sw;

        private readonly List<Font> _toWrite;
        private bool _timerRunning = false;

        public SegmentStopwatch()
        {
            _toWrite = new List<Font>(4);
            _sw = new Stopwatch();
        }

        public void Run()
        {
            bool[] decimalsEnabled = new bool[] { false, true, false, false };

            var scheme = new LedSegmentDisplay5641ASPinScheme(16, 21, 6, 19, 26, 20, 5, 13,
                22, 27, 17, 4);

            using var gpio = new System.Device.Gpio.GpioController();
            using var display = new LedSegmentDisplay5641AS(scheme, gpio, false);
            GpioButton timerButton = new GpioButton(12, gpio: gpio);
            timerButton.Press += TimerButton_Press;

            display.Clear();
            Thread.Sleep(2000);

            do
            {
                lock (_lockObj)
                {
                    _toWrite.Clear();
                }

                // Use the Segment[] overload to set DP per digit
                Segment[] elapsedString = FontHelper.GetString(_sw.Elapsed.ToString("ssff")).Cast<Segment>().ToArray();
                elapsedString[1] |= Segment.Dot;

                /* Use Font[] overload for general string writes without DP
                Font[] elapsedString = FontHelper.GetString(_sw.Elapsed.ToString("ssff"));
                */

                display.Write(elapsedString);
                Thread.Sleep(1);
            }
            while (true);
        }

        private void TimerButton_Press(object? sender, EventArgs e)
        {
            lock (_lockObj)
            {
                if (_timerRunning)
                {
                    _sw.Stop();
                }
                else
                {
                    _sw.Start();
                }

                _timerRunning = !_timerRunning;
            }
        }
    }
}
