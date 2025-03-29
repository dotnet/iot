// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static Interop;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// A GPIO driver for Unix.
/// </summary>
public class SysFsDriver : UnixDriver
{
    private const int ERROR_CODE_EINTR = 4; // Interrupted system call

    private const string GpioBasePath = "/sys/class/gpio";
    private const string GpioChip = "gpiochip";
    private const string GpioLabel = "/label";
    private const string GpioContoller = "pinctrl";
    private const string GpioOffsetBase = "/base";
    private const string GpioCount = "/ngpio";
    private const int PollingTimeout = 50;

    private readonly List<int> _exportedPins = new List<int>();
    private readonly Dictionary<int, UnixDriverDevicePin> _devicePins = new Dictionary<int, UnixDriverDevicePin>();
    private readonly Dictionary<int, PinValue> _pinValues = new Dictionary<int, PinValue>();
    private readonly int _pinOffset;
    private readonly int _chipNumber;

    private TimeSpan _statusUpdateSleepTime = TimeSpan.FromMilliseconds(1);
    private int _pollFileDescriptor = -1;
    private Thread? _eventDetectionThread;
    private int _pinsToDetectEventsCount;
    private CancellationTokenSource? _eventThreadCancellationTokenSource;

    private bool _isDisposed;

    private static int ReadOffset(int chip)
    {
        IEnumerable<string> fileNames = Directory.EnumerateFileSystemEntries(GpioBasePath);
        foreach (string name in fileNames)
        {
            if (name.Contains(GpioChip + chip.ToString(CultureInfo.InvariantCulture)) ||
                (chip == 0 && name.Contains(GpioChip))) // If the chip is specified as 0, take the first entry (legacy behavior)
            {
                try
                {
                    if (File.ReadAllText($"{name}{GpioLabel}").StartsWith(GpioContoller, StringComparison.Ordinal))
                    {
                        if (int.TryParse(File.ReadAllText($"{name}{GpioOffsetBase}"), out int pinOffset))
                        {
                            return pinOffset;
                        }
                    }
                }
                catch (IOException)
                {
                    // Ignoring file not found or any other IO exceptions as it is not guaranteed the folder would have files "label" "base"
                    // And don't want to throw in this case just continue to load the gpiochip with default offset = 0
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SysFsDriver"/> class.
    /// </summary>
    public SysFsDriver()
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Linux/Unix.");
        }

        _isDisposed = false;
        _chipNumber = 0;
        _pinOffset = ReadOffset(0);
    }

    /// <summary>
    /// Creates a SysFsDriver instance for the provided chip number
    /// </summary>
    /// <param name="chip">The chip to select (use <see cref="GetAvailableChips"/> to query the list of available values)</param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public SysFsDriver(GpioChipInfo chip)
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Linux/Unix.");
        }

        _isDisposed = false;
        _chipNumber = chip.Id;
        _pinOffset = ReadOffset(chip.Id);
    }

    /// <summary>
    /// The sleep time after an event occured and before the new value is read.
    /// </summary>
    internal TimeSpan StatusUpdateSleepTime
    {
        get
        {
            return _statusUpdateSleepTime;
        }
        set
        {
            _statusUpdateSleepTime = value;
        }
    }

    /// <summary>
    /// Returns the list of available chips.
    /// This can be used to determine the correct gpio chip for constructor calls to <see cref="LibGpiodDriver"/>
    /// </summary>
    /// <returns>A list of chips detected</returns>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public static IList<GpioChipInfo> GetAvailableChips()
    {
        string[] fileNames = Directory.GetFileSystemEntries(GpioBasePath, $"{GpioChip}*", SearchOption.TopDirectoryOnly);
        List<GpioChipInfo> list = new List<GpioChipInfo>();

        if (fileNames.Length > 0)
        {
            // Add the default entry (the first entry, but we name it "0")
            // There's no such actual entry on RPI4, but RPI3 indeed has a file /sys/class/gpio/gpiochip0, in which
            // case that one is the right one to use
            string nullFile = Path.Combine(GpioBasePath, "gpiochip0");
            GpioChipInfo temp;
            if (Directory.Exists(nullFile))
            {
                temp = GetChipInfoForName(nullFile);
            }
            else
            {
                temp = GetChipInfoForName(fileNames.First());
            }

            list.Add(temp with
            {
                Id = 0
            });
        }

        foreach (string name in fileNames)
        {
            if (name.Contains(GpioChip))
            {
                try
                {
                    GpioChipInfo entry = GetChipInfoForName(name);
                    list.Add(entry);
                }
                catch (IOException)
                {
                    // Ignoring file not found or any other IO exceptions as it is not guaranteed the folder would have files "label" "base"
                    // And don't want to throw in this case just continue to load the gpiochip with default offset = 0
                }
            }
        }

        return list;
    }

    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    private static GpioChipInfo GetChipInfoForName(string name)
    {
        int idx = name.IndexOf(GpioChip, StringComparison.Ordinal);
        var idString = name.Substring(idx + GpioChip.Length);
        int id = 0;
        if (!Int32.TryParse(idString, out id))
        {
            throw new InvalidOperationException($"Unable to parse {idString} as number (path is {name})");
        }

        var label = File.ReadAllText($"{name}{GpioLabel}").Trim();
        var numPins = File.ReadAllText($"{name}{GpioCount}");
        if (!int.TryParse(numPins, out int pins))
        {
            pins = 0;
        }

        var entry = new GpioChipInfo(id, name, label, pins);
        return entry;
    }

    /// <summary>
    /// The number of pins provided by the driver.
    /// </summary>
    protected internal override int PinCount => throw new PlatformNotSupportedException("This driver is generic so it can not enumerate how many pins are available.");

    /// <summary>
    /// Converts a board pin number to the driver's logical numbering scheme.
    /// </summary>
    /// <param name="pinNumber">The board pin number to convert.</param>
    /// <returns>The pin number in the driver's logical numbering scheme.</returns>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new PlatformNotSupportedException("This driver is generic so it can not perform conversions between pin numbering schemes.");

    /// <summary>
    /// Opens a pin in order for it to be ready to use.
    /// This retains the pin direction, but if it is output, the value will always be low after open.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    protected internal override void OpenPin(int pinNumber)
    {
        CheckValidDriver();
        int pinOffset = pinNumber + _pinOffset;
        string pinPath = $"{GpioBasePath}/gpio{pinOffset}";
        // If the directory exists, this becomes a no-op since the pin might have been opened already by some controller or somebody else.
        if (!Directory.Exists(pinPath))
        {
            try
            {
                File.WriteAllText(Path.Combine(GpioBasePath, "export"), pinOffset.ToString(CultureInfo.InvariantCulture));
                SysFsHelpers.EnsureReadWriteAccessToPath(pinPath);

                _exportedPins.Add(pinNumber);
                // Default value is low, otherwise it's the set pin mode with default value that will override this
                _pinValues.Add(pinNumber, PinValue.Low);
            }
            catch (UnauthorizedAccessException e)
            {
                // Wrapping the exception in order to get a better message.
                throw new UnauthorizedAccessException("Opening pins requires root permissions.", e);
            }
        }
    }

    /// <summary>
    /// Closes an open pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    protected internal override void ClosePin(int pinNumber)
    {
        CheckValidDriver();
        int pinOffset = pinNumber + _pinOffset;
        string pinPath = $"{GpioBasePath}/gpio{pinOffset}";
        // If the directory doesn't exist, this becomes a no-op since the pin was closed already.
        if (Directory.Exists(pinPath))
        {
            try
            {
                SetPinEventsToDetect(pinNumber, PinEventTypes.None);
                if (_devicePins.ContainsKey(pinNumber))
                {
                    _devicePins[pinNumber].Dispose();
                    _devicePins.Remove(pinNumber);
                    _pinValues.Remove(pinNumber);
                }

                // If this controller wasn't the one that opened the pin, then Remove will return false, so we don't need to close it.
                if (_exportedPins.Remove(pinNumber))
                {
                    File.WriteAllText(Path.Combine(GpioBasePath, "unexport"), pinOffset.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException("Closing pins requires root permissions.", e);
            }
        }
    }

    /// <summary>
    /// Sets the mode to a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="mode">The mode to be set.</param>
    protected internal override void SetPinMode(int pinNumber, PinMode mode)
    {
        CheckValidDriver();
        if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
        {
            throw new PlatformNotSupportedException("This driver is generic so it does not support Input Pull Down or Input Pull Up modes.");
        }

        string directionPath = $"{GpioBasePath}/gpio{pinNumber + _pinOffset}/direction";
        string sysFsMode = ConvertPinModeToSysFsMode(mode);
        if (File.Exists(directionPath))
        {
            try
            {
                File.WriteAllText(directionPath, sysFsMode);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException("Setting a mode to a pin requires root permissions.", e);
            }
        }
        else
        {
            throw new InvalidOperationException("There was an attempt to set a mode to a pin that is not open.");
        }
    }

    private string ConvertPinModeToSysFsMode(PinMode mode)
    {
        if (mode == PinMode.Input)
        {
            return "in";
        }

        if (mode == PinMode.Output)
        {
            return "out";
        }

        throw new PlatformNotSupportedException($"{mode} is not supported by this driver.");
    }

    private PinMode ConvertSysFsModeToPinMode(string sysFsMode)
    {
        sysFsMode = sysFsMode.Trim();
        if (sysFsMode == "in")
        {
            return PinMode.Input;
        }

        if (sysFsMode == "out")
        {
            return PinMode.Output;
        }

        throw new ArgumentException($"Unable to parse {sysFsMode} as a PinMode.");
    }

    /// <summary>
    /// Reads the current value of a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <returns>The value of the pin.</returns>
    protected internal override PinValue Read(int pinNumber)
    {
        CheckValidDriver();
        PinValue result = default;
        string valuePath = $"{GpioBasePath}/gpio{pinNumber + _pinOffset}/value";
        if (File.Exists(valuePath))
        {
            try
            {
                string valueContents = File.ReadAllText(valuePath);
                result = ConvertSysFsValueToPinValue(valueContents);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
            }
        }
        else
        {
            throw new InvalidOperationException("There was an attempt to read from a pin that is not open.");
        }

        _pinValues[pinNumber] = result;
        return result;
    }

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber) => Write(pinNumber, !_pinValues[pinNumber]);

    private PinValue ConvertSysFsValueToPinValue(string value)
    {
        return value.Trim() switch
        {
            "0" => PinValue.Low,
            "1" => PinValue.High,
            _ => throw new ArgumentException($"Invalid GPIO pin value {value}.")
        };
    }

    /// <summary>
    /// Writes a value to a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="value">The value to be written to the pin.</param>
    protected internal override void Write(int pinNumber, PinValue value)
    {
        CheckValidDriver();
        string valuePath = $"{GpioBasePath}/gpio{pinNumber + _pinOffset}/value";
        if (File.Exists(valuePath))
        {
            try
            {
                string sysFsValue = ConvertPinValueToSysFs(value);
                File.WriteAllText(valuePath, sysFsValue);
                _pinValues[pinNumber] = value;
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
            }
        }
        else
        {
            throw new InvalidOperationException("There was an attempt to write to a pin that is not open.");
        }
    }

    private string ConvertPinValueToSysFs(PinValue value)
        => value == PinValue.High ? "1" : "0";

    /// <summary>
    /// Checks if a pin supports a specific mode.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="mode">The mode to check.</param>
    /// <returns>The status if the pin supports the mode.</returns>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        CheckValidDriver();
        // Unix driver does not support pull up or pull down resistors.
        if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Blocks execution until an event of type eventType is received or a cancellation is requested.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for. Can be <see cref="PinEventTypes.Rising"/>, <see cref="PinEventTypes.Falling"/> or both.</param>
    /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
    /// <returns>A structure that contains the result of the waiting operation.</returns>
    protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        CheckValidDriver();
        int pollFileDescriptor = -1;
        int valueFileDescriptor = -1;
        SetPinEventsToDetect(pinNumber, eventTypes);
        AddPinToPoll(pinNumber, ref valueFileDescriptor, ref pollFileDescriptor, out bool closePinValueFileDescriptor);

        bool eventDetected = WasEventDetected(pollFileDescriptor, valueFileDescriptor, out _, cancellationToken);
        if (_statusUpdateSleepTime > TimeSpan.Zero)
        {
            Thread.Sleep(_statusUpdateSleepTime); // Adding some delay to make sure that the value of the File has been updated so that we will get the right event type.
        }

        PinEventTypes detectedEventType = PinEventTypes.None;
        if (eventDetected)
        {
            // This is the only case where we need to read the new state. Although there are reports of this not being 100% reliable in all situations,
            // it seems to be working fine most of the time.
            if (eventTypes == (PinEventTypes.Rising | PinEventTypes.Falling))
            {
                detectedEventType = (Read(pinNumber) == PinValue.High) ? PinEventTypes.Rising : PinEventTypes.Falling;
            }
            else if (eventTypes != PinEventTypes.None)
            {
                // If we're only waiting for one event type, we know which one it has to be
                detectedEventType = eventTypes;
            }
        }

        RemovePinFromPoll(pinNumber, ref valueFileDescriptor, ref pollFileDescriptor, closePinValueFileDescriptor, closePollFileDescriptor: true, cancelEventDetectionThread: false);
        return new WaitForEventResult
        {
            TimedOut = !eventDetected,
            EventTypes = detectedEventType,
        };
    }

    private void SetPinEventsToDetect(int pinNumber, PinEventTypes eventTypes)
    {
        string edgePath = Path.Combine(GpioBasePath, $"gpio{pinNumber + _pinOffset}", "edge");
        // Even though the pin is open, we might sometimes need to wait for access
        SysFsHelpers.EnsureReadWriteAccessToPath(edgePath);
        string stringValue = PinEventTypeToStringValue(eventTypes);
        File.WriteAllText(edgePath, stringValue);
    }

    private PinEventTypes GetPinEventsToDetect(int pinNumber)
    {
        string edgePath = Path.Combine(GpioBasePath, $"gpio{pinNumber + _pinOffset}", "edge");
        // Even though the pin is open, we might sometimes need to wait for access
        SysFsHelpers.EnsureReadWriteAccessToPath(edgePath);
        string stringValue = File.ReadAllText(edgePath);
        return StringValueToPinEventType(stringValue);
    }

    private PinEventTypes StringValueToPinEventType(string value)
    {
        return value.Trim() switch
        {
            "none" => PinEventTypes.None,
            "both" => PinEventTypes.Falling | PinEventTypes.Rising,
            "rising" => PinEventTypes.Rising,
            "falling" => PinEventTypes.Falling,
            _ => throw new ArgumentException("Invalid pin event value.", value)
        };
    }

    private string PinEventTypeToStringValue(PinEventTypes kind)
    {
        if (kind == PinEventTypes.None)
        {
            return "none";
        }

        if ((kind & PinEventTypes.Falling) != 0 && (kind & PinEventTypes.Rising) != 0)
        {
            return "both";
        }

        if (kind == PinEventTypes.Rising)
        {
            return "rising";
        }

        if (kind == PinEventTypes.Falling)
        {
            return "falling";
        }

        throw new ArgumentException("Invalid Pin Event Type.", nameof(kind));
    }

    private void AddPinToPoll(int pinNumber, ref int valueFileDescriptor, ref int pollFileDescriptor, out bool closePinValueFileDescriptor)
    {
        if (pollFileDescriptor == -1)
        {
            pollFileDescriptor = Interop.epoll_create(1);
            if (pollFileDescriptor < 0)
            {
                throw new IOException("Error while trying to initialize pin interrupts (epoll_create failed).");
            }
        }

        closePinValueFileDescriptor = false;

        if (valueFileDescriptor == -1)
        {
            string valuePath = Path.Combine(GpioBasePath, $"gpio{pinNumber + _pinOffset}", "value");
            valueFileDescriptor = Interop.open(valuePath, FileOpenFlags.O_RDONLY | FileOpenFlags.O_NONBLOCK);
            if (valueFileDescriptor < 0)
            {
                throw new IOException($"Error while trying to open pin value file {valuePath}.");
            }

            closePinValueFileDescriptor = true;
        }

        epoll_event epollEvent = new epoll_event
        {
            events = PollEvents.EPOLLIN | PollEvents.EPOLLET | PollEvents.EPOLLPRI,
            data = new epoll_data()
            {
                pinNumber = pinNumber
            }
        };

        int result = Interop.epoll_ctl(pollFileDescriptor, PollOperations.EPOLL_CTL_ADD, valueFileDescriptor, ref epollEvent);
        if (result == -1)
        {
            throw new IOException("Error while trying to initialize pin interrupts (epoll_ctl failed).");
        }

        // Ignore first time because it will always return the current state.
        using var eventBuffer = new UnmanagedArray<epoll_event>(1);
        while (Interop.epoll_wait(pollFileDescriptor, eventBuffer, 1, 0) == -1)
        {
            var errorCode = Marshal.GetLastWin32Error();
            if (errorCode != ERROR_CODE_EINTR)
            {
                // don't retry on unknown error
                break;
            }
        }
    }

    private bool WasEventDetected(int pollFileDescriptor, int valueFileDescriptor, out int pinNumber, CancellationToken cancellationToken)
    {
        pinNumber = -1;

        using var eventBuffer = new UnmanagedArray<epoll_event>(1);

        while (!cancellationToken.IsCancellationRequested)
        {
            // Wait until something happens
            int waitResult = Interop.epoll_wait(pollFileDescriptor, eventBuffer, 1, PollingTimeout);
            if (waitResult == -1)
            {
                var errorCode = Marshal.GetLastWin32Error();
                if (errorCode == ERROR_CODE_EINTR)
                {
                    // ignore Interrupted system call error and retry
                    continue;
                }

                throw new IOException($"Error while waiting for pin interrupts. (ErrorCode={errorCode})");
            }

            if (waitResult > 0)
            {
                var @event = eventBuffer.ReadToManagedArray()[0];
                pinNumber = @event.data.pinNumber;
                return true;
            }
        }

        return false;
    }

    private void RemovePinFromPoll(int pinNumber, ref int valueFileDescriptor, ref int pollFileDescriptor, bool closePinValueFileDescriptor, bool closePollFileDescriptor, bool cancelEventDetectionThread)
    {
        epoll_event epollEvent = new epoll_event
        {
            events = PollEvents.EPOLLIN | PollEvents.EPOLLET | PollEvents.EPOLLPRI
        };

        int result = Interop.epoll_ctl(pollFileDescriptor, PollOperations.EPOLL_CTL_DEL, valueFileDescriptor, ref epollEvent);
        if (result == -1)
        {
            throw new IOException("Error while trying to delete pin interrupts.");
        }

        if (closePinValueFileDescriptor)
        {
            Interop.close(valueFileDescriptor);
            valueFileDescriptor = -1;
        }

        if (closePollFileDescriptor)
        {
            if (cancelEventDetectionThread)
            {
                try
                {
                    if (_eventThreadCancellationTokenSource != null)
                    {
                        _eventThreadCancellationTokenSource.Cancel();
                        _eventThreadCancellationTokenSource.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                }

                while (_eventDetectionThread != null && _eventDetectionThread.IsAlive)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10)); // Wait until the event detection thread is aborted.
                }
            }

            Interop.close(pollFileDescriptor);
            pollFileDescriptor = -1;
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _pinsToDetectEventsCount = 0;
        if (_eventDetectionThread != null && _eventDetectionThread.IsAlive)
        {
            try
            {
                if (_eventThreadCancellationTokenSource != null)
                {
                    _eventThreadCancellationTokenSource.Cancel();
                    _eventThreadCancellationTokenSource.Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
                // The Cancellation Token source may already be disposed.
            }

            while (_eventDetectionThread != null && _eventDetectionThread.IsAlive)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10)); // Wait until the event detection thread is aborted.
            }
        }

        foreach (UnixDriverDevicePin devicePin in _devicePins.Values)
        {
            devicePin.Dispose();
        }

        _devicePins.Clear();
        _pinValues.Clear();
        if (_pollFileDescriptor != -1)
        {
            Interop.close(_pollFileDescriptor);
            _pollFileDescriptor = -1;
        }

        while (_exportedPins.Count > 0)
        {
            ClosePin(_exportedPins.FirstOrDefault());
        }

        _isDisposed = true;
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public override GpioChipInfo GetChipInfo()
    {
        return GetAvailableChips().First(x => x.Id == _chipNumber);
    }

    private void CheckValidDriver()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(SysFsDriver));
        }
    }

    /// <summary>
    /// Adds a handler for a pin value changed event.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="eventTypes">The event types to wait for.</param>
    /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
    protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        CheckValidDriver();
        if (!_devicePins.ContainsKey(pinNumber))
        {
            _devicePins.Add(pinNumber, new UnixDriverDevicePin(Read(pinNumber)));
            _pinsToDetectEventsCount++;
            AddPinToPoll(pinNumber, ref _devicePins[pinNumber].FileDescriptor, ref _pollFileDescriptor, out _);
        }

        if ((eventTypes & PinEventTypes.Rising) != 0)
        {
            _devicePins[pinNumber].ValueRising += callback;
        }

        if ((eventTypes & PinEventTypes.Falling) != 0)
        {
            _devicePins[pinNumber].ValueFalling += callback;
        }

        PinEventTypes events = (GetPinEventsToDetect(pinNumber) | eventTypes);
        SetPinEventsToDetect(pinNumber, events);

        // Remember which events are active
        _devicePins[pinNumber].ActiveEdges = events;
        InitializeEventDetectionThread();
    }

    private void InitializeEventDetectionThread()
    {
        if (_eventDetectionThread == null)
        {
            _eventDetectionThread = new Thread(DetectEvents)
            {
                IsBackground = true
            };
            _eventThreadCancellationTokenSource = new CancellationTokenSource();
            _eventDetectionThread.Start();
        }
    }

    private void DetectEvents()
    {
        if (_eventThreadCancellationTokenSource == null)
        {
            throw new InvalidOperationException("Cannot start to detect events when CancellationTokenSource is null.");
        }

        while (_pinsToDetectEventsCount > 0)
        {
            try
            {
                bool eventDetected = WasEventDetected(_pollFileDescriptor, -1, out int pinNumber, _eventThreadCancellationTokenSource.Token);
                if (eventDetected)
                {
                    if (_statusUpdateSleepTime > TimeSpan.Zero)
                    {
                        Thread.Sleep(_statusUpdateSleepTime); // Adding some delay to make sure that the value of the File has been updated so that we will get the right event type.
                    }

                    PinValue newValue = Read(pinNumber);

                    UnixDriverDevicePin currentPin = _devicePins[pinNumber];
                    PinEventTypes activeEdges = currentPin.ActiveEdges;
                    PinEventTypes eventType = activeEdges;
                    PinEventTypes secondEvent = PinEventTypes.None;
                    // Only if the active edges are both, we need to query the current state and guess about the change
                    if (activeEdges == (PinEventTypes.Falling | PinEventTypes.Rising))
                    {
                        PinValue oldValue = currentPin.LastValue;
                        if (oldValue == PinValue.Low && newValue == PinValue.High)
                        {
                            eventType = PinEventTypes.Rising;
                        }
                        else if (oldValue == PinValue.High && newValue == PinValue.Low)
                        {
                            eventType = PinEventTypes.Falling;
                        }
                        else if (oldValue == PinValue.High)
                        {
                            // Both high -> There must have been a low-active peak
                            eventType = PinEventTypes.Falling;
                            secondEvent = PinEventTypes.Rising;
                        }
                        else
                        {
                            // Both low -> There must have been a high-active peak
                            eventType = PinEventTypes.Rising;
                            secondEvent = PinEventTypes.Falling;
                        }

                        currentPin.LastValue = newValue;
                    }
                    else
                    {
                        // Update the value, in case we need it later
                        currentPin.LastValue = newValue;
                    }

                    PinValueChangedEventArgs args = new PinValueChangedEventArgs(eventType, pinNumber);
                    currentPin.OnPinValueChanged(args);
                    if (secondEvent != PinEventTypes.None)
                    {
                        args = new PinValueChangedEventArgs(secondEvent, pinNumber);
                        currentPin.OnPinValueChanged(args);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                break; // If cancellation token source is disposed then we need to exit this thread.
            }
        }

        _eventDetectionThread = null;
    }

    /// <summary>
    /// Removes a handler for a pin value changed event.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        CheckValidDriver();
        if (!_devicePins.ContainsKey(pinNumber))
        {
            throw new InvalidOperationException("Attempted to remove a callback for a pin that is not listening for events.");
        }

        _devicePins[pinNumber].ValueFalling -= callback;
        _devicePins[pinNumber].ValueRising -= callback;
        if (_devicePins[pinNumber].IsCallbackListEmpty())
        {
            _pinsToDetectEventsCount--;

            bool closePollFileDescriptor = (_pinsToDetectEventsCount == 0);
            RemovePinFromPoll(pinNumber, ref _devicePins[pinNumber].FileDescriptor, ref _pollFileDescriptor, true, closePollFileDescriptor, true);
            _devicePins[pinNumber].Dispose();
            _devicePins.Remove(pinNumber);
        }
    }

    /// <summary>
    /// Gets the mode of a pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <returns>The mode of the pin.</returns>
    protected internal override PinMode GetPinMode(int pinNumber)
    {
        CheckValidDriver();
        pinNumber += _pinOffset;
        string directionPath = $"{GpioBasePath}/gpio{pinNumber}/direction";
        if (File.Exists(directionPath))
        {
            try
            {
                string sysFsMode = File.ReadAllText(directionPath);
                return ConvertSysFsModeToPinMode(sysFsMode);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException("Getting a mode to a pin requires root permissions.", e);
            }
        }
        else
        {
            throw new InvalidOperationException("There was an attempt to get a mode to a pin that is not open.");
        }
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        var self = new ComponentInformation(this, nameof(SysFsDriver));
#pragma warning disable SDGPIO0001
        self.Properties["ChipInfo"] = GetChipInfo().ToString();
#pragma warning restore SDGPIO0001
        return self;
    }
}
