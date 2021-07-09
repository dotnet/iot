// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// Used to describe the pin status inside the driver.
    /// </summary>
    public class PinState
    {
        /// <summary>
        /// Initialize <see cref="PinState"/>.
        /// </summary>
        /// <param name="currentMode">Pin mode.</param>
        public PinState(PinMode currentMode)
        {
            CurrentPinMode = currentMode;
            InUseByInterruptDriver = false;
        }

        /// <summary>
        /// Current pin mode.
        /// </summary>
        public PinMode CurrentPinMode { get; set; }

        /// <summary>
        /// Is the pin used by the interrupt driver.
        /// </summary>
        public bool InUseByInterruptDriver { get; set; }
    }
}
