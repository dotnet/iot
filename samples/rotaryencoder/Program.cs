using System;
using System.Devices.Gpio;
using System.Threading;

namespace rotaryencoder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var controller = new GpioController(new RaspberryPiDriver(),PinNumberingScheme.Gpio);

            var pinA = controller.OpenPin(20,PinMode.InputPullUp);
            var pinB = controller.OpenPin(21, PinMode.InputPullUp);
            var pinC = controller.OpenPin(26, PinMode.InputPullUp);

            var ledPins = new GpioPin[]
            {
                GetOutPin(19),
                GetOutPin(4),
                GetOutPin(17),
                GetOutPin(22),
                GetOutPin(5),
                GetOutPin(13),
                GetOutPin(12),
                GetOutPin(24),
                GetOutPin(25),
                GetOutPin(18),
            };

            foreach (var pin in ledPins)
            {
                pin.Write(PinValue.Low);
            }

            GpioPin GetOutPin(int pin)
            {
                return controller.OpenPin(pin,PinMode.Output);
            }

            var counter = 5;
            var buttonPressed = false;

            while (true)
            {
                var dataA = pinA.Read();
                var dataB = pinB.Read();
                var dataC = pinC.Read();

                if (dataC == PinValue.Low)
                {
                    buttonPressed = true;
                    foreach (var pin in ledPins)
                    {
                        pin.Write(PinValue.High);
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(50);
                }
                else if (dataC == PinValue.High && buttonPressed)
                {
                    foreach (var pin in ledPins)
                    {
                        pin.Write(PinValue.Low);
                    }
                    buttonPressed = false;
                }
                else if (dataA == PinValue.Low && dataB == PinValue.High && counter -1 > -1)
                {
                    Console.WriteLine($"counter: {counter}; left");
                    ledPins[counter].Write(PinValue.Low);
                    counter--;
                    ledPins[counter].Write(PinValue.High);
                }
                else if (dataB == PinValue.Low && dataA == PinValue.High && counter + 1 < 10)
                {
                    Console.WriteLine($"counter: {counter}; right");
                    ledPins[counter].Write(PinValue.Low);
                    counter++;
                    ledPins[counter].Write(PinValue.High);
                }

                Thread.Sleep(10);
            }
        }
    }
}
