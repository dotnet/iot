// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.LiquidLevel
{
    /// <summary>
    /// Supports any single pin output digital liquid level switch which is configured
    /// </summary>
    public class LiquidLevelSwitch : IDisposable
    {
        private readonly int _dataPin;
        private readonly PinValue _liquidPresentPinState;
        private readonly bool _shouldDispose;

        private GpioController _controller;

        /// <summary>Creates a new instance of the LiquidLevelSwitch.</summary>
        /// <param name="dataPin">The data pin</param>
        /// <param name="liquidPresentPinState">Data pin state representing liquid being present</param>
        /// <param name="pinNumberingScheme">Use the logical or physical pin layout</param>
        /// <param name="gpioController">A Gpio Controller if you want to use a specific one</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public LiquidLevelSwitch(int dataPin, PinValue liquidPresentPinState, GpioController? gpioController = null, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _controller = gpioController ?? new GpioController(pinNumberingScheme);
            _shouldDispose = shouldDispose || gpioController == null;
            _dataPin = dataPin;
            _liquidPresentPinState = liquidPresentPinState;

            _controller.OpenPin(_dataPin, PinMode.Input);

        }

        /// <summary>
        /// Determines whether liquid is present.
        /// </summary>
        /// <returns><code>true</code> if liquid is present, otherwise <code>false</code>.</returns>
        public bool IsLiquidPresent() => _controller.Read(_dataPin) == _liquidPresentPinState;

        /// <summary>
        /// Dispose Buzzer.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
        }
    }
}
