using System;
using System.Devices.Gpio;
using System.Diagnostics;
using System.Threading;
using Iot.Device.HCSR501;

namespace HCSR501.Samples
{
    class Program
    {
        static HCSR501 sensor;
        static GpioPin led;

        static void Main(string[] args)
        {
            // get the GPIO controller
            GpioController controller = new GpioController(PinNumberingScheme.Gpio);
            // open PIN 27 for led
            led = controller.OpenPin(27, PinMode.Output);

            // initialize PIR sensor
            sensor = new HCSR501(17);
            sensor.Initialize();

            // loop
            while (true)
            {
                if (sensor.Read() == true)
                {
                    // turn the led on when the sensor detected infrared heat
                    led.Write(PinValue.High);
                    Console.WriteLine("Detected! Turn the LED on.");
                }
                else
                {
                    // turn the led off when the sensor undetected infrared heat
                    led.Write(PinValue.Low);
                    Console.WriteLine("Undetected! Turn the LED off.");
                }

                // wait for a second
                Thread.Sleep(1000);
            }
        }
    }
}
