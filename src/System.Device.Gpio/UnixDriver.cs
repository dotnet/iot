// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Devices.Gpio
{
    public class UnixDriver : GpioDriver
    {
        #region Interop

        private const string LibraryName = "libc";

        [Flags]
        private enum FileOpenFlags
        {
            O_RDONLY = 0x00,
            O_NONBLOCK = 0x800,
            O_RDWR = 0x02,
            O_SYNC = 0x101000
        }

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, FileOpenFlags flags);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int close(int fd);

        private enum PollOperations
        {
            EPOLL_CTL_ADD = 1,
            EPOLL_CTL_DEL = 2
        }

        private enum PollEvents : uint
        {
            EPOLLIN = 0x01,
            EPOLLET = 0x80000000,
            EPOLLPRI = 0x02
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct epoll_data
        {
            [FieldOffset(0)]
            public IntPtr ptr;

            [FieldOffset(0)]
            public int fd;

            [FieldOffset(0)]
            public uint u32;

            [FieldOffset(0)]
            public ulong u64;

            [FieldOffset(0)]
            public int gpioPinNumber;
        }

        private struct epoll_event
        {
            public PollEvents events;
            public epoll_data data;
        }

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int epoll_create(int size);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int epoll_ctl(int epfd, PollOperations op, int fd, ref epoll_event events);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int epoll_wait(int epfd, out epoll_event events, int maxevents, int timeout);

        private enum SeekFlags
        {
            SEEK_SET = 0
        }

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int lseek(int fd, int offset, SeekFlags whence);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int read(int fd, IntPtr buf, int count);

        #endregion

        private const string GpioPath = "/sys/class/gpio";
        private const string PWMPath = "/sys/class/pwm";

        private readonly int _pinCount;
        private readonly IList<int> _exportedPins;
        private readonly IList<Tuple<int,int>> _exportedPWMChannels;

        private int _pollFileDescriptor = -1;
        private IDictionary<int, int> _pinValueFileDescriptors;

        private int _pinsToDetectEventsCount;
        private readonly IList<int> _pinsToDetectEvents;
        private Thread _eventDetectionThread;
        private readonly IDictionary<int, TimeSpan> _debounceTimeouts;
        private readonly IDictionary<int, DateTime> _lastEvents;

        public UnixDriver()
            : this(-1)
        {
            // Nothing
        }

        public UnixDriver(int pinCount)
        {
            _pinCount = pinCount;
            _exportedPins = new List<int>();
            _exportedPWMChannels = new List<Tuple<int,int>>();
            _pinsToDetectEvents = new List<int>();
            _debounceTimeouts = new Dictionary<int, TimeSpan>();
            _lastEvents = new Dictionary<int, DateTime>();
            _pinValueFileDescriptors = new Dictionary<int, int>();
        }

        public override void Dispose()
        {
            _pinsToDetectEventsCount = 0;

            foreach (int fd in _pinValueFileDescriptors.Values)
            {
                close(fd);
            }

            _pinValueFileDescriptors.Clear();

            if (_pollFileDescriptor != -1)
            {
                close(_pollFileDescriptor);
                _pollFileDescriptor = -1;
            }

            while (_exportedPins.Count > 0)
            {
                int gpioPinNumber = _exportedPins[0];
                UnexportPin(gpioPinNumber);
            }

            while (_exportedPWMChannels.Count > 0)
            {
                UnexportPWMPin(_exportedPWMChannels[0].Item1, _exportedPWMChannels[1].Item2);
            }
        }

        protected internal override int PinCount
        {
            get
            {
                if (_pinCount == -1)
                {
                    throw new NotSupportedException("Unknown pin count");
                }

                return _pinCount;
            }
        }

        protected internal override bool IsPinModeSupported(PinMode mode)
        {
            bool result;

            switch (mode)
            {
                case PinMode.Input:
                case PinMode.Output:
                    result = true;
                    break;

                default:
                    result = false;
                    break;
            }

            return result;
        }

        protected internal override void OpenPin(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);
            ExportPin(gpioPinNumber);
        }

        protected internal override void OpenPWMPin(int chip, int channel)
        {
            ValidatePWMChannel(chip, channel);
            ExportPWMChannel(chip, channel);
        }

        protected internal override void ClosePin(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            SetPinEventsToDetect(gpioPinNumber, PinEvent.None);
            _debounceTimeouts.Remove(gpioPinNumber);
            _lastEvents.Remove(gpioPinNumber);
            UnexportPin(gpioPinNumber);
        }

        protected internal override void ClosePWMPin(int chip, int channel)
        {
            ValidatePWMChannel(chip, channel);
            UnexportPWMPin(chip, channel);
        }

        protected internal override PinMode GetPinMode(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            string directionPath = $"{GpioPath}/gpio{gpioPinNumber}/direction";
            string stringMode = File.ReadAllText(directionPath);
            PinMode mode = StringModeToPinMode(stringMode);
            return mode;
        }

        protected internal override void SetPinMode(int gpioPinNumber, PinMode mode)
        {
            ValidatePinNumber(gpioPinNumber);
            ValidatePinMode(mode);

            string directionPath = $"{GpioPath}/gpio{gpioPinNumber}/direction";
            string stringMode = PinModeToStringMode(mode);
            File.WriteAllText(directionPath, stringMode);
        }

        protected internal override PinValue Input(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            string valuePath = $"{GpioPath}/gpio{gpioPinNumber}/value";
            string stringValue = File.ReadAllText(valuePath);
            PinValue value = StringValueToPinValue(stringValue);
            return value;
        }

        protected internal override void Output(int gpioPinNumber, PinValue value)
        {
            ValidatePinNumber(gpioPinNumber);
            ValidatePinValue(value);

            string valuePath = $"{GpioPath}/gpio{gpioPinNumber}/value";
            string stringValue = PinValueToStringValue(value);
            File.WriteAllText(valuePath, stringValue);
        }

        protected internal override void PWMWrite(int chip, int channel, PWMMode mode, int period, int dutyCycle)
        {
            if (mode == PWMMode.Serial) // Unix driver only supports balanced pwm mode.
            {
                throw new NotSupportedException("The selected PWMMode is not supported by this driver. Please select the Balanced mode.");
            }
            if (dutyCycle > period) // Duty cycle must always be equal to or less than period.
            {
                throw new ArgumentException("Duty cycle must be less than period.");
            }
            ValidatePWMChannel(chip, channel); // Ensure that the chip is valid and that it supports the channel

            string periodPath = Path.Combine(PWMPath, $"pwmchip{chip}", $"pwm{channel}", "period");
            File.WriteAllText(periodPath, Convert.ToString(period)); // Set the period. Must happen first in case duty_cycle is higher than current period.
            
            string dutyCyclePath = Path.Combine(PWMPath, $"pwmchip{chip}", $"pwm{channel}", "duty_cycle");
            File.WriteAllText(dutyCyclePath, Convert.ToString(dutyCycle)); // Set the duty cycle.
            
            string enablePath = Path.Combine(PWMPath, $"pwmchip{chip}", $"pwm{channel}", "enable");
            File.WriteAllText(enablePath, "1"); // Enable PWM.
        }

        protected internal override void SetDebounce(int gpioPinNumber, TimeSpan timeout)
        {
            ValidatePinNumber(gpioPinNumber);

            _debounceTimeouts[gpioPinNumber] = timeout;
        }

        protected internal override TimeSpan GetDebounce(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            TimeSpan timeout = _debounceTimeouts[gpioPinNumber];
            return timeout;
        }

        protected internal override void SetPinEventsToDetect(int gpioPinNumber, PinEvent kind)
        {
            ValidatePinNumber(gpioPinNumber);

            string edgePath = $"{GpioPath}/gpio{gpioPinNumber}/edge";
            string stringValue = EventKindToStringValue(kind);
            File.WriteAllText(edgePath, stringValue);
        }

        protected internal override PinEvent GetPinEventsToDetect(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            string edgePath = $"{GpioPath}/gpio{gpioPinNumber}/edge";
            string stringValue = File.ReadAllText(edgePath);
            PinEvent value = StringValueToEventKind(stringValue);
            return value;
        }

        protected internal override void SetEnableRaisingPinEvents(int gpioPinNumber, bool enable)
        {
            ValidatePinNumber(gpioPinNumber);

            bool wasEnabled = _pinsToDetectEvents.Contains(gpioPinNumber);

            if (enable && !wasEnabled)
            {
                // Enable pin events detection
                _pinsToDetectEvents.Add(gpioPinNumber);
                _pinsToDetectEventsCount++;

                AddPinToPoll(gpioPinNumber, ref _pollFileDescriptor, out _);

                if (_eventDetectionThread == null)
                {
                    _eventDetectionThread = new Thread(DetectEvents)
                    {
                        IsBackground = true
                    };

                    _eventDetectionThread.Start();
                }
            }
            else if (!enable && wasEnabled)
            {
                // Disable pin events detection
                _pinsToDetectEvents.Remove(gpioPinNumber);
                _pinsToDetectEventsCount--;

                bool closePollFileDescriptor = (_pinsToDetectEventsCount == 0);
                RemovePinFromPoll(gpioPinNumber, ref _pollFileDescriptor, closePinValueFileDescriptor: true, closePollFileDescriptor);
            }
        }

        private void AddPinToPoll(int gpioPinNumber, ref int pollFileDescriptor, out bool closePinValueFileDescriptor)
        {
            //Console.WriteLine($"Adding pin to poll: {gpioPinNumber}");

            if (pollFileDescriptor == -1)
            {
                pollFileDescriptor = epoll_create(1);

                if (pollFileDescriptor < 0)
                {
                    throw Utils.CreateIOException("Error initializing pin interrupts", pollFileDescriptor);
                }
            }

            closePinValueFileDescriptor = false;
            bool ok = _pinValueFileDescriptors.TryGetValue(gpioPinNumber, out int fd);

            if (!ok)
            {
                string valuePath = $"{GpioPath}/gpio{gpioPinNumber}/value";
                fd = open(valuePath, FileOpenFlags.O_RDONLY | FileOpenFlags.O_NONBLOCK);

                //Console.WriteLine($"{valuePath} open result: {fd}");

                if (fd < 0)
                {
                    throw Utils.CreateIOException("Error initializing pin interrupts", fd);
                }

                _pinValueFileDescriptors[gpioPinNumber] = fd;
                closePinValueFileDescriptor = true;
            }

            var ev = new epoll_event
            {
                events = PollEvents.EPOLLIN | PollEvents.EPOLLET | PollEvents.EPOLLPRI,
                data = new epoll_data()
                {
                    //fd = fd
                    gpioPinNumber = gpioPinNumber
                }
            };

            //Console.WriteLine($"poll_fd = {pollFileDescriptor}, pin_value_fd = {fd}");

            int r = epoll_ctl(pollFileDescriptor, PollOperations.EPOLL_CTL_ADD, fd, ref ev);

            if (r == -1)
            {
                throw Utils.CreateIOException("Error initializing pin interrupts", r);
            }

            // Ignore first time because it returns the current state
            epoll_wait(pollFileDescriptor, out _, 1, 0);
        }

        private void RemovePinFromPoll(int gpioPinNumber, ref int pollFileDescriptor, bool closePinValueFileDescriptor, bool closePollFileDescriptor)
        {
            //Console.WriteLine($"Removing pin from poll: {gpioPinNumber}");

            int fd = _pinValueFileDescriptors[gpioPinNumber];

            var ev = new epoll_event
            {
                events = PollEvents.EPOLLIN | PollEvents.EPOLLET | PollEvents.EPOLLPRI,
            };

            //Console.WriteLine($"poll_fd = {pollFileDescriptor}, pin_value_fd = {fd}");

            int r = epoll_ctl(pollFileDescriptor, PollOperations.EPOLL_CTL_DEL, fd, ref ev);

            if (r == -1)
            {
                throw Utils.CreateIOException("Error initializing pin interrupts", r);
            }

            if (closePinValueFileDescriptor)
            {
                close(fd);
                _pinValueFileDescriptors.Remove(gpioPinNumber);
            }

            if (closePollFileDescriptor)
            {
                close(pollFileDescriptor);
                pollFileDescriptor = -1;
            }
        }

        protected internal override bool GetEnableRaisingPinEvents(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            bool pinEventsEnabled = _pinsToDetectEvents.Contains(gpioPinNumber);
            return pinEventsEnabled;
        }

        private unsafe void DetectEvents()
        {
            char buf;
            IntPtr bufPtr = new IntPtr(&buf);

            while (_pinsToDetectEventsCount > 0)
            {
                bool eventDetected = WasEventDetected(_pollFileDescriptor, out int gpioPinNumber, timeout: -1);

                if (eventDetected)
                {
                    //Console.WriteLine($"Event detected for pin {gpioPinNumber}");
                    OnPinValueChanged(gpioPinNumber);
                }
            }

            _eventDetectionThread = null;
        }

        protected internal override bool WaitForPinEvent(int gpioPinNumber, TimeSpan timeout)
        {
            ValidatePinNumber(gpioPinNumber);

            int pollFileDescriptor = -1;
            AddPinToPoll(gpioPinNumber, ref pollFileDescriptor, out bool closePinValueFileDescriptor);

            int timeoutInMilliseconds = Convert.ToInt32(timeout.TotalMilliseconds);
            bool eventDetected = WasEventDetected(pollFileDescriptor, out _, timeoutInMilliseconds);

            RemovePinFromPoll(gpioPinNumber, ref pollFileDescriptor, closePinValueFileDescriptor, closePollFileDescriptor: true);
            return eventDetected;
        }

        private bool WasEventDetected(int pollFileDescriptor, out int gpioPinNumber, int timeout)
        {
            bool result = PollForPin(pollFileDescriptor, out gpioPinNumber, timeout);

            if (result)
            {
                bool ok = _debounceTimeouts.TryGetValue(gpioPinNumber, out TimeSpan debounce);

                if (!ok)
                {
                    debounce = TimeSpan.MinValue;
                }

                ok = _lastEvents.TryGetValue(gpioPinNumber, out DateTime last);

                if (!ok)
                {
                    last = DateTime.MinValue;
                }

                DateTime now = DateTime.UtcNow;

                if (now.Subtract(last) > debounce)
                {
                    _lastEvents[gpioPinNumber] = now;
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        private unsafe bool PollForPin(int pollFileDescriptor, out int gpioPinNumber, int timeout)
        {
            char buf;
            IntPtr bufPtr = new IntPtr(&buf);
            bool result;

            //Console.WriteLine($"Before epoll_wait: poll_fd = {pollFileDescriptor}");

            int n = epoll_wait(pollFileDescriptor, out epoll_event events, 1, timeout);

            //Console.WriteLine($"After epoll_wait: resut = {n}");

            if (n == -1)
            {
                throw Utils.CreateIOException("Error initializing pin interrupts", n);
            }

            if (n > 0)
            {
                gpioPinNumber = events.data.gpioPinNumber;
                int fd = _pinValueFileDescriptors[gpioPinNumber];

                lseek(fd, 0, SeekFlags.SEEK_SET);
                int r = read(fd, bufPtr, 1);

                if (r != 1)
                {
                    throw Utils.CreateIOException("Error initializing pin interrupts", r);
                }

                result = true;
            }
            else
            {
                gpioPinNumber = -1;
                result = false;
            }

            return result;
        }

        protected internal override int ConvertPinNumber(int gpioPinNumber, PinNumberingScheme from, PinNumberingScheme to)
        {
            ValidatePinNumber(gpioPinNumber);

            if (from != PinNumberingScheme.Gpio || to != PinNumberingScheme.Gpio)
            {
                throw new NotSupportedException("Only Gpio numbering scheme is supported");
            }

            return gpioPinNumber;
        }

        #region Private Methods

        private void ValidatePWMChip(int chipNumber)
        {
            string chipPath = Path.Combine(PWMPath, $"pwmchip{chipNumber}");
            if (!Directory.Exists(chipPath))
            {
                throw new ArgumentException($"The chip number {chipNumber} is invalid or is not enabled.");
            }
        }

        private void ValidatePWMChannel(int chipNumber, int channel)
        {
            ValidatePWMChip(chipNumber);
            string chipPath = Path.Combine(PWMPath, $"pwmchip{chipNumber}");
            string supportedChannels = File.ReadAllText(Path.Combine(chipPath, "npwm"));
            int numSupportedChannels;
            if (int.TryParse(supportedChannels, out numSupportedChannels))
            {
                if (chipNumber < 0 || chipNumber >= numSupportedChannels)
                {
                    throw new ArgumentException($"The pwm chip {chipNumber} does not support the channel {channel}");
                }
            }
            else
            {
                throw new GpioException($"Unable to parse the number of supported channels at {Path.Combine(chipPath, "npwm")}");
            }
        }

        private void ExportPin(int gpioPinNumber)
        {
            string pinPath = $"{GpioPath}/gpio{gpioPinNumber}";

            if (!Directory.Exists(pinPath))
            {
                File.WriteAllText($"{GpioPath}/export", Convert.ToString(gpioPinNumber));
            }

            _exportedPins.Add(gpioPinNumber);
        }

        private void ExportPWMChannel(int chip, int channel)
        {
            string channelPath = Path.Combine(PWMPath, $"pwmchip{chip}", $"pwm{channel}");
            
            if (!Directory.Exists(channelPath))
            {
                File.WriteAllText(Path.Combine(PWMPath, $"pwmchip{chip}", "export"), Convert.ToString(channel));
            }

            _exportedPWMChannels.Add(new Tuple<int,int>(chip, channel));
        }

        private void UnexportPin(int gpioPinNumber)
        {
            string pinPath = $"{GpioPath}/gpio{gpioPinNumber}";

            if (Directory.Exists(pinPath))
            {
                File.WriteAllText($"{GpioPath}/unexport", Convert.ToString(gpioPinNumber));
            }

            _exportedPins.Remove(gpioPinNumber);
        }

        private void UnexportPWMPin(int chip, int channel)
        {
            string channelPath = Path.Combine(PWMPath, $"pwmchip{chip}", $"pwm{channel}");

            if (Directory.Exists(channelPath))
            {
                File.WriteAllText(Path.Combine(PWMPath, $"pwmchip{chip}", "unexport"), Convert.ToString(channel));
            }
            _exportedPWMChannels.Remove(new Tuple<int, int>(chip, channel));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidatePinMode(PinMode mode)
        {
            bool supportedPinMode = IsPinModeSupported(mode);

            if (!supportedPinMode)
            {
                throw new NotSupportedException($"Not supported GPIO pin mode '{mode}'");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidatePinValue(PinValue value)
        {
            switch (value)
            {
                case PinValue.Low:
                case PinValue.High:
                    // Do nothing
                    break;

                default:
                    throw new ArgumentException($"Invalid GPIO pin value '{value}'");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidatePinNumber(int gpioPinNumber)
        {
            //if (gpioPinNumber < 0 || gpioPinNumber >= PinCount)
            if (gpioPinNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gpioPinNumber));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PinMode StringModeToPinMode(string mode)
        {
            PinMode result;

            switch (mode)
            {
                case "in":
                    result = PinMode.Input;
                    break;
                case "out":
                    result = PinMode.Output;
                    break;
                default:
                    throw new NotSupportedException($"Not supported GPIO pin mode '{mode}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string PinModeToStringMode(PinMode mode)
        {
            string result;

            switch (mode)
            {
                case PinMode.Input:
                    result = "in";
                    break;
                case PinMode.Output:
                    result = "out";
                    break;
                default:
                    throw new NotSupportedException($"Not supported GPIO pin mode '{mode}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PinValue StringValueToPinValue(string value)
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
                    throw new ArgumentException($"Invalid GPIO pin value '{value}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string PinValueToStringValue(PinValue value)
        {
            string result;

            switch (value)
            {
                case PinValue.Low:
                    result = "0";
                    break;
                case PinValue.High:
                    result = "1";
                    break;
                default:
                    throw new ArgumentException($"Invalid GPIO pin value '{value}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string EventKindToStringValue(PinEvent kind)
        {
            string result;

            if (kind == PinEvent.None)
            {
                result = "none";
            }
            else if (kind.HasFlag(PinEvent.SyncFallingRisingEdge) ||
                     kind.HasFlag(PinEvent.AsyncFallingRisingEdge))
            {
                result = "both";
            }
            else if (kind.HasFlag(PinEvent.SyncRisingEdge) ||
                     kind.HasFlag(PinEvent.AsyncRisingEdge))
            {
                result = "rising";
            }
            else if (kind.HasFlag(PinEvent.SyncFallingEdge) ||
                     kind.HasFlag(PinEvent.AsyncFallingEdge))
            {
                result = "falling";
            }
            else
            {
                throw new NotSupportedException($"Not supported GPIO event kind '{kind}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PinEvent StringValueToEventKind(string kind)
        {
            PinEvent result;
            kind = kind.Trim();

            switch (kind)
            {
                case "none":
                    result = PinEvent.None;
                    break;
                case "rising":
                    result = PinEvent.SyncRisingEdge | PinEvent.AsyncRisingEdge;
                    break;
                case "falling":
                    result = PinEvent.SyncFallingEdge | PinEvent.AsyncFallingEdge;
                    break;
                case "both":
                    result = PinEvent.SyncFallingRisingEdge | PinEvent.AsyncFallingRisingEdge;
                    break;
                default:
                    throw new NotSupportedException($"Not supported GPIO event kind '{kind}'");
            }

            return result;
        }

        #endregion
    }
}
