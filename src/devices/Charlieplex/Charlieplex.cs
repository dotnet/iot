using System;
using System.Device.Gpio;
using System.Threading;

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
        private int _nodeCount;
        private CharlieplexNode[] _charlieNodes;

        /// <summary>
        /// Initializes a new Charlieplex type that can be use for multiplex over a relatively small number of GPIO pins.
        /// </summary>
        /// <param name="pins">The set of pins to use.</param>
        /// <param name="nodeCount">The count of nodes (like LEDs) that will be addressable. If 0, then the Charlieplex maximum is used for the pins provided (n^2-n).</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public Charlieplex(int[] pins, int nodeCount = 0,  GpioController gpioController = null, bool shouldDispose = true)
        {
            if (pins.Length < 2)
            {
                throw new ArgumentException($"{nameof(Charlieplex)}: 2 or more pins must be provided.");
            }

            var charlieCount = (int)Math.Pow(pins.Length, 2) - pins.Length;
            if (nodeCount > charlieCount)
            {
                throw new ArgumentException($"{nameof(Charlieplex)}: maximum count is {charlieCount} based on {pins.Length} pins. {nodeCount} was specified as the count.");
            }

            if (nodeCount == 0)
            {
                nodeCount = charlieCount;
            }

            if (gpioController == null)
            {
                gpioController = new GpioController();
            }

            foreach (var pin in pins)
            {
                gpioController.OpenPin(pin, PinMode.Input);
            }

            _controller = gpioController;
            _shouldDispose = shouldDispose;
            _pins = pins;
            _nodeCount = nodeCount;
            _charlieNodes = GetCharlieLoads(pins, nodeCount);
        }

        /// <summary>
        /// The number of load devices (like LEDs) that can be addressed.
        /// </summary>
        public int LoadCount => _nodeCount;

        /// <summary>
        /// Write a PinValue to a load.
        /// Address scheme is 0-based. Given 8 loads, addresses would be 0-7.
        /// </summary>
        public void Write(int node, PinValue value, int delay = 10)
        {
            _charlieNodes[node].Value = value;

            for (int i = 0; i < delay; i++)
            {
                WriteSegment();
            }
        }

        private void WriteSegment()
        {
            // int delay = 1;
            // configure the circuit back to a neutral configuration
            foreach (var p in _pins)
            {
                _controller.SetPinMode(p, PinMode.Input);
            }

            for (int i = 0; i < _charlieNodes.Length; i++)
            {
                var node = _charlieNodes[i];
                if (node.Value == PinValue.Low)
                {
                    continue;
                }

                // set ground on Cathode leg
                _controller.SetPinMode(node.Cathode, PinMode.Output);
                _controller.Write(node.Cathode, 0);

                // set power on Anode leg
                _controller.SetPinMode(node.Anode, PinMode.Output);
                _controller.Write(node.Anode, 1);
                // Thread.Sleep(delay);
                _controller.SetPinMode(node.Anode, PinMode.Input);
                _controller.SetPinMode(node.Cathode, PinMode.Input);
            }

        }

        /// <summary>
        /// Provides the set of Charlie loads given the set of pins and the count provided.
        /// If count = 0, then the Charlieplex maximum is used for the pins provided (n^2-n).
        /// </summary>
        public static CharlieplexNode[] GetCharlieLoads(int[] pins, int loadCount = 0)
        {
            var pinCount = pins.Length;

            if (loadCount == 0)
            {
                var charlieCount = (int)Math.Pow(pinCount, 2) - pinCount;
                loadCount = charlieCount;
            }

            var charliePins = new CharlieplexNode[loadCount];

            var pin = 0;
            var pinJump = 1;
            var firstLeg = false;
            var resetCount = pinCount - 1;
            for (int i = 0; i < loadCount; i++)
            {
                if ((pin > 0 && pin % resetCount == 0) || pin + pinJump > resetCount)
                {
                    pin = 0;
                    pinJump++;
                }

                var charliePin = new CharlieplexNode();

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
