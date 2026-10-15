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
        public ExplorerHat(GpioController? controller = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || controller is null;
            _controller = controller ?? new GpioController();

            // The controller belongs to this instance: motors and lights must not dispose it
            Motors = new Motors(_controller, shouldDispose: false);
            Lights = new Lights(_controller, shouldDispose: false);
        }

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        public void Dispose()
        {
            // Motors first: their software PWM threads write to the controller until they are disposed
            Motors.Dispose();
            Lights.Dispose();

            if (_shouldDispose)
            {
                _controller?.Dispose();
            }

            _controller = null!;
        }
    }
}
