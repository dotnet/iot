// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.RelayBoard
{
    /// <summary>
    /// A generic GPIO relay device.
    /// </summary>
    public class Relay : IDisposable
    {
        private readonly GpioController _controller;

        #region Properties

        private bool _shouldDispose;

        /// <summary>
        /// The pin this relay uses.
        /// </summary>
        public int Pin { get; }

        /// <summary>
        /// Gets the type of relay this device is.
        /// </summary>
        public RelayType Type { get; }

        /// <summary>
        /// Get or set if the relay is currently on
        /// </summary>
        public bool On
        {
            get
            {
                // TODO: make sure this isn't backwards.
                return Type == RelayType.NormallyClosed ? _controller.Read(Pin) == PinValue.High : _controller.Read(Pin) == PinValue.Low;
            }
            set
            {
                // TODO: make sure this isn't backwards.
                var val = Type == RelayType.NormallyClosed ? value : !value;
                _controller.Write(Pin, val);
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Relay"/> class.
        /// </summary>
        /// <param name="pin">The pin this relay is on.</param>
        /// <param name="gpioController">The GPIO controller to use.</param>
        /// <param name="relayType">The type of relay this is.</param>
        /// <param name="pinNumberingScheme">Numbering scheme to use for <paramref name="gpioController"/>, if one is not passed.</param>
        /// <param name="shouldDispose">Whether this class should dispose the <paramref name="gpioController"/>.</param>
        public Relay(int pin, GpioController? gpioController = null, RelayType relayType = RelayType.NormallyClosed, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _shouldDispose = gpioController == null || shouldDispose;
            Pin = pin;
            Type = relayType;
            _controller = gpioController ?? new GpioController(pinNumberingScheme);
            _controller.OpenPin(pin);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _controller?.ClosePin(Pin);

            if (_shouldDispose)
            {
                _controller?.Dispose();
            }
        }
    }
}
