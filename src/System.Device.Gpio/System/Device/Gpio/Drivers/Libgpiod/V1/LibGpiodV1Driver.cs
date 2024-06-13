// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Device.Gpio.Libgpiod.V1;
using System.Diagnostics;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Drivers.Libgpiod.V1;

/// <summary>
/// This driver uses libgpiod v0/1 to get user-level access to the gpio ports.
/// It superseeds the SysFsDriver, but requires that libgpiod is installed. To do so, run
/// "sudo apt install -y libgpiod-dev".
/// </summary>
internal class LibGpiodV1Driver : UnixDriver
{
    private static string s_consumerName = Process.GetCurrentProcess().ProcessName;
    private readonly object _pinNumberLock;
    private readonly ConcurrentDictionary<int, LineHandle> _pinNumberToSafeLineHandle;
    private readonly ConcurrentDictionary<int, LibGpiodV1DriverEventHandler> _pinNumberToEventHandler;
    private readonly int _pinCount;
    private readonly ConcurrentDictionary<int, PinValue> _pinValue;
    private SafeChipHandle _chip;

    /// <inheritdoc />
    protected internal override int PinCount => _pinCount;

    // for use the bias flags we need libgpiod version 1.5 or later
    private static bool IsLibgpiodVersion1_5orHigher()
    {
        IntPtr libgpiodVersionPtr = LibgpiodV1.gpiod_version_string();
        string? libgpiodVersionMatch = Marshal.PtrToStringAnsi(libgpiodVersionPtr);

        if (libgpiodVersionMatch is object)
        {
            Version libgpiodVersion = new Version(libgpiodVersionMatch);
            return (libgpiodVersion.Major >= 1 && libgpiodVersion.Minor >= 5);
        }

        return false;
    }

    private static bool s_isLibgpiodVersion1_5orHigher = IsLibgpiodVersion1_5orHigher();

    private enum RequestFlag : ulong
    {
        GPIOD_LINE_REQUEST_FLAG_OPEN_DRAIN = (1UL << 0),
        GPIOD_LINE_REQUEST_FLAG_OPEN_SOURCE = (1UL << 1),
        GPIOD_LINE_REQUEST_FLAG_ACTIVE_LOW = (1UL << 2),
        GPIOD_LINE_REQUEST_FLAG_BIAS_DISABLE = (1UL << 3),
        GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN = (1UL << 4),
        GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP = (1UL << 5)
    }

    /// <summary>
    /// Construct an instance
    /// </summary>
    /// <param name="gpioChip">Number of the gpio Chip. Default 0</param>
    public LibGpiodV1Driver(int gpioChip = 0)
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Linux/Unix.");
        }

        try
        {
            _pinNumberLock = new object();
            _chip = LibgpiodV1.gpiod_chip_open_by_number(gpioChip);
            if (_chip == null || _chip.IsInvalid || _chip.IsClosed)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.NoChipFound, Marshal.GetLastWin32Error());
            }

            _pinCount = LibgpiodV1.gpiod_chip_num_lines(_chip);
            _pinNumberToEventHandler = new ConcurrentDictionary<int, LibGpiodV1DriverEventHandler>();
            _pinNumberToSafeLineHandle = new ConcurrentDictionary<int, LineHandle>();
            _pinValue = new ConcurrentDictionary<int, PinValue>();
        }
        catch (DllNotFoundException)
        {
            throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.LibGpiodNotInstalled);
        }
    }

    /// <inheritdoc/>
    protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        if ((eventTypes & PinEventTypes.Rising) != 0 || (eventTypes & PinEventTypes.Falling) != 0)
        {
            LibGpiodV1DriverEventHandler eventHandler = _pinNumberToEventHandler.GetOrAdd(pinNumber, PopulateEventHandler);

            if ((eventTypes & PinEventTypes.Rising) != 0)
            {
                eventHandler.ValueRising += callback;
            }

            if ((eventTypes & PinEventTypes.Falling) != 0)
            {
                eventHandler.ValueFalling += callback;
            }
        }
        else
        {
            throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
        }
    }

    private LibGpiodV1DriverEventHandler PopulateEventHandler(int pinNumber)
    {
        lock (_pinNumberLock)
        {
            _pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle);

            if (pinHandle is null || (pinHandle is object && !LibgpiodV1.gpiod_line_is_free(pinHandle.Handle)))
            {
                pinHandle?.Dispose();
                pinHandle = new LineHandle(LibgpiodV1.gpiod_chip_get_line(_chip, pinNumber));
                _pinNumberToSafeLineHandle[pinNumber] = pinHandle;
            }

            return new LibGpiodV1DriverEventHandler(pinNumber, pinHandle!);
        }
    }

    /// <inheritdoc/>
    protected internal override void ClosePin(int pinNumber)
    {
        lock (_pinNumberLock)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle) &&
                !IsListeningEvent(pinNumber))
            {
                pinHandle?.Dispose();
                // We know this works
                _pinNumberToSafeLineHandle.TryRemove(pinNumber, out _);
                _pinValue.TryRemove(pinNumber, out _);
            }
        }
    }

    private bool IsListeningEvent(int pinNumber)
    {
        return _pinNumberToEventHandler.ContainsKey(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) =>
        throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.ConvertPinNumberingSchemaError);

    /// <inheritdoc/>
    protected internal override PinMode GetPinMode(int pinNumber)
    {
        lock (_pinNumberLock)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError,
                    pin: pinNumber);
            }

            return pinHandle.PinMode;
        }
    }

    /// <inheritdoc/>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => mode switch
    {
        PinMode.Input or PinMode.Output => true,
        PinMode.InputPullDown or PinMode.InputPullUp => s_isLibgpiodVersion1_5orHigher,
        _ => false,
    };

    /// <inheritdoc/>
    protected internal override void OpenPin(int pinNumber)
    {
        lock (_pinNumberLock)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out _))
            {
                return;
            }

            LineHandle pinHandle = new LineHandle(LibgpiodV1.gpiod_chip_get_line(_chip, pinNumber));
            if (pinHandle == null)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.OpenPinError, Marshal.GetLastWin32Error());
            }

            int mode = LibgpiodV1.gpiod_line_direction(pinHandle.Handle);
            if (mode == 1)
            {
                pinHandle.PinMode = PinMode.Input;
            }
            else if (mode == 2)
            {
                pinHandle.PinMode = PinMode.Output;
            }

            if (s_isLibgpiodVersion1_5orHigher && pinHandle.PinMode == PinMode.Input)
            {
                int bias = LibgpiodV1.gpiod_line_bias(pinHandle.Handle);
                if (bias == (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN)
                {
                    pinHandle.PinMode = PinMode.InputPullDown;
                }

                if (bias == (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP)
                {
                    pinHandle.PinMode = PinMode.InputPullUp;
                }
            }

            _pinNumberToSafeLineHandle.TryAdd(pinNumber, pinHandle);
            // This is setting up a default value without reading the driver as it's the default behavior.
            // If the Setmode with an initial value is used, this is going to be corrected automatically
            _pinValue.TryAdd(pinNumber, PinValue.Low);
        }
    }

    /// <inheritdoc/>
    protected internal override PinValue Read(int pinNumber)
    {
        if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle))
        {
            int result = LibgpiodV1.gpiod_line_get_value(pinHandle.Handle);
            if (result == -1)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.ReadPinError, Marshal.GetLastWin32Error(), pinNumber);
            }

            _pinValue[pinNumber] = result;
            return result;
        }

        throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber) => Write(pinNumber, !_pinValue[pinNumber]);

    /// <inheritdoc/>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        if (_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodV1DriverEventHandler? eventHandler))
        {
            eventHandler.ValueFalling -= callback;
            eventHandler.ValueRising -= callback;
            if (eventHandler.IsCallbackListEmpty())
            {
                _pinNumberToEventHandler.TryRemove(pinNumber, out eventHandler);
                eventHandler?.Dispose();
            }
        }
        else
        {
            throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.NotListeningForEventError);
        }
    }

    /// <inheritdoc/>
    protected internal override void SetPinMode(int pinNumber, PinMode mode)
    {
        if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle))
        {
            // This call does not release the handle. It only releases the lock on the handle. Without this, changing the direction of a line is not possible.
            // Line handles cannot be freed and are cached until the chip is closed.
            pinHandle.ReleaseLock();
            int requestResult = mode switch
            {
                PinMode.Input => LibgpiodV1.gpiod_line_request_input(pinHandle.Handle, s_consumerName),
                PinMode.InputPullDown => LibgpiodV1.gpiod_line_request_input_flags(pinHandle.Handle, s_consumerName,
                        (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN),
                PinMode.InputPullUp => LibgpiodV1.gpiod_line_request_input_flags(pinHandle.Handle, s_consumerName,
                        (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP),
                PinMode.Output => LibgpiodV1.gpiod_line_request_output(pinHandle.Handle, s_consumerName, 0),
                _ => -1,
            };

            if (requestResult == -1)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.SetPinModeError, Marshal.GetLastWin32Error(),
                    pinNumber);
            }

            pinHandle.PinMode = mode;
            return;
        }

        throw new InvalidOperationException($"Pin {pinNumber} is not open");
    }

    /// <inheritdoc />
    protected internal override void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue)
    {
        if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle))
        {
            // This call does not release the handle. It only releases the lock on the handle. Without this, changing the direction of a line is not possible.
            // Line handles cannot be freed and are cached until the chip is closed.
            pinHandle.ReleaseLock();
            int requestResult = mode switch
            {
                PinMode.Input => LibgpiodV1.gpiod_line_request_input(pinHandle.Handle, s_consumerName),
                PinMode.InputPullDown => LibgpiodV1.gpiod_line_request_input_flags(pinHandle.Handle, s_consumerName,
                    (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN),
                PinMode.InputPullUp => LibgpiodV1.gpiod_line_request_input_flags(pinHandle.Handle, s_consumerName,
                    (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP),
                PinMode.Output => LibgpiodV1.gpiod_line_request_output(pinHandle.Handle, s_consumerName, initialValue == PinValue.High ? 1 : 0),
                _ => -1,
            };

            if (requestResult == -1)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.SetPinModeError, Marshal.GetLastWin32Error(),
                    pinNumber);
            }

            _pinValue[pinNumber] = initialValue;
            pinHandle.PinMode = mode;
            return;
        }

        throw new InvalidOperationException($"Pin {pinNumber} is not open");
    }

    /// <inheritdoc/>
    protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        if ((eventTypes & PinEventTypes.Rising) != 0 || (eventTypes & PinEventTypes.Falling) != 0)
        {
            LibGpiodV1DriverEventHandler eventHandler = _pinNumberToEventHandler.GetOrAdd(pinNumber, PopulateEventHandler);

            if ((eventTypes & PinEventTypes.Rising) != 0)
            {
                eventHandler.ValueRising += Callback;
            }

            if ((eventTypes & PinEventTypes.Falling) != 0)
            {
                eventHandler.ValueFalling += Callback;
            }

            bool eventOccurred = false;
            PinEventTypes typeOfEventOccured = PinEventTypes.None;
            void Callback(object o, PinValueChangedEventArgs e)
            {
                eventOccurred = true;
                typeOfEventOccured = e.ChangeType;
            }

            WaitForEventResult(cancellationToken, eventHandler.CancellationToken, ref eventOccurred);
            RemoveCallbackForPinValueChangedEvent(pinNumber, Callback);

            return new WaitForEventResult
            {
                TimedOut = !eventOccurred,
                EventTypes = eventOccurred ? typeOfEventOccured : PinEventTypes.None,
            };
        }
        else
        {
            throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
        }
    }

    private void WaitForEventResult(CancellationToken sourceToken, CancellationToken parentToken, ref bool eventOccurred)
    {
        while (!(sourceToken.IsCancellationRequested || parentToken.IsCancellationRequested || eventOccurred))
        {
            Thread.Sleep(1);
        }
    }

    /// <inheritdoc/>
    protected internal override void Write(int pinNumber, PinValue value)
    {
        if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out LineHandle? pinHandle))
        {
            throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError,
                pin: pinNumber);
        }

        LibgpiodV1.gpiod_line_set_value(pinHandle.Handle, (value == PinValue.High) ? 1 : 0);
        _pinValue[pinNumber] = value;
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        var self = new ComponentInformation(this, "LibGpiodDriver");
        IntPtr libgpiodVersionPtr = LibgpiodV1.gpiod_version_string();
        string libgpiodVersion = Marshal.PtrToStringAnsi(libgpiodVersionPtr) ?? string.Empty;
        self.Properties["LibGpiodVersion"] = libgpiodVersion;
        return self;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_pinNumberToEventHandler != null)
        {
            foreach (KeyValuePair<int, LibGpiodV1DriverEventHandler> kv in _pinNumberToEventHandler)
            {
                LibGpiodV1DriverEventHandler eventHandler = kv.Value;
                eventHandler.Dispose();
            }

            _pinNumberToEventHandler.Clear();
        }

        if (_pinNumberToSafeLineHandle != null)
        {
            foreach (int pin in _pinNumberToSafeLineHandle.Keys)
            {
                if (_pinNumberToSafeLineHandle.TryGetValue(pin, out LineHandle? pinHandle))
                {
                    pinHandle?.Dispose();
                }
            }

            _pinNumberToSafeLineHandle.Clear();
            _pinValue.Clear();
        }

        _chip?.Dispose();
        _chip = null!;

        base.Dispose(disposing);
    }
}
