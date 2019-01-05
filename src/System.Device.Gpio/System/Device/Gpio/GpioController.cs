// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio
{
    /// <summary>
    /// Main class to interact with when trying to control Gpio pins.
    /// </summary>
    public sealed partial class GpioController : IDisposable
    {
        private readonly GpioDriver _driver;
        private readonly HashSet<int> _openPins;

        /// <summary>
        /// Initializes new instance of GpioController that will use the logical pin numbering scheme as default.
        /// </summary>
        public GpioController()
            : this(PinNumberingScheme.Logical)
        {
        }

        /// <summary>
        /// Initializes new instance of GpioController that will use the specified numbering scheme and driver.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins on the board.</param>
        /// <param name="driver">The driver that will be in charge of performing all of the pin operations.</param>
        public GpioController(PinNumberingScheme numberingScheme, GpioDriver driver)
        {
            _driver = driver;
            NumberingScheme = numberingScheme;
            _openPins = new HashSet<int>();
        }

        /// <summary>
        /// The numbering scheme used to represent pins on the board.
        /// </summary>
        public PinNumberingScheme NumberingScheme { get; }

        /// <summary>
        /// Returns the number of pins on the board.
        /// </summary>
        public int PinCount
        {
            get
            {
                return _driver.PinCount;
            }
        }

        /// <summary>
        /// Converts a controller's pin number into the logical numbering the driver understands.
        /// </summary>
        /// <param name="pinNumber">The pin number using the controller's numbering scheme.</param>
        /// <returns>The pin number using the logical numbering scheme.</returns>
        private int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return _driver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public void OpenPin(int pinNumber)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("The selected pin is already open.");
            }
            _driver.OpenPin(logicalPinNumber);
            _openPins.Add(logicalPinNumber);
        }

        /// <summary>
        /// Opens a pin and sets it to a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public void OpenPin(int pinNumber, PinMode mode)
        {
            OpenPin(pinNumber);
            SetPinMode(pinNumber, mode);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public void ClosePin(int pinNumber)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not close a pin that is not open.");
            }
            _driver.ClosePin(logicalPinNumber);
            _openPins.Remove(pinNumber);
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public void SetPinMode(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not set a mode to a pin that is not open.");
            }
            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException("The pin does not support the mode selected.");
            }
            _driver.SetPinMode(logicalPinNumber, mode);
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <returns></returns>
        public PinMode GetPinMode(int pinNumber)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not get a mode of a pin that is not open.");
            }
            return _driver.GetPinMode(logicalPinNumber);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        public PinValue Read(int pinNumber)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not read from a pin that is not open.");
            }
            if (_driver.GetPinMode(pinNumber) == PinMode.Output)
            {
                throw new InvalidOperationException("Can not read from a pin that is set to Output mode.");
            }
            return _driver.Read(logicalPinNumber);
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        public void Write(int pinNumber, PinValue value)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not write to a pin that is not open.");
            }
            if (_driver.GetPinMode(pinNumber) != PinMode.Output)
            {
                throw new InvalidOperationException("Can not write to a pin that is not set to Output mode.");
            }
            _driver.Write(logicalPinNumber, value);
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a period of time has expired.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventType">The event types to listen for.</param>
        /// <param name="timeout">The time to wait for the event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, TimeSpan timeout)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
            return WaitForEvent(pinNumber, eventType, tokenSource.Token);
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventType">The event types to listen for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not wait for events from a pin that is not open.");
            }
            return _driver.WaitForEvent(logicalPinNumber, eventType, cancellationToken);
        }

        /// <summary>
        /// Async call to wait until an event of type eventType is received or a period of time has expired.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventType">The event types to listen for.</param>
        /// <param name="timeout">The time to wait for the event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation.</returns>
        public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventType, TimeSpan timeout)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
            return WaitForEventAsync(pinNumber, eventType, tokenSource.Token);
        }

        /// <summary>
        /// Async call until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventType">The event types to listen for.</param>
        /// <param name="token">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
        public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventType, CancellationToken token)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not wait for events from a pin that is not open.");
            }
            return _driver.WaitForEventAsync(logicalPinNumber, eventType, token);
        }

        /// <summary>
        /// Adds a callback that will be invoked when pinNumber has an event of type eventType.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventType">The event types to listen for.</param>
        /// <param name="callback">The callback method that will be invoked.</param>
        public void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not add callback for a pin that is not open.");
            }
            _driver.AddCallbackForPinValueChangedEvent(logicalPinNumber, eventType, callback);
        }

        /// <summary>
        /// Removes a callback that was being invoked for pin at pinNumber.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="callback">The callback method that will be invoked.</param>
        public void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException("Can not add callback for a pin that is not open.");
            }
            _driver.RemoveCallbackForPinValueChangedEvent(logicalPinNumber, callback);
        }

        private int ConvertToLogicalPinNumber(int pinNumber)
        {
            return (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
        }

        /// <summary>
        /// Checks if a specific pin is open.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The status if pin is open (True) or closed (False).</returns>
        public bool IsPinOpen(int pinNumber)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            return _openPins.Contains(logicalPinNumber);
        }

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if a pin supports the mode (True) or not (False).</returns>
        public bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = ConvertToLogicalPinNumber(pinNumber);
            return _driver.IsPinModeSupported(logicalPinNumber, mode);
        }

        private void Dispose(bool disposing)
        {
            foreach(int pin in _openPins)
            {
                _driver.ClosePin(pin);
            }
            _openPins.Clear();
            _driver.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
