// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio
{
    /// <summary>
    /// Represents a general-purpose I/O (GPIO) pin.
    /// </summary>
    public class Gpio​Pin
    {
        private readonly int _pinNumber;
        private readonly GpioDriver _driver;

        internal Gpio​Pin(int pinNumber, GpioDriver driver)
        {
            _driver = driver;
            _pinNumber = pinNumber;
        }

        /// <summary>
        /// Gets the pin number of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <value>
        /// The pin number of the GPIO pin.
        /// </value>
        public virtual int PinNumber => _pinNumber;

        /// <summary>
        /// Gets the current pin mode for the general-purpose I/O (GPIO) pin. The pin mode specifies whether the pin is configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <returns>An enumeration value that indicates the current pin mode for the GPIO pin.
        /// The pin mode specifies whether the pin is configured as an input or an output, and determines how values are driven onto the pin.</returns>
        public virtual PinMode GetPinMode() => _driver.GetPinMode(_pinNumber);

        /// <summary>
        /// Gets whether the general-purpose I/O (GPIO) pin supports the specified pin mode.
        /// </summary>
        /// <param name="pinMode">The pin mode that you want to check for support.</param>
        /// <returns>
        /// <see langword="true"/> if the GPIO pin supports the pin mode that pinMode specifies; otherwise false.
        /// If you specify a pin mode for which this method returns <see langword="false"/> when you call <see cref="SetPinMode"/>, <see cref="SetPinMode"/> generates an exception.
        /// </returns>
        public virtual bool IsPinModeSupported(PinMode pinMode) => _driver.IsPinModeSupported(_pinNumber, pinMode);

        /// <summary>
        /// Sets the pin mode of the general-purpose I/O (GPIO) pin.
        /// The pin mode specifies whether the pin is configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <param name="value">An enumeration value that specifies pin mode to use for the GPIO pin.
        /// The pin mode specifies whether the pin is configured as an input or an output, and determines how values are driven onto the pin.</param>
        /// <exception cref="ArgumentException">The GPIO pin does not support the specified pin mode.</exception>
        public virtual void SetPinMode(PinMode value) => _driver.SetPinMode(_pinNumber, value);

        /// <summary>
        /// Reads the current value of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <returns>The current value of the GPIO pin. If the pin is configured as an output, this value is the last value written to the pin.</returns>
        public virtual PinValue Read() => _driver.Read(_pinNumber);

        /// <summary>
        /// Drives the specified value onto the general purpose I/O (GPIO) pin according to the current pin mode for the pin
        /// if the pin is configured as an output, or updates the latched output value for the pin if the pin is configured as an input.
        /// </summary>
        /// <param name="value">The enumeration value to write to the GPIO pin.
        /// <para>If the GPIO pin is configured as an output, the method drives the specified value onto the pin according to the current pin mode for the pin.</para>
        /// <para>If the GPIO pin is configured as an input, the method updates the latched output value for the pin. The latched output value is driven onto the pin when the configuration for the pin changes to output.</para>
        /// </param>
        /// <exception cref="InvalidOperationException">This exception will be thrown on an attempt to write to a pin that hasn't been opened or is not configured as output.</exception>
        public virtual void Write(PinValue value) => _driver.Write(_pinNumber, value);

        /// <summary>
        /// Occurs when the value of the general-purpose I/O (GPIO) pin changes, either because of an external stimulus when the pin is configured as an input, or when a value is written to the pin when the pin in configured as an output.
        /// </summary>
        public virtual event PinChangeEventHandler ValueChanged
        {
            add
            {
                _driver.AddCallbackForPinValueChangedEvent(_pinNumber, PinEventTypes.Falling | PinEventTypes.Rising, value);
            }

            remove
            {
                _driver.RemoveCallbackForPinValueChangedEvent(_pinNumber, value);
            }
        }

        /// <summary>
        /// Toggles the output of the general purpose I/O (GPIO) pin if the pin is configured as an output.
        /// </summary>
        public virtual void Toggle() => _driver.Toggle(_pinNumber);
    }
}
