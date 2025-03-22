// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace System.Device.Gpio;

/// <summary>
/// Represents a general-purpose I/O (GPIO) controller.
/// </summary>
public class GpioController : IDisposable
{
    // Constants used to check the hardware on linux
    private const string CpuInfoPath = "/proc/cpuinfo";
    private const string RaspberryPiHardware = "BCM2835";

    // Constants used to check the hardware on Windows
    private const string BaseBoardProductRegistryValue = @"SYSTEM\HardwareConfig\Current\BaseBoardProduct";
    private const string RaspberryPi2Product = "Raspberry Pi 2";
    private const string RaspberryPi3Product = "Raspberry Pi 3";
    private const string RaspberryPi5Product = "Raspberry Pi 5";

    /// <summary>
    /// If a pin element exists, that pin is open. Uses current controller's numbering scheme
    /// </summary>
    private readonly ConcurrentDictionary<int, PinValue?> _openPins;
    private readonly ConcurrentDictionary<int, GpioPin> _gpioPins;
    private GpioDriver _driver;

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioController"/> class that will use the logical pin numbering scheme as default.
    /// </summary>
    public GpioController()
#pragma warning disable CS0612 // PinNumberingScheme is obsolete
        : this(PinNumberingScheme.Logical)
#pragma warning restore CS0612
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme and driver.
    /// </summary>
    /// <param name="driver">The driver that manages all of the pin operations for the controller.</param>
    public GpioController(GpioDriver driver)
    {
        _driver = driver;

#pragma warning disable CS0612 // PinNumberingScheme is obsolete
        NumberingScheme = PinNumberingScheme.Logical;
#pragma warning restore CS0612

        _openPins = new ConcurrentDictionary<int, PinValue?>();
        _gpioPins = new ConcurrentDictionary<int, GpioPin>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme and driver.
    /// </summary>
    /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
    /// <param name="driver">The driver that manages all of the pin operations for the controller.</param>
    [Obsolete]
    public GpioController(PinNumberingScheme numberingScheme, GpioDriver driver)
    {
        _driver = driver;
        NumberingScheme = numberingScheme;
        _openPins = new ConcurrentDictionary<int, PinValue?>();
        _gpioPins = new ConcurrentDictionary<int, GpioPin>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme.
    /// The controller will default to use the driver that best applies given the platform the program is executing on.
    /// </summary>
    /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
    [Obsolete]
    public GpioController(PinNumberingScheme numberingScheme)
        : this(numberingScheme, GetBestDriverForBoard())
    {
    }

    /// <summary>
    /// The numbering scheme used to represent pins provided by the controller.
    /// </summary>
    [Obsolete]
    public PinNumberingScheme NumberingScheme { get; }

    /// <summary>
    /// The number of pins provided by the controller.
    /// </summary>
    public virtual int PinCount
    {
        get
        {
            CheckDriverValid();
            return _driver.PinCount;
        }
    }

    /// <summary>
    /// Returns the collection of open pins
    /// </summary>
    private IEnumerable<GpioPin> OpenPins
    {
        get
        {
            return _gpioPins.Values;
        }
    }

    /// <summary>
    /// Gets the logical pin number in the controller's numbering scheme.
    /// </summary>
    /// <param name="pinNumber">The pin number</param>
    /// <returns>The logical pin number in the controller's numbering scheme.</returns>
    protected virtual int GetLogicalPinNumber(int pinNumber)
    {
#pragma warning disable CS0612 // PinNumberingScheme is obsolete
        return (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : _driver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
#pragma warning restore CS0612
    }

    /// <summary>
    /// Opens a pin in order for it to be ready to use.
    /// The driver attempts to open the pin without changing its mode or value.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    public GpioPin OpenPin(int pinNumber)
    {
        if (IsPinOpen(pinNumber))
        {
            return _gpioPins[pinNumber];
        }

        OpenPinCore(pinNumber);
        _openPins.TryAdd(pinNumber, null);
        _gpioPins[pinNumber] = new GpioPin(pinNumber, this);
        return _gpioPins[pinNumber];
    }

    /// <summary>
    /// Opens a pin in order for it to be ready to use.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    protected virtual void OpenPinCore(int pinNumber)
    {
        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        _driver.OpenPin(logicalPinNumber);
    }

    /// <summary>
    /// Opens a pin and sets it to a specific mode.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="mode">The mode to be set.</param>
    public GpioPin OpenPin(int pinNumber, PinMode mode)
    {
        var pin = OpenPin(pinNumber);
        SetPinMode(pinNumber, mode);
        return pin;
    }

    /// <summary>
    /// Opens a pin and sets it to a specific mode and value.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="mode">The mode to be set.</param>
    /// <param name="initialValue">The initial value to be set if the mode is output. The driver will attempt to set the mode without causing glitches to the other value.
    /// (if <paramref name="initialValue"/> is <see cref="PinValue.High"/>, the pin should not glitch to low during open)</param>
    public GpioPin OpenPin(int pinNumber, PinMode mode, PinValue initialValue)
    {
        var pin = OpenPin(pinNumber);
        // Set the desired initial value
        _openPins[pinNumber] = initialValue;

        SetPinMode(pinNumber, mode);
        return pin;
    }

    /// <summary>
    /// Closes an open pin.
    /// If allowed by the driver, the state of the pin is not changed.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    public void ClosePin(int pinNumber)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not close pin {pinNumber} because it is not open.");
        }

        ClosePinCore(pinNumber);
        _openPins.TryRemove(pinNumber, out _);
    }

    /// <summary>
    /// Closes an open pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    protected virtual void ClosePinCore(int pinNumber)
    {
        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        _driver.ClosePin(logicalPinNumber);
        _gpioPins.TryRemove(pinNumber, out _);
    }

    /// <summary>
    /// Sets the mode to a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="mode">The mode to be set.</param>
    public virtual void SetPinMode(int pinNumber, PinMode mode)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not set a mode to pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        if (!IsPinModeSupported(pinNumber, mode))
        {
            throw new InvalidOperationException($"Pin {pinNumber} does not support mode {mode}.");
        }

        if (_openPins.TryGetValue(pinNumber, out var desired) && desired.HasValue)
        {
            _driver.SetPinMode(logicalPinNumber, mode, desired.Value);
        }
        else
        {
            _driver.SetPinMode(logicalPinNumber, mode);
        }
    }

    /// <summary>
    /// Gets the mode of a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <returns>The mode of the pin.</returns>
    public virtual PinMode GetPinMode(int pinNumber)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not get the mode of pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        return _driver.GetPinMode(logicalPinNumber);
    }

    /// <summary>
    /// Checks if a specific pin is open.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <returns>The status if the pin is open or closed.</returns>
    public virtual bool IsPinOpen(int pinNumber)
    {
        CheckDriverValid();
        return _openPins.ContainsKey(pinNumber);
    }

    private void CheckDriverValid()
    {
        if (_driver == null)
        {
            throw new ObjectDisposedException(nameof(GpioController));
        }
    }

    /// <summary>
    /// Checks if a pin supports a specific mode.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="mode">The mode to check.</param>
    /// <returns>The status if the pin supports the mode.</returns>
    public virtual bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        CheckDriverValid();
        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        return _driver.IsPinModeSupported(logicalPinNumber, mode);
    }

    /// <summary>
    /// Reads the current value of a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <returns>The value of the pin.</returns>
    public virtual PinValue Read(int pinNumber)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not read from pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        return _driver.Read(logicalPinNumber);
    }

    /// <summary>
    /// Toggle the current value of a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    public virtual void Toggle(int pinNumber)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not read from pin {pinNumber} because it is not open.");
        }

        _driver.Toggle(pinNumber);
    }

    /// <summary>
    /// Writes a value to a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="value">The value to be written to the pin.</param>
    public virtual void Write(int pinNumber, PinValue value)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not write to pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);

        _openPins[pinNumber] = value;

        if (_driver.GetPinMode(logicalPinNumber) != PinMode.Output)
        {
            return;
        }

        _driver.Write(logicalPinNumber, value);
    }

    /// <summary>
    /// Blocks execution until an event of type eventType is received or a period of time has expired.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for.</param>
    /// <param name="timeout">The time to wait for the event.</param>
    /// <returns>A structure that contains the result of the waiting operation.</returns>
    public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
    {
        using CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
        return WaitForEvent(pinNumber, eventTypes, tokenSource.Token);
    }

    /// <summary>
    /// Blocks execution until an event of type eventType is received or a cancellation is requested.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for.</param>
    /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
    /// <returns>A structure that contains the result of the waiting operation.</returns>
    public virtual WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not wait for events from pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        return _driver.WaitForEvent(logicalPinNumber, eventTypes, cancellationToken);
    }

    /// <summary>
    /// Async call to wait until an event of type eventType is received or a period of time has expired.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for.</param>
    /// <param name="timeout">The time to wait for the event.</param>
    /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation.</returns>
    public async ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
    {
        using CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
        return await WaitForEventAsync(pinNumber, eventTypes, tokenSource.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Async call until an event of type eventType is received or a cancellation is requested.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for.</param>
    /// <param name="token">The cancellation token of when the operation should stop waiting for an event.</param>
    /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
    public virtual ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken token)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not wait for events from pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        return _driver.WaitForEventAsync(logicalPinNumber, eventTypes, token);
    }

    /// <summary>
    /// Adds a callback that will be invoked when pinNumber has an event of type eventType.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for.</param>
    /// <param name="callback">The callback method that will be invoked.</param>
    public virtual void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not add callback for pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        _driver.AddCallbackForPinValueChangedEvent(logicalPinNumber, eventTypes, callback);
    }

    /// <summary>
    /// Removes a callback that was being invoked for pin at pinNumber.
    /// </summary>
    /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
    /// <param name="callback">The callback method that will be invoked.</param>
    public virtual void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        if (!IsPinOpen(pinNumber))
        {
            throw new InvalidOperationException($"Can not remove callback for pin {pinNumber} because it is not open.");
        }

        int logicalPinNumber = GetLogicalPinNumber(pinNumber);
        _driver.RemoveCallbackForPinValueChangedEvent(logicalPinNumber, callback);
    }

    /// <summary>
    /// Disposes this instance and closes all open pins associated with this controller.
    /// </summary>
    /// <param name="disposing">True to dispose all instances, false to dispose only unmanaged resources</param>
    protected virtual void Dispose(bool disposing)
    {
        foreach (int pin in _openPins.Keys)
        {
            // The list contains the pin in the current NumberingScheme
            ClosePinCore(pin);
        }

        _openPins.Clear();
        _gpioPins.Clear();
        _driver?.Dispose();
        _driver = null!;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
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

    /// <summary>
    /// Read the given pins with the given pin numbers.
    /// </summary>
    /// <param name="pinValuePairs">The pin/value pairs to read.</param>
    public void Read(Span<PinValuePair> pinValuePairs)
    {
        for (int i = 0; i < pinValuePairs.Length; i++)
        {
            int pin = pinValuePairs[i].PinNumber;
            pinValuePairs[i] = new PinValuePair(pin, Read(pin));
        }
    }

    /// <summary>
    /// Tries to create the GPIO driver that best matches the current hardware
    /// </summary>
    /// <returns>An instance of a GpioDriver that best matches the current hardware</returns>
    /// <exception cref="PlatformNotSupportedException">No matching driver could be found</exception>
    private static GpioDriver GetBestDriverForBoard()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return GetBestDriverForBoardOnWindows();
        }
        else
        {
            return GetBestDriverForBoardOnLinux();
        }
    }

    /// <summary>
    /// Attempt to get the best applicable driver for the board the program is executing on.
    /// </summary>
    /// <returns>A driver that works with the board the program is executing on.</returns>
    private static GpioDriver GetBestDriverForBoardOnLinux()
    {
        var boardInfo = RaspberryBoardInfo.LoadBoardInfo();

        switch (boardInfo.BoardModel)
        {
            case RaspberryBoardInfo.Model.RaspberryPi3B:
            case RaspberryBoardInfo.Model.RaspberryPi3APlus:
            case RaspberryBoardInfo.Model.RaspberryPi3BPlus:
            case RaspberryBoardInfo.Model.RaspberryPiZeroW:
            case RaspberryBoardInfo.Model.RaspberryPiZero2W:
            case RaspberryBoardInfo.Model.RaspberryPi4:
            case RaspberryBoardInfo.Model.RaspberryPi400:
            case RaspberryBoardInfo.Model.RaspberryPiComputeModule4:
            case RaspberryBoardInfo.Model.RaspberryPiComputeModule3:

                RaspberryPi3LinuxDriver? internalDriver = RaspberryPi3Driver.CreateInternalRaspberryPi3LinuxDriver(out _);

                if (internalDriver is object)
                {
                    return new RaspberryPi3Driver(internalDriver);
                }

                return UnixDriver.Create();

            case RaspberryBoardInfo.Model.RaspberryPi5:

                // For now, for Raspberry Pi 5, we'll use the LibGpiodDriver.
                // We need to create a new driver for the Raspberry Pi 5,
                // because the Raspberry Pi 5 uses an entirely different GPIO controller (RP1)
#pragma warning disable SDGPIO0001
                var chips = LibGpiodDriver.GetAvailableChips();
                // The RP1 chip reports 54 lines
                GpioChipInfo? selectedChip = chips.FirstOrDefault(x => x.NumLines == 54);
                if (selectedChip is null)
                {
                    throw new NotSupportedException("Couldn't find the default GPIO chip. You might need to create the LibGpiodDriver explicitly");
                }
#pragma warning restore SDGPIO0001
                return new LibGpiodDriver(selectedChip.Id);

            default:

                return UnixDriver.Create();
        }
    }

    /// <summary>
    /// Attempt to get the best applicable driver for the board the program is executing on.
    /// </summary>
    /// <returns>A driver that works with the board the program is executing on.</returns>
    /// <remarks>
    ///     This really feels like it needs a driver-based pattern, where each driver exposes a static method:
    ///     public static bool IsSpecificToCurrentEnvironment { get; }
    ///     The GpioController could use reflection to find all GpioDriver-derived classes and call this
    ///     static method to determine if the driver considers itself to be the best match for the environment.
    /// </remarks>
    private static GpioDriver GetBestDriverForBoardOnWindows()
    {
#pragma warning disable CA1416 // Registry.LocalMachine is only supported on Windows, but we will only hit this method if we are on Windows.
        string? baseBoardProduct = Registry.LocalMachine.GetValue(BaseBoardProductRegistryValue, string.Empty)?.ToString();
#pragma warning restore CA1416

        if (baseBoardProduct is null)
        {
            throw new Exception("Single board computer type cannot be detected.");
        }

        if (baseBoardProduct == RaspberryPi3Product || baseBoardProduct.StartsWith($"{RaspberryPi3Product} ") ||
            baseBoardProduct == RaspberryPi2Product || baseBoardProduct.StartsWith($"{RaspberryPi2Product} "))
        {
            return new RaspberryPi3Driver();
        }

        // Default for Windows IoT Core on a non-specific device
        throw new PlatformNotSupportedException();
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
        ComponentInformation self = new ComponentInformation(this, "Generic GPIO Controller");

        if (_driver != null)
        {
            ComponentInformation driverInfo = _driver.QueryComponentInformation();
            self.AddSubComponent(driverInfo);
        }

        // PinCount is not added on purpose, because the property throws NotSupportedException on some hardware
        self.Properties["OpenPins"] = string.Join(", ", _openPins.Select(x => x.Key));

        return self;
    }
}
