// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Pimoroni Explorer HAT for Raspberry Pi
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        private readonly bool _shouldDispose;
        private GpioController _controller;

        /// <summary>
        /// Explorer HAT DCMotors collection
        /// </summary>
        public Motors Motors { get; private set; }

        /// <summary>
        /// Explorer HAT led array
        /// </summary>
        public Lights Lights { get; private set; }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        public ExplorerHat(GpioController controller = null, bool shouldDispose = true)
        {
            _controller = controller;
            _shouldDispose = shouldDispose;

            if (_controller is null)
            {
                _controller = new GpioController(PinNumberingScheme.Logical);
            }

            if (_controller.NumberingScheme != PinNumberingScheme.Logical)
            {
                throw new ArgumentException("Invalid GpioController config: NumberingScheme value must be 'Logical'", nameof(controller));
            }

            Motors = new Motors(_controller);
            Lights = new Lights(_controller);
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Lights.Dispose();
                    Motors.Dispose();

                    if (_shouldDispose)
                    {
                        _controller.Dispose();
                    }

                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
