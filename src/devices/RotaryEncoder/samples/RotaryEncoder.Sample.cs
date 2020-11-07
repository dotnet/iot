// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.RotaryEncoder.Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Tune your radio station with RotaryEncoder and press a key to exit");
            using (GpioController controller = new GpioController())
            {
                // create a RotaryEncoder that represents an FM Radio tuning dial with a range of 88 -> 108 MHz
                ScaledQuadratureEncoder encoder = new ScaledQuadratureEncoder(new GpioController(), pinA: 5, pinB: 6, PinEventTypes.Falling, pulsesPerRotation: 20, pulseIncrement: 0.1, rangeMin: 88.0, rangeMax: 108.0) { Value = 88 };
                // 2 milliseconds debonce time
                encoder.DebounceMilliseconds = TimeSpan.FromMilliseconds(2);
                // Register to Value change events
                encoder.ValueChanged += (o, e) =>
                {
                    Console.WriteLine($"Tuned to {e.Value}MHz");
                };

                while (!Console.KeyAvailable)
                {
                    System.Threading.Tasks.Task.Delay(100).Wait();
                }
            }
        }
    }
}
