// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using RelayBoard;

namespace Iot.Device.RelayBoard
{
    /// <summary>
    /// A board containing some <see cref="Relay"/> devices.
    /// </summary>
    /// <example>
    /// Create a relay board, with a relay:
    /// <code>
    /// RelayBoard board = new RelayBoard();
    /// board.CreateRelay(pin: 1);
    /// </code>
    /// </example>
    public class RelayBoard : IDisposable, IEnumerable<Relay>
    {
        private List<Relay> _relays = new List<Relay>();

        private bool _shouldDispose;
        private GpioController _controller;

        /// <summary>
        /// Get's the type of relay board.
        /// </summary>
        public RelayType Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayBoard"/> class.
        /// </summary>
        public RelayBoard(RelayType relayType = RelayType.NormallyClosed, GpioController gpioController = null, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _shouldDispose = gpioController == null || shouldDispose;
            _controller = gpioController ?? new GpioController(pinNumberingScheme);
            Type = relayType;
        }

        /// <summary>
        /// Create and initialize a relay on this board.
        /// </summary>
        /// <param name="pin">Pin to use.</param>
        public void CreateRelay(int pin)
        {
            _relays.Add(new Relay(pin, _controller, Type));
        }

        /// <summary>
        /// Create and initialize relays on the specified pins.
        /// </summary>
        /// <param name="pins">List of pins to create relays on.</param>
        public void CreateRelays(params int[] pins)
        {
            foreach (var pin in pins)
            {
                CreateRelay(pin);
            }
        }

        /// <summary>
        /// Remove a relay from this board.
        /// </summary>
        /// <param name="pin">Pin the relay is on.</param>
        public void RemoveRelay(int pin)
        {
            var relay = _relays.FindIndex(x => x.Pin == pin);
            if (relay == -1)
            {
                throw new ArgumentOutOfRangeException($"No relay exists on pin {pin}", nameof(pin));
            }

            _relays.RemoveAt(relay);
        }

        /// <summary>
        /// Remove a relay from this board.
        /// </summary>
        /// <param name="relay">The relay to remove.</param>
        public void RemoveRelay(Relay relay)
        {
            _relays.Remove(relay);
        }

        /// <summary>
        /// Get a relay based on the specified pin.
        /// </summary>
        /// <param name="pin">The pin of the relay.</param>
        /// <returns>The relay, or null if a relay does not exist on that pin.</returns>
        public Relay GetRelay(int pin)
        {
            return _relays.FirstOrDefault(x => x.Pin == pin);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
            }

            if (_relays != null)
            {
                foreach (var item in _relays)
                {
                    item?.Dispose();
                }

                _relays = null;
            }
        }

        #region IEnumerable

        /// <inheritdoc/>
        public IEnumerator<Relay> GetEnumerator()
        {
            return _relays.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _relays.GetEnumerator();
        }
        #endregion
    }
}
