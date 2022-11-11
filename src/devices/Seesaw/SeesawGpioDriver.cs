// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Seesaw
{
    /// <summary>
    /// Seesaw GPIO driver
    /// </summary>
    public class SeesawGpioDriver : GpioDriver
    {
        private readonly Dictionary<int, PinMode> _openPins;
        private readonly Dictionary<int, PinValue> _pinValues;

        private Seesaw _seesawDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeesawGpioDriver"/> class that will use the specified I2cDevice to communicate with the Seesaw device.
        /// </summary>
        /// <param name="i2cDevice">The I2cDevice used for communicating with the Seesaw device.</param>
        public SeesawGpioDriver(I2cDevice i2cDevice)
            : this(new Seesaw(i2cDevice))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeesawGpioDriver"/> class that will use the specified Seesaw device.
        /// </summary>
        /// <param name="seesawDevice">The Seesaw device used fir communicating with the Gpio.</param>
        public SeesawGpioDriver(Seesaw seesawDevice)
        {
            _seesawDevice = seesawDevice;

            if (!_seesawDevice.HasModule(Seesaw.SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {seesawDevice.I2cDevice.ConnectionSettings.BusId}, Address 0x{seesawDevice.I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw GPIO functionality");
            }

            _openPins = new Dictionary<int, PinMode>();
            _pinValues = new Dictionary<int, PinValue>();
        }

        /// <summary>
        /// Number of Gpio pins available.
        /// </summary>
        /// <remarks>Hardcoded to 64 pins as the Seesaw devices do not describe the number of pins.</remarks>
        /// <value>Number of Gpio pins available</value>
        protected override int PinCount => 64;

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        protected override void ClosePin(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException("Can not close a pin that is not open.");
            }

            _openPins.Remove(pinNumber);
            _pinValues.Remove(pinNumber);
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected override PinMode GetPinMode(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException("Can not get the mode a pin that is not open.");
            }

            return _openPins[pinNumber];
        }

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <remarks>All standard pin modes are supported</remarks>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            return true;
        }

        /// <summary>
        /// Checks if a specific pin is open.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The status if the pin is open or closed.</returns>
        public bool IsPinOpen(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            return _openPins.ContainsKey(pinNumber);
        }

        /// <summary>
        /// Opens a pin and in Input mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        protected override void OpenPin(int pinNumber) => OpenPin(pinNumber, PinMode.Input);

        /// <summary>
        /// Opens a pin and sets it to a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public void OpenPin(int pinNumber, PinMode mode)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            if (IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException("The selected pin is already open.");
            }

            _openPins.Add(pinNumber, mode);
            _pinValues.Add(pinNumber, PinValue.Low);
            SetPinMode(pinNumber, mode);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected override PinValue Read(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException("Can not read a value from a pin that is not open.");
            }

            _pinValues[pinNumber] = _seesawDevice.ReadGpioDigital((byte)pinNumber) ? PinValue.High : PinValue.Low;
            return _pinValues[pinNumber];
        }

        /// <inheritdoc/>
        protected override void Toggle(int pinNumber) => Write(pinNumber, !_pinValues[pinNumber]);

        /// <summary>
        /// Read the given pins with the given pin numbers.
        /// </summary>
        /// <param name="pinValuePairs">The pin/value pairs to read.</param>
        public void Read(Span<PinValuePair> pinValuePairs)
        {
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                pinValuePairs[i] = new PinValuePair(pinValuePairs[i].PinNumber, Read(pinValuePairs[i].PinNumber));
            }
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            if (!_openPins.ContainsKey(pinNumber))
            {
                throw new InvalidOperationException("Can not set a mode to a pin that is not open.");
            }

            _seesawDevice.SetGpioPinMode((byte)pinNumber, mode);
            _openPins[pinNumber] = mode;
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected override void Write(int pinNumber, PinValue value)
        {
            if (pinNumber < 0 || pinNumber > 63)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Gpio pin must be within 0-63 range.");
            }

            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException("Can not set a value to a pin that is not open.");
            }

            _seesawDevice.WriteGpioDigital((byte)pinNumber, (value == PinValue.High));
            _pinValues[pinNumber] = value;
        }

        /// <summary>
        /// Write the given pins with the given values.
        /// </summary>
        /// <param name="pinValuePairs">The pin/value pairs to write.</param>
        public void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
        {
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                Write(pinValuePairs[i].PinNumber, pinValuePairs[i].PinValue);
            }
        }

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            foreach (int pinNumber in _openPins.Keys)
            {
                ClosePin(pinNumber);
            }

            _openPins.Clear();
            _seesawDevice?.Dispose();
            _seesawDevice = null!;
            base.Dispose(disposing);
        }
    }
}
