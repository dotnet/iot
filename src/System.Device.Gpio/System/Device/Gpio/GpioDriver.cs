// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio;

/// <summary>
/// Base class for Gpio Drivers.
/// A Gpio driver provides methods to read from and write to digital I/O pins.
/// </summary>
public abstract class GpioDriver : IDisposable
{
    /// <summary>
    /// Finalizer to clean up unmanaged resources
    /// </summary>
    ~GpioDriver()
    {
        Dispose(false);
    }

    /// <summary>
    /// The number of pins provided by the driver.
    /// </summary>
    protected internal abstract int PinCount { get; }

    /// <summary>
    /// Converts a board pin number to the driver's logical numbering scheme.
    /// </summary>
    /// <param name="pinNumber">The board pin number to convert.</param>
    /// <returns>The pin number in the driver's logical numbering scheme.</returns>
    protected internal abstract int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);

    /// <summary>
    /// Opens a pin in order for it to be ready to use.
    /// The driver attempts to open the pin without changing its mode or value.
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
    /// Sets the mode to a pin and sets an initial value for an output pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="mode">The mode to be set.</param>
    /// <param name="initialValue">The initial value if the <paramref name="mode"/> is output. The driver will do it's best to prevent glitches to the other value when
    /// changing from input to output.</param>
    protected internal virtual void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue)
    {
        SetPinMode(pinNumber, mode);
        if (mode == PinMode.Output)
        {
            Write(pinNumber, initialValue);
        }
    }

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
    /// Toggle the current value of a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    protected internal virtual void Toggle(int pinNumber) => Write(pinNumber, !Read(pinNumber));

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
    /// Query information about a component and its children.
    /// </summary>
    /// <returns>A tree of <see cref="ComponentInformation"/> instances.</returns>
    /// <remarks>
    /// The returned data structure (or rather, its string representation) can be used to diagnose problems with incorrect driver types or
    /// other system configuration problems.
    /// This method is currently reserved for debugging purposes. Its behavior its and signature are subject to change.
    /// </remarks>
    public virtual ComponentInformation QueryComponentInformation()
    {
        return new ComponentInformation(this, "Gpio Driver");
    }

    /// <summary>
    /// Gets information about the current chip
    /// </summary>
    /// <returns>An instance of the <see cref="GpioChipInfo"/> record</returns>
    /// <exception cref="NotSupportedException">The current driver does not support this data</exception>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public virtual GpioChipInfo GetChipInfo()
    {
        throw new NotSupportedException();
    }
}
