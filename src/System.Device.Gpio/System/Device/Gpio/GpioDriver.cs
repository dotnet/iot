// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio
{
    /// <summary>
    /// Base class for Gpio Drivers.
    /// A Gpio driver provides methods to read from and write to digital I/O pins.
    /// </summary>
    public abstract class GpioDriver : IDisposable
    {
        /// <summary>
        /// The number of pins provided by the driver.
        /// </summary>
        protected internal abstract int PinCount { get; }

        /// <summary>
        /// True if this driver supports extended pin mode settings.
        /// </summary>
        protected internal virtual bool ExtendedPinModeSupported => false;

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal abstract int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal abstract void OpenPin(int pinNumber);

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal abstract void ClosePin(int pinNumber);

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected internal abstract void SetPinMode(int pinNumber, PinMode mode);

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal abstract PinMode GetPinMode(int pinNumber);

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected internal abstract bool IsPinModeSupported(int pinNumber, PinMode mode);

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal abstract PinValue Read(int pinNumber);

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal abstract void Write(int pinNumber, PinValue value);

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        protected internal abstract WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Async call until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
        protected internal virtual ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            return new ValueTask<WaitForEventResult>(Task.Run(() => WaitForEvent(pinNumber, eventTypes, cancellationToken)));
        }

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal abstract void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback);

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal abstract void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback);

        /// <summary>
        /// Disposes this instance, closing all open pins
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing">True if explicitly disposing, false if in finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }

        /// <summary>
        /// Retrieve the current alternate pin mode for a given logical pin.
        /// This works also with closed pins.
        /// </summary>
        /// <param name="logicalPinNumber">Pin number in the logical scheme of the driver</param>
        /// <returns>An instance describing the active pin mode</returns>
        protected internal virtual ExtendedPinMode GetExtendedPinMode(int logicalPinNumber)
        {
            // Virtual instead of abstract, so as not to be breaking.
            // It is highly recommended to update the drivers to support this method though.
            // Note that we cannot use the GetPinMode() method here to eventually return AlternatePinMode.Gpio, since that
            // method requires the pin to be open, and this method must also work for closed pins, because it
            // is illegal to open a pin in Gpio mode when it is not set to Gpio.
            throw new NotSupportedException("This driver does not support alternate modes");
        }

        /// <summary>
        /// Set the specified alternate mode for the given pin.
        /// </summary>
        /// <param name="logicalPinNumber">Pin number in the logcal scheme of the driver</param>
        /// <param name="altMode">Alternate mode to set</param>
        /// <exception cref="NotSupportedException">This mode is not supported by this driver (or by the given pin)</exception>
        protected internal virtual void SetExtendedPinMode(int logicalPinNumber, ExtendedPinMode altMode)
        {
            // Virtual instead of abstract, so as not to be breaking.
            throw new NotSupportedException("This driver does not support alternate modes");
        }
    }
}
