using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;

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
        public AnalogController(PinNumberingScheme numberingScheme)
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
        public double VoltageReference
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
        /// Convert the input pin number to logical pin numbers.
        /// </summary>
        /// <param name="pinNumber">Pin Number, in the numbering scheme of the analog controller</param>
        /// <returns>Logical pin number</returns>
        public virtual int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        /// <summary>
        /// Convert logical pin to caller's pin numbering scheme.
        /// </summary>
        /// <param name="logicalPinNumber">Logical pin numbering of the board</param>
        /// <returns>Pin number in the scheme of the parent analog controller</returns>
        public virtual int ConvertLogicalNumberingSchemeToPinNumber(int logicalPinNumber)
        {
            return logicalPinNumber;
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public AnalogInputPin OpenPin(int pinNumber)
        {
            // This does the number conversion itself, therefore done first
            if (!SupportsAnalogInput(pinNumber))
            {
                throw new NotSupportedException($"Pin {pinNumber} is not supporting analog input.");
            }

            int logicalPinNumber = ConvertPinNumberToLogicalNumberingScheme(pinNumber);

            if (_openPins.Any(x => x.PinNumber == pinNumber))
            {
                throw new InvalidOperationException("The selected pin is already open.");
            }

            AnalogInputPin openPin = OpenPinInternal(logicalPinNumber);
            _openPins.Add(openPin);
            return openPin;
        }

        /// <summary>
        /// Overriden by derived classes: Returns an instance of the <see cref="AnalogInputPin"/> for the specified pin.
        /// </summary>
        /// <param name="pinNumber">Pin number, in the logical pin numbering scheme</param>
        /// <returns>An instance of a pin</returns>
        protected abstract AnalogInputPin OpenPinInternal(int pinNumber);

        /// <summary>
        /// Checks if a specific pin is open.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The status if the pin is open or closed.</returns>
        public virtual bool IsPinOpen(int pinNumber)
        {
            int logicalPinNumber = ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            return _openPins.Any(x => x.PinNumber == logicalPinNumber);
        }

        /// <summary>
        /// Closes the given pin
        /// </summary>
        /// <param name="pin">The pin to close</param>
        public virtual void Close(AnalogInputPin pin)
        {
            if (_openPins.Remove(pin))
            {
                // This may fire back, therefore the test above
                pin.Close();
            }
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            foreach (var pin in _openPins)
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
