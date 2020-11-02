// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            _shouldDispose = controller == null ? true : shouldDispose;

            _controller = controller ?? new GpioController();

            Motors = new Motors(_controller);
            Lights = new Lights(_controller);
        }

        #region IDisposable Support

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!(_controller is null))
            {
                if (disposing)
                {
                    Lights.Dispose();
                    Motors.Dispose();

                    if (_shouldDispose)
                    {
                        _controller.Dispose();
                        _controller = null;
                    }
                }
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
