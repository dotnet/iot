// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WinGpio = global::Windows.Devices.Gpio;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Windows 10 IoT.
    /// </summary>
    public class Windows10Driver : GpioDriver
    {
        private static readonly WinGpio.GpioController s_winGpioController = WinGpio.GpioController.GetDefault();
        private readonly Dictionary<int, Windows10DriverPin> _openPins = new Dictionary<int, Windows10DriverPin>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Windows10Driver"/> class.
        /// </summary>
        public Windows10Driver()
        {
            if (s_winGpioController == null)
            {
                throw new NotSupportedException("No GPIO controllers exist on this system.");
            }
        }

        /// <summary>
        /// The number of pins provided by the driver.
        /// </summary>
        protected internal override int PinCount => s_winGpioController.PinCount;

        protected override void Dispose(bool disposing)
        {
            foreach (Windows10DriverPin devicePin in _openPins.Values)
            {
                devicePin.Dispose();
            }

            _openPins.Clear();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
            => LookupOpenPin(pinNumber).AddCallbackForPinValueChangedEvent(eventTypes, callback);

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void ClosePin(int pinNumber)
        {
            if (_openPins.TryGetValue(pinNumber, out Windows10DriverPin pin))
            {
                pin.ClosePin();
                _openPins.Remove(pinNumber);
            }
        }

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
            => throw new PlatformNotSupportedException($"The {GetType().Name} driver does not support physical (board) numbering, since it's generic.");

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber) => LookupOpenPin(pinNumber).GetPinMode();

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => LookupOpenPin(pinNumber).IsPinModeSupported(mode);

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void OpenPin(int pinNumber)
        {
            // Ignore calls to open an already open pin
            if (_openPins.ContainsKey(pinNumber))
            {
                return;
            }

            if (pinNumber < 0 || pinNumber >= PinCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }

            WinGpio.GpioPin windowsPin = s_winGpioController.OpenPin(pinNumber, WinGpio.GpioSharingMode.Exclusive);
            _openPins[pinNumber] = new Windows10DriverPin(this, windowsPin);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal override PinValue Read(int pinNumber) => LookupOpenPin(pinNumber).Read();

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
            => LookupOpenPin(pinNumber).RemoveCallbackForPinValueChangedEvent(callback);

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected internal override void SetPinMode(int pinNumber, PinMode mode) => LookupOpenPin(pinNumber).SetPinMode(mode);

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
            => LookupOpenPin(pinNumber).WaitForEvent(eventTypes, cancellationToken);

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal override void Write(int pinNumber, PinValue value) => LookupOpenPin(pinNumber).Write(value);

        /// <summary>
        /// Lookups an open pin in the driver.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The open pin in the driver.</returns>
        private Windows10DriverPin LookupOpenPin(int pinNumber) => _openPins[pinNumber];
    }
}
