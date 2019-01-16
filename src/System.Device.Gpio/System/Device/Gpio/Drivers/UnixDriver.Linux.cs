// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Unix.
    /// </summary>
    public class UnixDriver : GpioDriver
    {
        private const string GpioBasePath = "/sys/class/gpio";
        private int _pollFileDescriptor = -1;
        private Thread _eventDetectionThread;
        private int _pinsToDetectEventsCount;
        private readonly static CancellationTokenSource s_eventThreadCancellationTokenSource = new CancellationTokenSource();
        private readonly List<int> _exportedPins = new List<int>();
        private readonly Dictionary<int, UnixDriverDevicePin> _devicePins = new Dictionary<int, UnixDriverDevicePin>();
        private readonly int _pollingTimeoutInMilliseconds = Convert.ToInt32(TimeSpan.FromMilliseconds(1).TotalMilliseconds);

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

        private PinMode ConvertSysFsModeToPinMode(string sysfsMode)
        {
            sysfsMode = sysfsMode.Trim();
            if (sysfsMode == "in")
            {
                return PinMode.Input;
            }
            if (sysfsMode == "out")
            {
                return PinMode.Output;
            }
            throw new ArgumentException($"Unable to parse {sysfsMode} as a PinMode.");
        }

        private PinValue ConvertSysFsValueToPinValue(string value)
        {
            PinValue result;
            value = value.Trim();

            switch (value)
            {
                case "0":
                    result = PinValue.Low;
                    break;
                case "1":
                    result = PinValue.High;
                    break;
                default:
                    throw new ArgumentException($"Invalid GPIO pin value {value}.");
            }

            return result;
        }

        private string ConvertPinValueToSysFs(PinValue value)
        {
            string result = string.Empty;
            switch (value)
            {
                case PinValue.High:
                    result = "1";
                    break;
                case PinValue.Low:
                    result = "0";
                    break;
                default:
                    throw new ArgumentException($"Invalid pin value {value}.");
            }
            return result;
        }

        private void SetPinEventsToDetect(int pinNumber, PinEventTypes eventType)
        {
            string edgePath = Path.Combine(GpioBasePath, $"gpio{pinNumber}", "edge");
            string stringValue = PinEventTypeToStringValue(eventType);
            File.WriteAllText(edgePath, stringValue);
        }

        private PinEventTypes GetPinEventsToDetect(int pinNumber)
        {
            string edgePath = Path.Combine(GpioBasePath, $"gpio{pinNumber}", "edge");
            string stringValue = File.ReadAllText(edgePath);
            return StringValueToPinEventType(stringValue);
        }

        private PinEventTypes StringValueToPinEventType(string value)
        {
            value = value.Trim();
            switch (value)
            {
                case "none":
                    return PinEventTypes.None;
                case "both":
                    return PinEventTypes.Falling | PinEventTypes.Rising;
                case "rising":
                    return PinEventTypes.Rising;
                case "falling":
                    return PinEventTypes.Falling;
                default:
                    throw new ArgumentException("Invalid pin event value.", value);
            }
        }

        private string PinEventTypeToStringValue(PinEventTypes kind)
        {
            if (kind == PinEventTypes.None)
            {
                return "none";
            }
            if (kind.HasFlag(PinEventTypes.Falling) && kind.HasFlag(PinEventTypes.Rising))
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
                    throw new IOException("Error while trying to initialize pin interrupts.");
                }
            }

            closePinValueFileDescriptor = false;

            if (valueFileDescriptor == -1)
            {
                string valuePath = Path.Combine(GpioBasePath, $"gpio{pinNumber}", "value");
                valueFileDescriptor = Interop.open(valuePath, FileOpenFlags.O_RDONLY | FileOpenFlags.O_NONBLOCK);
                if (valueFileDescriptor < 0)
                {
                    throw new IOException("Error while trying to initialize pin interrupts.");
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
                throw new IOException("Error while trying to initialize pin interrupts.");
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
                // poll every 1 millisecond
                int waitResult = Interop.epoll_wait(pollFileDescriptor, out epoll_event events, 1, _pollingTimeoutInMilliseconds);
                if (waitResult == -1)
                {
                    throw new IOException("Error while trying to initialize pin interrupts.");
                }
                if (waitResult > 0)
                {
                    pinNumber = events.data.pinNumber;

                    // valueFileDescriptor will be -1 when using the callback eventing. For WaitForEvent, the value will be set.
                    if (valueFileDescriptor == -1)
                    {
                        valueFileDescriptor = _devicePins[pinNumber].FileDescriptor;
                    }

                    int lseekResult = Interop.lseek(valueFileDescriptor, 0, SeekFlags.SEEK_SET);
                    if (lseekResult == -1)
                    {
                        throw new IOException("Error while trying to initialize pin interrupts.");
                    }

                    int readResult = Interop.read(valueFileDescriptor, bufPtr, 1);
                    if (readResult != 1)
                    {
                        throw new IOException("Error while trying to initialize pin interrupts.");
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
                throw new IOException("Error while trying to initialize pin interrupts.");
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
                    s_eventThreadCancellationTokenSource.Cancel();
                    while (_eventDetectionThread != null && _eventDetectionThread.IsAlive)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(10)); // Wait until the event detection thread is aborted.
                    }
                }

                Interop.close(pollFileDescriptor);
                pollFileDescriptor = -1;
            }
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

        private unsafe void DetectEvents()
        {
            while (_pinsToDetectEventsCount > 0)
            {
                bool eventDetected = WasEventDetected(_pollFileDescriptor, -1, out int pinNumber, s_eventThreadCancellationTokenSource.Token);
                if (eventDetected)
                {
                    PinEventTypes eventType = (Read(pinNumber) == PinValue.High) ? PinEventTypes.Rising : PinEventTypes.Falling;
                    var args = new PinValueChangedEventArgs(eventType, pinNumber);
                    _devicePins[pinNumber]?.OnPinValueChanged(args, GetPinEventsToDetect(pinNumber));
                }
            }
            _eventDetectionThread = null;
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
            string pinPath = $"{GpioBasePath}/gpio{pinNumber}";
            // If the directory exists, this becomes a no-op since the pin might have been opened already by the some controller or somebody else.
            if (!Directory.Exists(pinPath))
            {
                try
                {
                    File.WriteAllText(Path.Combine(GpioBasePath, "export"), pinNumber.ToString());
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
            string pinPath = $"{GpioBasePath}/gpio{pinNumber}";
            // If the directory doesn't exist, this becomes a no-op since the pin was closed already.
            if (Directory.Exists(pinPath))
            {
                try
                {
                    SetPinEventsToDetect(pinNumber, PinEventTypes.None);
                    File.WriteAllText(Path.Combine(GpioBasePath, "unexport"), pinNumber.ToString());
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
            string directionPath = $"{GpioBasePath}/gpio{pinNumber}/direction";
            string sysfsMode = ConvertPinModeToSysFsMode(mode);
            if (File.Exists(directionPath))
            {
                try
                {
                    File.WriteAllText(directionPath, sysfsMode);
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

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            string directionPath = $"{GpioBasePath}/gpio{pinNumber}/direction";
            if (File.Exists(directionPath))
            {
                try
                {
                    string sysfsMode = File.ReadAllText(directionPath);
                    return ConvertSysFsModeToPinMode(sysfsMode);
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
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal override PinValue Read(int pinNumber)
        {
            PinValue result = default;
            string valuePath = $"{GpioBasePath}/gpio{pinNumber}/value";
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

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal override void Write(int pinNumber, PinValue value)
        {
            string valuePath = $"{GpioBasePath}/gpio{pinNumber}/value";
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

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventType">The event type to listen for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            int pollFileDescriptor = -1;
            int valueFileDescriptor = -1;
            SetPinEventsToDetect(pinNumber, eventType);
            AddPinToPoll(pinNumber, ref valueFileDescriptor, ref pollFileDescriptor, out bool closePinValueFileDescriptor);

            bool eventDetected = WasEventDetected(pollFileDescriptor, valueFileDescriptor, out _, cancellationToken);

            RemovePinFromPoll(pinNumber, ref valueFileDescriptor, ref pollFileDescriptor, closePinValueFileDescriptor, closePollFileDescriptor: true, cancelEventDetectionThread: false);
            return new WaitForEventResult
            {
                TimedOut = !eventDetected,
                EventType = eventType
            };
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            if (!_devicePins.ContainsKey(pinNumber))
            {
                _devicePins.Add(pinNumber, new UnixDriverDevicePin());
                _pinsToDetectEventsCount++;
                AddPinToPoll(pinNumber, ref _devicePins[pinNumber].FileDescriptor, ref _pollFileDescriptor, out _);
            }
            if (eventType.HasFlag(PinEventTypes.Rising))
            {
                _devicePins[pinNumber].ValueRising += callback;
            }
            if (eventType.HasFlag(PinEventTypes.Falling))
            {
                _devicePins[pinNumber].ValueFalling += callback;
            }
            SetPinEventsToDetect(pinNumber, (GetPinEventsToDetect(pinNumber) | eventType));
            InitializeEventDetectionThread();
        }

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

        protected override void Dispose(bool disposing)
        {
            _pinsToDetectEventsCount = 0;
            if (_eventDetectionThread != null && _eventDetectionThread.IsAlive)
            {
                s_eventThreadCancellationTokenSource.Cancel();
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
    }
}
