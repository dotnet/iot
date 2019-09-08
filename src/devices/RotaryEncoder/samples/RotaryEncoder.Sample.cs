// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.RotaryEncoder.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            using(GpioController controller = new GpioController())
            {
                // create a RotaryEncoder that represents an FM Radio tuning dial with a range of 88 -> 108 MHz
                RotaryEncoder<decimal> encoder = new RotaryEncoder<decimal>(new GpioController(), pinA: 5, pinB: 6, pulseIncrement: 0.1M, rangeMin: 88.0M, rangeMax: 108.0M) { Value = 88 };
                encoder.ValueChanged += (o, e) =>
                {
                    Console.WriteLine($"Tuned to {e.Value}MHz");
                };

                while (true)
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();
                }
            }
        }
    }
}
