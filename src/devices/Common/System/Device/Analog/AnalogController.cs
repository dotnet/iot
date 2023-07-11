// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using UnitsNet;

namespace System.Device.Analog
{
    /// <summary>
    /// Base class for Analog Controllers.
    /// These control analog input pins.
    /// </summary>
    public abstract class AnalogController : IDisposable
    {
        private readonly List<AnalogInputPin> _openPins;

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme and driver.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        protected AnalogController(PinNumberingScheme numberingScheme)
        {
            NumberingScheme = numberingScheme;
            _openPins = new List<AnalogInputPin>();
        }

        /// <summary>
        /// The numbering scheme used to represent pins provided by the controller.
        /// </summary>
        public PinNumberingScheme NumberingScheme { get; }

        /// <summary>
        /// The number of pins provided by the controller.
        /// </summary>
        public abstract int PinCount
        {
            get;
        }

        /// <summary>
        /// Reference voltage (the maximum voltage measurable).
        /// For some hardware, it might be necessary to manually set this value for the <see cref="AnalogInputPin.ReadVoltage"/> method to return correct values.
        /// </summary>
        public ElectricPotential VoltageReference
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if the given pin supports analog input
        /// </summary>
        /// <param name="pin">Number of the pin</param>
        /// <returns>True if the pin supports analog input</returns>
        public abstract bool SupportsAnalogInput(int pin);

        /// <summary>
        /// Convert the input pin number to an analog channel number.
        /// The analog channel number is typically named A0 - Axx on Arduino boards. E.g. on Uno and Nano boards, A0
        /// equals digital pin 14.
        /// </summary>
        /// <param name="pinNumber">Pin number</param>
        /// <returns>Analog channel number. Returns -1 if the given pin does not support analog input</returns>
        public virtual int ConvertPinNumberToAnalogChannelNumber(int pinNumber)
        {
            return pinNumber;
        }

        /// <summary>
        /// Convert logical pin to caller's pin numbering scheme.
        /// </summary>
        /// <param name="analogChannelNumber">Logical pin numbering of the board</param>
        /// <returns>Pin number of the given analog channel, or -1 if the input channel is not valid.</returns>
        public virtual int ConvertAnalogChannelNumberToPinNumber(int analogChannelNumber)
        {
            return analogChannelNumber;
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number to open (not the analog channel!).</param>
        public AnalogInputPin OpenPin(int pinNumber)
        {
            // This does the number conversion itself, therefore done first
            if (!SupportsAnalogInput(pinNumber))
            {
                throw new NotSupportedException($"Pin {pinNumber} is not supporting analog input.");
            }

            if (_openPins.Any(x => x.PinNumber == pinNumber))
            {
                throw new InvalidOperationException("The selected pin is already open.");
            }

            AnalogInputPin openPin = OpenPinCore(pinNumber);
            _openPins.Add(openPin);
            return openPin;
        }

        /// <summary>
        /// Overriden by derived classes: Returns an instance of the <see cref="AnalogInputPin"/> for the specified pin.
        /// </summary>
        /// <param name="pinNumber">The pin number to open (not the analog channel)</param>
        /// <returns>An instance of an analog pin</returns>
        protected abstract AnalogInputPin OpenPinCore(int pinNumber);

        /// <summary>
        /// Checks if a specific analog channel is open.
        /// </summary>
        /// <param name="pinNumber">The analog channel number.</param>
        /// <returns>The status if the channel is open or closed.</returns>
        public virtual bool IsPinOpen(int pinNumber)
        {
            return _openPins.Any(x => x.PinNumber == pinNumber);
        }

        /// <summary>
        /// Closes the given pin
        /// </summary>
        /// <param name="pin">The pin to close</param>
        public virtual void ClosePin(AnalogInputPin pin)
        {
            if (_openPins.Remove(pin))
            {
                // This may fire back, therefore the test above
                pin.Dispose();
            }
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            // Make copy, because Dispose manipulates the array.
            var copy = _openPins.ToArray();
            foreach (var pin in copy)
            {
                pin.Dispose();
            }

            _openPins.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
