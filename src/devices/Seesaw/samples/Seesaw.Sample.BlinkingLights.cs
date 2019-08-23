// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Seesaw.Samples
{
    class Program
    {
        static void Main()
        {
            // pins
            const int ledOne = 9;
            const int ledTwo = 10;
            const int ledThree = 11;
            const int buttonOne = 14;

            const int buttonSleep = 50;
            const int volumeSleep = 50;

            // volume support
            const int initialSleep = 100;
            int sleep = initialSleep;
            Volume volume = null;

            TimeEnvelope[] envelopes = new TimeEnvelope[] { new TimeEnvelope(1000), new TimeEnvelope(1000), new TimeEnvelope(4000) };

            Console.WriteLine("Hello World!");

            Console.WriteLine($"Let's blink some LEDs!");

            using (Seesaw seesawDevice = new Seesaw(I2cDevice.Create(new I2cConnectionSettings(1, 0x49))))
            using (SeesawGpioDriver seesawGpioDevice = new SeesawGpioDriver(seesawDevice))
            using (GpioController controller = new GpioController(PinNumberingScheme.Logical, seesawGpioDevice))
            {
                // this line should only be enabled if a trimpot is connected
                volume = Volume.EnableVolume(seesawDevice);

                controller.OpenPin(ledOne, PinMode.Output);
                controller.OpenPin(ledTwo, PinMode.Output);
                controller.OpenPin(ledThree, PinMode.Output);
                controller.OpenPin(buttonOne, PinMode.InputPullDown);

                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                {
                    controller.Dispose();
                    Console.WriteLine("Pin cleanup complete!");
                };

                while (true)
                {
                    // behavior for ledOne
                    if (envelopes[0].Time == 0)
                    {
                        Console.WriteLine($"Light LED one for 800ms");
                        controller.Write(ledOne, PinValue.High);
                    }
                    else if (envelopes[0].IsLastMultiple(200))
                    {
                        Console.WriteLine($"Dim LED one for 200ms");
                        controller.Write(ledOne, PinValue.Low);
                    }

                    // behavior for ledTwo
                    if (envelopes[1].IsMultiple(200))
                    {
                        Console.WriteLine($"Light LED two for 100ms");
                        controller.Write(ledTwo, PinValue.High);
                    }
                    else if (envelopes[1].IsMultiple(100))
                    {
                        Console.WriteLine($"Dim LED two for 100ms");
                        controller.Write(ledTwo, PinValue.Low);
                    }

                    // behavior for ledThree
                    if (envelopes[2].Time == 0)
                    {
                        Console.WriteLine("Light LED three for 2000 ms");
                        controller.Write(ledThree, PinValue.High);
                    }
                    else if (envelopes[2].IsFirstMultiple(2000))
                    {
                        Console.WriteLine("Dim LED three for 2000 ms");
                        controller.Write(ledThree, PinValue.Low);
                    }

                    // behavior for buttonOne
                    if (volume != null)
                    {
                        var update = true;
                        var value = 0;
                        while (update)
                        {
                            (update, value) = volume.GetSleepForVolume(initialSleep);
                            if (update)
                            {
                                sleep = value;
                                Thread.Sleep(volumeSleep);
                            }
                        }
                    }

                    while (controller.Read(buttonOne) == PinValue.High)
                    {
                        Console.WriteLine("Button one pin value high!");
                        controller.Write(ledOne, PinValue.High);
                        controller.Write(ledTwo, PinValue.High);
                        controller.Write(ledThree, PinValue.High);
                        Thread.Sleep(buttonSleep);
                    }

                    Console.WriteLine($"Sleep: {sleep}");
                    Thread.Sleep(sleep); // starts at 100ms
                    TimeEnvelope.AddTime(envelopes, 100); // always stays at 100
                }
            }
        }
    }
}
