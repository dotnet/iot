// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.LiquidLevelSwitch
{
    /// <summary>
    /// Represents base class for Liquid Level Switches
    /// </summary>
    public abstract class LiquidLevelSwitchBase : IDisposable
    {
        private readonly int _pin;
        private readonly PinMode _switchMode;
        private readonly bool _shouldDispose;

        private GpioController _controller;

        /// <summary>Initializes a new instance of the <see cref="LiquidLevelSwitchBase" /> class.</summary>
        /// <param name="pin">The pin.</param>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="switchMode">The switch mode.</param>
        /// <param name="pinNumberingScheme">The pin numbering scheme.</param>
        /// <param name="shouldDispose">if set to <c>true</c> [should dispose].</param>
        public LiquidLevelSwitchBase(int pin, PinMode switchMode, GpioController gpioController = null,
                                     PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _controller = gpioController != null ? gpioController : new GpioController(pinNumberingScheme);
            _pin = pin;
            _switchMode = switchMode;

            _controller.OpenPin(_pin, System.Device.Gpio.PinMode.Input);

            _shouldDispose = shouldDispose || gpioController == null;
        }

        /// <summary>Determines whether [is liquid present].</summary>
        /// <returns>
        ///   <c>true</c> if [is liquid present]; otherwise, <c>false</c>.</returns>
        public bool IsLiquidPresent()
        {
            var value = _controller.Read(_pin);

            return value == (_switchMode == PinMode.LiquidLow ? PinValue.Low : PinValue.High);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }
    }
}
