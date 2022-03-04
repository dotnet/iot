// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.VirtualGpio
{
    /// <summary>
    /// Program-control-input GPIO controller. For simulation, testing, etc.
    /// </summary>
    public class VirtualGpioController : GpioController
    {
        private VirtualGpioDriver _driver;

        /// <summary>
        /// Create a virtual GPIO controller with a specific number of pins.
        /// </summary>
        /// <param name="pinCount">Number of pins</param>
        public static VirtualGpioController Create(int pinCount)
        {
            var driver = new VirtualGpioDriver(pinCount);
            return new VirtualGpioController(driver);
        }

        private VirtualGpioController(VirtualGpioDriver driver)
            : base(PinNumberingScheme.Logical, driver)
        {
            _driver = driver;
        }

        /// <summary>
        /// Triggered when a input pin value changes.
        /// </summary>
        public event PinChangeEventHandler? InputPinValueChanged
        {
            add => _driver.InputPinValueChanged += value;
            remove => _driver.InputPinValueChanged -= value;
        }

        /// <summary>
        /// Triggered when a output pin value changes.
        /// </summary>
        public event PinChangeEventHandler? OutputPinValueChanged
        {
            add => _driver.OutputPinValueChanged += value;
            remove => _driver.OutputPinValueChanged -= value;
        }

        /// <summary>
        /// Simulates input value for a pin.
        /// </summary>
        /// <param name="pinNumber">Pin number that accepts input</param>
        /// <param name="value">Input value. Null represents a Hi-Z state</param>
        /// <exception cref="SystemException">Throws when the pin is in output mode and try to set a different pin value.</exception>
        public void Input(int pinNumber, PinValue? value) => _driver.Input(pinNumber, value);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _driver?.Dispose();
            _driver = null!;
        }
    }
}
