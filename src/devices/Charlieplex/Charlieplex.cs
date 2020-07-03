using System;
using System.Device.Gpio;

namespace Iot.Device.Multiplex
{
    /// <summary>
    /// Provides support for Charlieplex multiplexing.
    /// https://wikipedia.org/wiki/Charlieplexing
    /// </summary>
    public class Charlieplex : IDisposable
    {
        private readonly bool _shouldDispose;
        private GpioController _controller;
        private int[] _pins;
        private int _loadCount;
        private CharlieLoad[] _charliePins;
        private CharlieLoad _litPin;

        /// <summary>
        /// Initializes a new Charlieplex type that can be use for multiplex over a relatively small number of GPIO pins.
        /// </summary>
        /// <param name="pins">The set of pins to use.</param>
        /// <param name="loadCount">The count of loads (like LEDs) that will be addressable. If 0, then the Charlieplex maximum is used for the pins provided (n^2-n).</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public Charlieplex(int[] pins, int loadCount = 0,  GpioController gpioController = null, bool shouldDispose = true)
        {
            if (gpioController == null)
            {
                gpioController = new GpioController();
            }

            _controller = gpioController;
            _shouldDispose = shouldDispose;
            _pins = pins;
            var pinCount = _pins.Length;
            var charlieCount = (int)Math.Pow(pinCount, 2) - pinCount;

            if (loadCount == 0)
            {
                loadCount = charlieCount;
            }

            if (loadCount > charlieCount)
            {
                throw new ArgumentException($"{nameof(Charlieplex)}: maximum count is {charlieCount} based on {pinCount} pins. {loadCount} was specified as the count.");
            }

            if (pins.Length < 2)
            {
                throw new ArgumentException($"{nameof(Charlieplex)}: 2 or more pins must be provided.");
            }

            _loadCount = loadCount;
            _charliePins = GetCharlieLoads(pins, loadCount);

            foreach (var pin in pins)
            {
                _controller.OpenPin(pin, PinMode.Input);
            }
        }

        /// <summary>
        /// The number of load devices (like LEDs) that can be addressed.
        /// </summary>
        public int LoadCount => _loadCount;

        /// <summary>
        /// Write a PinValue to a load.
        /// Address scheme is 0-based. Given 8 loads, addresses would be 0-7.
        /// </summary>
        public void Write(int load, PinValue value)
        {
            var charliePin = _charliePins[load];

            // drop power
            if (_litPin.Anode > 0)
            {
                _controller.Write(_litPin.Anode, 0);
            }

            _litPin = charliePin;

            // configure the circuit back to a neutral configuration
            foreach (var p in _pins)
            {
                if (p == charliePin.Anode || p == charliePin.Cathode)
                {
                    continue;
                }

                if (_controller.GetPinMode(p) == PinMode.Output)
                {
                    _controller.SetPinMode(p, PinMode.Input);
                }
            }

            // set ground on Cathode leg
            if (_controller.GetPinMode(charliePin.Cathode) == PinMode.Input)
            {
                _controller.SetPinMode(charliePin.Cathode, PinMode.Output);
            }

            _controller.Write(charliePin.Cathode, 0);

            // set power on Anode leg
            if (_controller.GetPinMode(charliePin.Anode) == PinMode.Input)
            {
                _controller.SetPinMode(charliePin.Anode, PinMode.Output);
            }

            _controller.Write(charliePin.Anode, 1);
        }

        /// <summary>
        /// Provides the set of Charlie loads given the set of pins and the count provided.
        /// If count = 0, then the Charlieplex maximum is used for the pins provided (n^2-n).
        /// </summary>
        public static CharlieLoad[] GetCharlieLoads(int[] pins, int loadCount = 0)
        {
            var pinCount = pins.Length;

            if (loadCount == 0)
            {
                var charlieCount = (int)Math.Pow(pinCount, 2) - pinCount;
                loadCount = charlieCount;
            }

            var charliePins = new CharlieLoad[loadCount];

            var pin = 0;
            var pinJump = 1;
            var firstLeg = false;
            var resetCount = pins.Length - 1;
            for (int i = 0; i < loadCount; i++)
            {
                if (pin > 0 && pin % resetCount == 0)
                {
                    pin = 0;
                    pinJump++;
                }

                var charliePin = new CharlieLoad();

                if (!firstLeg)
                {
                    charliePin.Anode = pins[pin];
                    charliePin.Cathode = pins[pin + pinJump];
                    firstLeg = true;
                }
                else
                {
                    charliePin.Anode = pins[pin + pinJump];
                    charliePin.Cathode = pins[pin];
                    firstLeg = false;
                    pin++;
                }

                charliePins[i] = charliePin;
            }

            return charliePins;
        }

        /// <summary>
        /// Cleanup.
        /// Failing to dispose this class, especially when callbacks are active, may lead to undefined behavior.
        /// </summary>
        public void Dispose()
        {
            // this condition only applies to GPIO devices
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }
    }
}
