// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.Gpio;

using Iot.Device.Hcsr501;

namespace Hcsr501.Samples
{
    class Program
    {
        // HC-SR501 OUT Pin
        static int hcsr501Pin = 17;
        // LED Pin
        static int ledPin = 27;

        static void Main(string[] args)
        {
            // get the GPIO controller
            GpioController ledController = new GpioController(PinNumberingScheme.Logical);
            // open PIN 27 for led
            ledController.OpenPin(ledPin, PinMode.Output);

            // initialize PIR sensor
            using(Iot.Device.Hcsr501.Hcsr501 sensor = new Iot.Device.Hcsr501.Hcsr501(hcsr501Pin, PinNumberingScheme.Logical))
            {
                // loop
                while (true)
                {
                    // adjusting the detection distance and time by rotating the potentiometer on the sensor
                    if (sensor.IsMotionDetected == true)
                    {
                        // turn the led on when the sensor detected infrared heat
                        ledController.Write(ledPin, PinValue.High);
                        Console.WriteLine("Detected! Turn the LED on.");
                    }
                    else
                    {
                        // turn the led off when the sensor undetected infrared heat
                        ledController.Write(ledPin, PinValue.Low);
                        Console.WriteLine("Undetected! Turn the LED off.");
                    }

                    // wait for a second
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
