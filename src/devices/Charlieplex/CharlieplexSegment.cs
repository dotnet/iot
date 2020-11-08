using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Provides support for Charlieplex multiplexing.
    /// https://wikipedia.org/wiki/Charlieplexing
    /// </summary>
    public class CharlieplexSegment : IDisposable
    {
        private readonly bool _shouldDispose;
        private readonly int[] _pins;
        private readonly CharlieplexSegmentNode[] _nodes;
        private readonly int _nodeCount;
        private GpioController _gpioController;
        private CharlieplexSegmentNode _lastNode;

        /// <summary>
        /// Initializes a new Charlieplex type that can be use for multiplex over a relatively small number of GPIO pins.
        /// </summary>
        /// <param name="pins">The set of pins to use.</param>
        /// <param name="nodeCount">The count of nodes (like LEDs) that will be addressable. If 0, then the Charlieplex maximum is used for the pins provided (n^2-n).</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public CharlieplexSegment(int[] pins, int nodeCount = 0,  GpioController? gpioController = null, bool shouldDispose = true)
        {
            if (pins.Length < 2)
            {
                throw new ArgumentException($"{nameof(CharlieplexSegment)}: 2 or more pins must be provided.");
            }

            int charlieCount = (int)Math.Pow(pins.Length, 2) - pins.Length;
            if (nodeCount > charlieCount)
            {
                throw new ArgumentException($"{nameof(CharlieplexSegment)}: maximum count is {charlieCount} based on {pins.Length} pins. {nodeCount} was specified as the count.");
            }

            if (nodeCount == 0)
            {
                nodeCount = charlieCount;
            }

            _shouldDispose = shouldDispose || gpioController is null;
            _gpioController = gpioController ?? new ();

            // first two pins will be needed as Output.
            _gpioController.OpenPin(pins[0], PinMode.Output);
            _gpioController.OpenPin(pins[1], PinMode.Output);

            // remaining pins should be input type
            // prevents participating in the circuit until needed
            for (int i = 2; i < pins.Length; i++)
            {
                _gpioController.OpenPin(pins[i], PinMode.Input);
            }

            _lastNode = new CharlieplexSegmentNode()
            {
                Anode = pins[1],
                Cathode = pins[0]
            };
            _pins = pins;
            _nodeCount = nodeCount;
            _nodes = GetNodes(pins, nodeCount);
        }

        /// <summary>
        /// The number of nodes (like LEDs) that can be addressed.
        /// </summary>
        public int NodeCount => _nodeCount;

        /// <summary>
        /// Write a PinValue to a node, to update Charlieplex segment.
        /// Address scheme is 0-based. Given 8 nodes, addresses would be 0-7.
        /// Displays nodes in their updated configuration for the specified duration.
        /// </summary>
        /// <param name="node">Node to update.</param>
        /// <param name="value">Value to write.</param>
        /// <param name="duration">Time to display segment, in milliseconds (default is 0; not displayed).</param>
        public void Write(int node, PinValue value, int duration = 0)
        {
            _nodes[node].Value = value;

            if (duration > 0)
            {
                DisplaySegment(TimeSpan.FromMilliseconds(duration));
            }
        }

        /// <summary>
        /// Displays nodes in their current configuration for the specified duration.
        /// </summary>
        /// <param name="duration">Time to display segment.</param>
        public void DisplaySegment(TimeSpan duration)
        {
            /*
                Cases to consider
                node.Cathode == _lastNode.Cathode
                node.Cathode == _lastNode.Anode -- drop low
                node.Anode == _lastNode.Cathode
                node.Anode == _lastNode.Anode
                node.Anode != _lastNode.Cathode | _lastNode.Anode
                node.Cathode != _lastNode.Cathode | _lastNode.Anode
            */

            Stopwatch watch = Stopwatch.StartNew();
            do
            {
                for (int i = 0; i < _nodes.Length; i++)
                {
                    CharlieplexSegmentNode node = _nodes[i];

                    // skip updating pinmode when possible
                    if (_lastNode.Anode != node.Anode && _lastNode.Anode != node.Cathode)
                    {
                        _gpioController.SetPinMode(_lastNode.Anode, PinMode.Input);
                    }

                    if (_lastNode.Cathode != node.Anode && _lastNode.Cathode != node.Cathode)
                    {
                        _gpioController.SetPinMode(_lastNode.Cathode, PinMode.Input);
                    }

                    if (node.Cathode != _lastNode.Anode && node.Cathode != _lastNode.Cathode)
                    {
                        _gpioController.SetPinMode(node.Cathode, PinMode.Output);
                    }

                    if (node.Anode != _lastNode.Anode && node.Anode != _lastNode.Cathode)
                    {
                        _gpioController.SetPinMode(node.Anode, PinMode.Output);
                    }

                    _gpioController.Write(node.Anode, node.Value);
                    // It is necessary to sleep for the LED to be seen with full brightness
                    // It may be possible to sleep less than 1ms -- this API has ms granularity
                    Thread.Sleep(1);
                    _gpioController.Write(node.Anode, 0);
                    _lastNode.Anode = node.Anode;
                    _lastNode.Cathode = node.Cathode;
                }
            }
            while (watch.Elapsed < duration);
        }

        /// <summary>
        /// Provides the set of Charlie nodes given the set of pins and the count provided.
        /// If count = 0, then the Charlieplex maximum is used for the pins provided (n^2-n).
        /// </summary>
        /// <param name="pins">The pins to use for the segment.</param>
        /// <param name="nodeCount">The number of nodes to use. Default is the Charlieplex maximum.</param>
        public static CharlieplexSegmentNode[] GetNodes(int[] pins, int nodeCount = 0)
        {
            int pinCount = pins.Length;

            if (nodeCount == 0)
            {
                nodeCount = (int)Math.Pow(pinCount, 2) - pinCount;
            }

            CharlieplexSegmentNode[] nodes = new CharlieplexSegmentNode[nodeCount];

            int pin = 0;
            int pinJump = 1;
            int resetCount = pinCount - 1;
            bool firstLeg = false;
            for (int i = 0; i < nodeCount; i++)
            {
                if ((pin > 0 && pin % resetCount == 0) || pin + pinJump > resetCount)
                {
                    pin = 0;
                    pinJump++;
                }

                CharlieplexSegmentNode node = new CharlieplexSegmentNode();

                if (!firstLeg)
                {
                    node.Anode = pins[pin];
                    node.Cathode = pins[pin + pinJump];
                    firstLeg = true;
                }
                else
                {
                    node.Anode = pins[pin + pinJump];
                    node.Cathode = pins[pin];
                    firstLeg = false;
                    pin++;
                }

                nodes[i] = node;
            }

            return nodes;
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
                _gpioController?.Dispose();
                _gpioController = null!;
            }
        }
    }
}
