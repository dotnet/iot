// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Unix.
    /// </summary>
    public class SysFsDriver : UnixDriver
    {
        private const string GpioBasePath = "/sys/class/gpio";
        private const string GpioChip = "gpiochip";
        private const string GpioLabel = "/label";
        private const string GpioContoller = "pinctrl";
        private const string GpioOffsetBase = "/base";
        private const int PollingTimeout = 50;
        private readonly CancellationTokenSource _eventThreadCancellationTokenSource;
        private readonly List<int> _exportedPins = new List<int>();
        private readonly Dictionary<int, UnixDriverDevicePin> _devicePins = new Dictionary<int, UnixDriverDevicePin>();
        private static readonly int s_pinOffset = ReadOffset();
        private TimeSpan _statusUpdateSleepTime = TimeSpan.FromMilliseconds(1);
        private int _pollFileDescriptor = -1;
        private Thread _eventDetectionThread;
        private int _pinsToDetectEventsCount;

        private static int ReadOffset()
        {
            IEnumerable<string> fileNames = Directory.EnumerateFileSystemEntries(GpioBasePath);
            foreach (string name in fileNames)
            {
                if (name.Contains(GpioChip))
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
        [Obsolete]
        public SysFsDriver()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Linux/Unix.");
            }

            _eventThreadCancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SysFsDriver"/> class.
        /// </summary>
        public SysFsDriver(Board board)
            : base(board)
        {
            _eventThreadCancellationTokenSource = new CancellationTokenSource();
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
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void OpenPin(int pinNumber)
        {
            int pinOffset = pinNumber + s_pinOffset;
            string pinPath = $"{GpioBasePath}/gpio{pinOffset}";
            // If the directory exists, this becomes a no-op since the pin might have been opened already by the some controller or somebody else.
            if (!Directory.Exists(pinPath))
            {
                try
                {
                    File.WriteAllText(Path.Combine(GpioBasePath, "export"), pinOffset.ToString(CultureInfo.InvariantCulture));
                    SysFsHelpers.EnsureReadWriteAccessToPath(pinPath);

                    _exportedPins.Add(pinNumber);
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
            int pinOffset = pinNumber + s_pinOffset;
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
                    }

                    File.WriteAllText(Path.Combine(GpioBasePath, "unexport"), pinOffset.ToString(CultureInfo.InvariantCulture));
                    _exportedPins.Remove(pinNumber);
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
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
            {
                throw new PlatformNotSupportedException("This driver is generic so it does not support Input Pull Down or Input Pull Up modes.");
            }

            string directionPath = $"{GpioBasePath}/gpio{pinNumber + s_pinOffset}/direction";
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
            PinValue result = default;
            string valuePath = $"{GpioBasePath}/gpio{pinNumber + s_pinOffset}/value";
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

            return result;
        }

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
            string valuePath = $"{GpioBasePath}/gpio{pinNumber + s_pinOffset}/value";
            if (File.Exists(valuePath))
            {
                try
                {
                    string sysFsValue = ConvertPinValueToSysFs(value);
                    File.WriteAllText(valuePath, sysFsValue);
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
            string edgePath = Path.Combine(GpioBasePath, $"gpio{pinNumber + s_pinOffset}", "edge");
            // Even though the pin is open, we might sometimes need to wait for access
            SysFsHelpers.EnsureReadWriteAccessToPath(edgePath);
            string stringValue = PinEventTypeToStringValue(eventTypes);
            File.WriteAllText(edgePath, stringValue);
        }

        private PinEventTypes GetPinEventsToDetect(int pinNumber)
        {
            string edgePath = Path.Combine(GpioBasePath, $"gpio{pinNumber + s_pinOffset}", "edge");
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
                string valuePath = Path.Combine(GpioBasePath, $"gpio{pinNumber + s_pinOffset}", "value");
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
            Interop.epoll_wait(pollFileDescriptor, out _, 1, 0);
        }

        private unsafe bool WasEventDetected(int pollFileDescriptor, int valueFileDescriptor, out int pinNumber, CancellationToken cancellationToken)
        {
            char buf;
            IntPtr bufPtr = new IntPtr(&buf);
            pinNumber = -1;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait until something happens
                int waitResult = Interop.epoll_wait(pollFileDescriptor, out epoll_event events, 1, PollingTimeout);
                if (waitResult == -1)
                {
                    throw new IOException("Error while waiting for pin interrupts.");
                }

                if (waitResult > 0)
                {
                    pinNumber = events.data.pinNumber;

                    // This entire section is probably not necessary, but this seems to be hard to validate.
                    // See https://github.com/dotnet/iot/pull/914#discussion_r389924106 and issue #1024.
                    if (valueFileDescriptor == -1)
                    {
                        // valueFileDescriptor will be -1 when using the callback eventing. For WaitForEvent, the value will be set.
                        valueFileDescriptor = _devicePins[pinNumber].FileDescriptor;
                    }

                    int lseekResult = Interop.lseek(valueFileDescriptor, 0, SeekFlags.SEEK_SET);
                    if (lseekResult == -1)
                    {
                        throw new IOException("Error while trying to seek in value file.");
                    }

                    int readResult = Interop.read(valueFileDescriptor, bufPtr, 1);
                    if (readResult != 1)
                    {
                        throw new IOException("Error while trying to read value file.");
                    }

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
                        _eventThreadCancellationTokenSource.Cancel();
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

        protected override void Dispose(bool disposing)
        {
            _pinsToDetectEventsCount = 0;
            if (_eventDetectionThread != null && _eventDetectionThread.IsAlive)
            {
                try
                {
                    _eventThreadCancellationTokenSource.Cancel();
                    _eventThreadCancellationTokenSource.Dispose();
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
            if (_pollFileDescriptor != -1)
            {
                Interop.close(_pollFileDescriptor);
                _pollFileDescriptor = -1;
            }

            while (_exportedPins.Count > 0)
            {
                ClosePin(_exportedPins.FirstOrDefault());
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
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
                _eventDetectionThread.Start();
            }
        }

        private void DetectEvents()
        {
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
            pinNumber += s_pinOffset;
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
    }
}
