// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;

namespace RelayBoard
{
    /// <summary>
    /// A relay.
    /// </summary>
    public class Relay : IDisposable
    {
        private GpioController _controller;

        #region Properties
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
        /// <param name="controller">The GPIO controller to use.</param>
        /// <param name="relayType">The type of relay this is.</param>
        public Relay(int pin, GpioController controller, RelayType relayType = RelayType.NormallyClosed)
        {
            Pin = pin;
            Type = relayType;
            _controller = controller;
            _controller.OpenPin(pin);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _controller?.ClosePin(Pin);
        }
    }
}
