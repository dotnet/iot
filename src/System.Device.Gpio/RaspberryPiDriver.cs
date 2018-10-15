// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Device.Gpio;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Device.Gpio
{
    public unsafe class RaspberryPiDriver : GpioDriver
    {
        #region RegisterView        

        [StructLayout(LayoutKind.Explicit)]
        private struct RegisterView
        {
            ///<summary>GPIO Function Select, 6x32 bits, R/W</summary>
            [FieldOffset(0x00)]
            public fixed uint GPFSEL[6];

            ///<summary>GPIO Pin Output Set, 2x32 bits, W</summary>
            [FieldOffset(0x1C)]
            public fixed uint GPSET[2];

            ///<summary>GPIO Pin Output Clear, 2x32 bits, W</summary>
            [FieldOffset(0x28)]
            public fixed uint GPCLR[2];

            ///<summary>GPIO Pin Level, 2x32 bits, R</summary>
            [FieldOffset(0x34)]
            public fixed uint GPLEV[2];

            ///<summary>GPIO Pin Event Detect Status, 2x32 bits, R/W</summary>
            [FieldOffset(0x40)]
            public fixed uint GPEDS[2];

            ///<summary>GPIO Pin Rising Edge Detect Enable, 2x32 bits, R/W</summary>
            [FieldOffset(0x4C)]
            public fixed uint GPREN[2];

            ///<summary>GPIO Pin Falling Edge Detect Enable, 2x32 bits, R/W</summary>
            [FieldOffset(0x58)]
            public fixed uint GPFEN[2];

            ///<summary>GPIO Pin High Detect Enable, 2x32 bits, R/W</summary>
            [FieldOffset(0x64)]
            public fixed uint GPHEN[2];

            ///<summary>GPIO Pin Low Detect Enable, 2x32 bits, R/W</summary>
            [FieldOffset(0x70)]
            public fixed uint GPLEN[2];

            ///<summary>GPIO Pin Async. Rising Edge Detect, 2x32 bits, R/W</summary>
            [FieldOffset(0x7C)]
            public fixed uint GPAREN[2];

            ///<summary>GPIO Pin Async. Falling Edge Detect, 2x32 bits, R/W</summary>
            [FieldOffset(0x88)]
            public fixed uint GPAFEN[2];

            ///<summary>GPIO Pin Pull-up/down Enable, 32 bits, R/W</summary>
            [FieldOffset(0x94)]
            public uint GPPUD;

            ///<summary>GPIO Pin Pull-up/down Enable Clock, 2x32 bits, R/W</summary>
            [FieldOffset(0x98)]
            public fixed uint GPPUDCLK[2];
        }

        #endregion

        #region Interop

        private const string LibraryName = "libc";

        [Flags]
        private enum FileOpenFlags
        {
            O_RDWR = 0x02,
            O_SYNC = 0x101000
        }

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, FileOpenFlags flags);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int close(int fd);

        [Flags]
        private enum MemoryMappedProtections
        {
            PROT_NONE = 0x0,
            PROT_READ = 0x1,
            PROT_WRITE = 0x2,
            PROT_EXEC = 0x4
        }

        [Flags]
        private enum MemoryMappedFlags
        {
            MAP_SHARED = 0x01,
            MAP_PRIVATE = 0x02,
            MAP_FIXED = 0x10
        }

        [DllImport(LibraryName, SetLastError = true)]
        private static extern IntPtr mmap(IntPtr addr, int length, MemoryMappedProtections prot, MemoryMappedFlags flags, int fd, int offset);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int munmap(IntPtr addr, int length);

        #endregion

        private const string GpioMemoryFilePath = "/dev/gpiomem";
        private const int GpioBaseOffset = 0;

        private RegisterView* _registerViewPointer = null;

        private int _pinsToDetectEventsCount;
        private IList<int> _pinsToDetectEvents;
        private Thread _eventDetectionThread;
        private IDictionary<int, TimeSpan> _debounceTimeouts;
        private IDictionary<int, DateTime> _lastEvents;

        public RaspberryPiDriver()
        {
        }

        private void Initialize()
        {
            if (_registerViewPointer != null)
            {
                return;
            }

            int fileDescriptor = open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);

            if (fileDescriptor < 0)
            {
                throw Utils.CreateIOException("Error initializing Gpio driver", fileDescriptor);
            }

            //Console.WriteLine($"file descriptor = {fileDescriptor}");

            IntPtr mapPointer = mmap(IntPtr.Zero, Environment.SystemPageSize, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, GpioBaseOffset);

            if (mapPointer.ToInt32() < 0)
            {
                throw Utils.CreateIOException("Error initializing Gpio driver", mapPointer.ToInt32());
            }

            //Console.WriteLine($"mmap returned address = {mapPointer.ToInt32():X16}");

            close(fileDescriptor);

            _registerViewPointer = (RegisterView*)mapPointer;

            _pinsToDetectEvents = new List<int>(PinCount);
            _debounceTimeouts = new Dictionary<int, TimeSpan>(PinCount);
            _lastEvents = new Dictionary<int, DateTime>(PinCount);
        }

        public override void Dispose()
        {
            _pinsToDetectEventsCount = 0;

            if (_registerViewPointer != null)
            {
                munmap((IntPtr)_registerViewPointer, 0);
                _registerViewPointer = null;
            }
        }

        protected internal override int PinCount => 54;

        protected internal override bool IsPinModeSupported(PinMode mode)
        {
            bool result;

            switch (mode)
            {
                case PinMode.Input:
                case PinMode.Output:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
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
            Initialize();
        }

        protected internal override void OpenPWMPin(int chip, int channel)
        {
            // ToDo: Add validation for chip and channel.
            // ToDo: Add initialization setup required for PWM if it is not done already.
        }

        protected internal override void ClosePin(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            SetPinEventsToDetect(gpioPinNumber, PinEvent.None);
            _debounceTimeouts.Remove(gpioPinNumber);
            _lastEvents.Remove(gpioPinNumber);
        }

        protected internal override void SetPinMode(int gpioPinNumber, PinMode mode)
        {
            ValidatePinNumber(gpioPinNumber);
            ValidatePinMode(mode);

            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    SetInputPullMode(gpioPinNumber, PinModeToGPPUD(mode));
                    break;
            }

            SetPinMode(gpioPinNumber, PinModeToGPFSEL(mode));
        }

        private void SetInputPullMode(int gpioPinNumber, uint mode)
        {
            /*
             * The GPIO Pull - up/down Clock Registers control the actuation of internal pull-downs on the respective GPIO pins.
             * These registers must be used in conjunction with the GPPUD register to effect GPIO Pull-up/down changes.
             * The following sequence of events is required: 
             * 
             * 1. Write to GPPUD to set the required control signal (i.e.Pull-up or Pull-Down or neither to remove the current Pull-up/down)
             * 2. Wait 150 cycles – this provides the required set-up time for the control signal 
             * 3. Write to GPPUDCLK0/1 to clock the control signal into the GPIO pads you wish to modify
             *    – NOTE only the pads which receive a clock will be modified, all others will retain their previous state.
             * 4. Wait 150 cycles – this provides the required hold time for the control signal 
             * 5. Write to GPPUD to remove the control signal 
             * 6. Write to GPPUDCLK0/1 to remove the clock
             */

            //SetBits(RegisterViewPointer->GPPUD, pin, 1, 2, mode);
            //Thread.SpinWait(150);
            //SetBit(RegisterViewPointer->GPPUDCLK, pin);
            //Thread.SpinWait(150);
            //SetBit(RegisterViewPointer->GPPUDCLK, pin, 0U);

            uint* gppudPointer = &_registerViewPointer->GPPUD;

            //Console.WriteLine($"{nameof(RegisterView.GPPUD)} register address = {(long)gppudPointer:X16}");

            uint register = *gppudPointer;

            //Console.WriteLine($"{nameof(RegisterView.GPPUD)} original register value = {register:X8}");

            register &= ~0b11U;
            register |= mode;

            //Console.WriteLine($"{nameof(RegisterView.GPPUD)} new register value = {register:X8}");

            *gppudPointer = register;

            // Wait 150 cycles – this provides the required set-up time for the control signal
            Thread.SpinWait(150);

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            uint* gppudclkPointer = &_registerViewPointer->GPPUDCLK[index];

            //Console.WriteLine($"{nameof(RegisterView.GPPUDCLK)} register address = {(long)gppudclkPointer:X16}");

            register = *gppudclkPointer;

            //Console.WriteLine($"{nameof(RegisterView.GPPUDCLK)} original register value = {register:X8}");

            register |= 1U << shift;

            //Console.WriteLine($"{nameof(RegisterView.GPPUDCLK)} new register value = {register:X8}");

            *gppudclkPointer = register;

            // Wait 150 cycles – this provides the required hold time for the control signal 
            Thread.SpinWait(150);

            register = *gppudPointer;
            register &= ~0b11U;

            //Console.WriteLine($"{nameof(RegisterView.GPPUD)} new register value = {register:X8}");
            //Console.WriteLine($"{nameof(RegisterView.GPPUDCLK)} new register value = {0:X8}");

            *gppudPointer = register;
            *gppudclkPointer = 0;
        }

        private void SetPinMode(int gpioPinNumber, uint mode)
        {
            //SetBits(RegisterViewPointer->GPFSEL, pin, 10, 3, mode);

            int index = gpioPinNumber / 10;
            int shift = (gpioPinNumber % 10) * 3;
            uint* registerPointer = &_registerViewPointer->GPFSEL[index];

            //Console.WriteLine($"{nameof(RegisterView.GPFSEL)} register address = {(long)registerPointer:X16}");

            uint register = *registerPointer;

            //Console.WriteLine($"{nameof(RegisterView.GPFSEL)} original register value = {register:X8}");

            register &= ~(0b111U << shift);
            register |= mode << shift;

            //Console.WriteLine($"{nameof(RegisterView.GPFSEL)} new register value = {register:X8}");

            *registerPointer = register;
        }

        protected internal override PinMode GetPinMode(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            //var mode = GetBits(RegisterViewPointer->GPFSEL, pin, 10, 3);

            int index = gpioPinNumber / 10;
            int shift = (gpioPinNumber % 10) * 3;
            uint register = _registerViewPointer->GPFSEL[index];
            uint mode = (register >> shift) & 0b111U;

            PinMode result = GPFSELToPinMode(mode);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint PinModeToGPPUD(PinMode mode)
        {
            uint result;

            switch (mode)
            {
                case PinMode.Input:
                    result = 0;
                    break;
                case PinMode.InputPullDown:
                    result = 1;
                    break;
                case PinMode.InputPullUp:
                    result = 2;
                    break;

                default:
                    throw new NotSupportedException($"Not supported GPIO pin mode '{mode}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint PinModeToGPFSEL(PinMode mode)
        {
            uint result;

            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    result = 0;
                    break;

                case PinMode.Output:
                    result = 1;
                    break;

                default:
                    throw new NotSupportedException($"Not supported GPIO pin mode '{mode}'");
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PinMode GPFSELToPinMode(uint gpfselValue)
        {
            PinMode result;

            switch (gpfselValue)
            {
                case 0:
                    result = PinMode.Input;
                    break;
                case 1:
                    result = PinMode.Output;
                    break;

                default:
                    throw new NotSupportedException($"Not supported GPIO pin mode '{gpfselValue}'");
            }

            return result;
        }

        protected internal override void Output(int gpioPinNumber, PinValue value)
        {
            ValidatePinNumber(gpioPinNumber);
            ValidatePinValue(value);

            //switch (value)
            //{
            //    case GpioPinValue.High:
            //        SetBit(RegisterViewPointer->GPSET, pin);
            //        break;

            //    case GpioPinValue.Low:
            //        SetBit(RegisterViewPointer->GPCLR, pin);
            //        break;

            //    default: throw new InvalidGpioPinValueException(value);
            //}

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            uint* registerPointer = null;
            string registerName = string.Empty;

            switch (value)
            {
                case PinValue.High:
                    registerPointer = &_registerViewPointer->GPSET[index];
                    registerName = nameof(RegisterView.GPSET);
                    break;

                case PinValue.Low:
                    registerPointer = &_registerViewPointer->GPCLR[index];
                    registerName = nameof(RegisterView.GPCLR);
                    break;

                default:
                    throw new ArgumentException($"Invalid GPIO pin value '{value}'");
            }

            //Console.WriteLine($"{registerName} register address = {(long)registerPointer:X16}");

            uint register = 1U << shift;

            //Console.WriteLine($"{registerName} new register value = {register:X8}");

            *registerPointer = register;
        }

        protected internal override PinValue Input(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            //var value = GetBit(RegisterViewPointer->GPLEV, pin);

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            uint register = _registerViewPointer->GPLEV[index];
            uint value = (register >> shift) & 1;

            PinValue result = Convert.ToBoolean(value) ? PinValue.High : PinValue.Low;
            return result;
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

        protected internal override void SetPinEventsToDetect(int gpioPinNumber, PinEvent events)
        {
            ValidatePinNumber(gpioPinNumber);

            PinEvent kind = PinEvent.Low;
            bool enabled = events.HasFlag(kind);
            SetEventDetection(gpioPinNumber, kind, enabled);

            kind = PinEvent.High;
            enabled = events.HasFlag(kind);
            SetEventDetection(gpioPinNumber, kind, enabled);

            kind = PinEvent.SyncRisingEdge;
            enabled = events.HasFlag(kind);
            SetEventDetection(gpioPinNumber, kind, enabled);

            kind = PinEvent.SyncFallingEdge;
            enabled = events.HasFlag(kind);
            SetEventDetection(gpioPinNumber, kind, enabled);

            kind = PinEvent.AsyncRisingEdge;
            enabled = events.HasFlag(kind);
            SetEventDetection(gpioPinNumber, kind, enabled);

            kind = PinEvent.AsyncFallingEdge;
            enabled = events.HasFlag(kind);
            SetEventDetection(gpioPinNumber, kind, enabled);

            ClearDetectedEvent(gpioPinNumber);
        }

        private void SetEventDetection(int gpioPinNumber, PinEvent kind, bool enabled)
        {
            //switch (kind)
            //{
            //    case GpioEventKind.High:
            //        SetBit(RegisterViewPointer->GPHEN, pin);
            //        break;

            //    case GpioEventKind.Low:
            //        SetBit(RegisterViewPointer->GPLEN, pin);
            //        break;

            //    case GpioEventKind.SyncRisingEdge:
            //        SetBit(RegisterViewPointer->GPREN, pin);
            //        break;

            //    case GpioEventKind.SyncFallingEdge:
            //        SetBit(RegisterViewPointer->GPFEN, pin);
            //        break;

            //    case GpioEventKind.AsyncRisingEdge:
            //        SetBit(RegisterViewPointer->GPAREN, pin);
            //        break;

            //    case GpioEventKind.AsyncFallingEdge:
            //        SetBit(RegisterViewPointer->GPAFEN, pin);
            //        break;

            //    default: throw new InvalidGpioEventKindException(kind);
            //}

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            uint* registerPointer = null;
            string registerName = string.Empty;

            switch (kind)
            {
                case PinEvent.High:
                    registerPointer = &_registerViewPointer->GPHEN[index];
                    registerName = nameof(RegisterView.GPHEN);
                    break;

                case PinEvent.Low:
                    registerPointer = &_registerViewPointer->GPLEN[index];
                    registerName = nameof(RegisterView.GPLEN);
                    break;

                case PinEvent.SyncRisingEdge:
                    registerPointer = &_registerViewPointer->GPREN[index];
                    registerName = nameof(RegisterView.GPREN);
                    break;

                case PinEvent.SyncFallingEdge:
                    registerPointer = &_registerViewPointer->GPFEN[index];
                    registerName = nameof(RegisterView.GPFEN);
                    break;

                case PinEvent.AsyncRisingEdge:
                    registerPointer = &_registerViewPointer->GPAREN[index];
                    registerName = nameof(RegisterView.GPAREN);
                    break;

                case PinEvent.AsyncFallingEdge:
                    registerPointer = &_registerViewPointer->GPAFEN[index];
                    registerName = nameof(RegisterView.GPAFEN);
                    break;

                default:
                    throw new ArgumentException($"Invalid GPIO event kind '{kind}'");
            }

            //Console.WriteLine($"{registerName} register address = {(long)registerPointer:X16}");

            uint register = *registerPointer;

            //Console.WriteLine($"{registerName} original register value = {register:X8}");

            if (enabled)
            {
                register |= 1U << shift;
            }
            else
            {
                register &= ~(1U << shift);
            }

            //Console.WriteLine($"{registerName} new register value = {register:X8}");

            *registerPointer = register;
        }

        protected internal override PinEvent GetPinEventsToDetect(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            PinEvent result = PinEvent.None;
            PinEvent kind = PinEvent.Low;
            bool enabled = GetEventDetection(gpioPinNumber, kind);
            if (enabled)
            {
                result |= kind;
            }

            kind = PinEvent.High;
            enabled = GetEventDetection(gpioPinNumber, kind);
            if (enabled)
            {
                result |= kind;
            }

            kind = PinEvent.SyncRisingEdge;
            enabled = GetEventDetection(gpioPinNumber, kind);
            if (enabled)
            {
                result |= kind;
            }

            kind = PinEvent.SyncFallingEdge;
            enabled = GetEventDetection(gpioPinNumber, kind);
            if (enabled)
            {
                result |= kind;
            }

            kind = PinEvent.AsyncRisingEdge;
            enabled = GetEventDetection(gpioPinNumber, kind);
            if (enabled)
            {
                result |= kind;
            }

            kind = PinEvent.AsyncFallingEdge;
            enabled = GetEventDetection(gpioPinNumber, kind);
            if (enabled)
            {
                result |= kind;
            }

            return result;
        }

        private bool GetEventDetection(int gpioPinNumber, PinEvent kind)
        {
            //switch (kind)
            //{
            //    case GpioEventKind.High:
            //        value = GetBit(RegisterViewPointer->GPHEN, pin);
            //        break;

            //    case GpioEventKind.Low:
            //        value = GetBit(RegisterViewPointer->GPLEN, pin);
            //        break;

            //    case GpioEventKind.SyncRisingEdge:
            //        value = GetBit(RegisterViewPointer->GPREN, pin);
            //        break;

            //    case GpioEventKind.SyncFallingEdge:
            //        value = GetBit(RegisterViewPointer->GPFEN, pin);
            //        break;

            //    case GpioEventKind.AsyncRisingEdge:
            //        value = GetBit(RegisterViewPointer->GPAREN, pin);
            //        break;

            //    case GpioEventKind.AsyncFallingEdge:
            //        value = GetBit(RegisterViewPointer->GPAFEN, pin);
            //        break;

            //    default: throw new InvalidGpioEventKindException(kind);
            //}

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            string registerName = string.Empty;
            uint register = 0U;

            switch (kind)
            {
                case PinEvent.High:
                    register = _registerViewPointer->GPHEN[index];
                    break;

                case PinEvent.Low:
                    register = _registerViewPointer->GPLEN[index];
                    break;

                case PinEvent.SyncRisingEdge:
                    register = _registerViewPointer->GPREN[index];
                    break;

                case PinEvent.SyncFallingEdge:
                    register = _registerViewPointer->GPFEN[index];
                    break;

                case PinEvent.AsyncRisingEdge:
                    register = _registerViewPointer->GPAREN[index];
                    break;

                case PinEvent.AsyncFallingEdge:
                    register = _registerViewPointer->GPAFEN[index];
                    break;

                default:
                    throw new ArgumentException($"Invalid GPIO event kind '{kind}'");
            }

            uint value = (register >> shift) & 1;

            bool result = Convert.ToBoolean(value);
            return result;
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
            }
        }

        protected internal override bool GetEnableRaisingPinEvents(int gpioPinNumber)
        {
            ValidatePinNumber(gpioPinNumber);

            bool pinEventsEnabled = _pinsToDetectEvents.Contains(gpioPinNumber);
            return pinEventsEnabled;
        }

        private void DetectEvents()
        {
            while (_pinsToDetectEventsCount > 0)
            {
                foreach (int gpioPinNumber in _pinsToDetectEvents)
                {
                    bool eventDetected = WasEventDetected(gpioPinNumber);

                    if (eventDetected)
                    {
                        //Console.WriteLine($"Event detected for pin {i}");
                        OnPinValueChanged(gpioPinNumber);
                    }
                }
            }

            _eventDetectionThread = null;
        }

        protected internal override bool WaitForPinEvent(int gpioPinNumber, TimeSpan timeout)
        {
            ValidatePinNumber(gpioPinNumber);

            DateTime initial = DateTime.UtcNow;
            TimeSpan elapsed;
            bool eventDetected;

            do
            {
                eventDetected = WasEventDetected(gpioPinNumber);
                elapsed = DateTime.UtcNow.Subtract(initial);
            }
            while (!eventDetected && elapsed < timeout);

            return eventDetected;
        }

        private bool WasEventDetected(int gpioPinNumber)
        {
            //var value = GetBit(RegisterViewPointer->GPEDS, pin);

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            uint register = _registerViewPointer->GPEDS[index];
            uint value = (register >> shift) & 1;

            bool result = Convert.ToBoolean(value);

            if (result)
            {
                ClearDetectedEvent(gpioPinNumber);

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

        private void ClearDetectedEvent(int gpioPinNumber)
        {
            //SetBit(RegisterViewPointer->GPEDS, pin);

            int index = gpioPinNumber / 32;
            int shift = gpioPinNumber % 32;
            uint* registerPointer = &_registerViewPointer->GPEDS[index];

            //Console.WriteLine($"{nameof(RegisterView.GPEDS)} register address = {(long)registerPointer:X16}");

            uint register = *registerPointer;

            //Console.WriteLine($"{nameof(RegisterView.GPEDS)} original register value = {register:X8}");

            register |= 1U << shift;

            //Console.WriteLine($"{nameof(RegisterView.GPEDS)} new register value = {register:X8}");

            *registerPointer = register;

            // Wait 150 cycles
            Thread.SpinWait(150);

            *registerPointer = 0;
        }

        protected internal override int ConvertPinNumber(int pinNumber, PinNumberingScheme from, PinNumberingScheme to)
        {
            int result = -1;

            switch (from)
            {
                case PinNumberingScheme.Gpio:
                    switch (to)
                    {
                        case PinNumberingScheme.Gpio:
                            result = pinNumber;
                            break;

                        case PinNumberingScheme.Board:
                            //throw new NotImplementedException();
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported GPIO pin numbering scheme {to}");
                    }
                    break;

                case PinNumberingScheme.Board:
                    switch (to)
                    {
                        case PinNumberingScheme.Board:
                            result = pinNumber;
                            break;

                        case PinNumberingScheme.Gpio:
                            //throw new NotImplementedException();
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported GPIO pin numbering scheme {to}");
                    }
                    break;

                default:
                    throw new NotSupportedException($"Unsupported GPIO pin numbering scheme {from}");
            }

            return result;
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

        protected internal override void ClosePWMPin(int chip, int channel)
        {
            throw new NotImplementedException();
        }

        protected internal override void PWMWrite(int chip, int channel, PWMMode mode, int period, int dutyCycle)
        {
            throw new NotImplementedException();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void SetBit(UInt32* pointer, int bit, uint value = 1)
        //{
        //    SetBits(pointer, bit, 32, 1, value);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static bool GetBit(UInt32* pointer, int bit)
        //{
        //    var result = GetBits(pointer, bit, 32, 1);
        //    return Convert.ToBoolean(result);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void SetBits(UInt32* pointer, int item, int itemsPerRegister, int bitsPerItem, uint value)
        //{
        //    var index = item / itemsPerRegister;
        //    var shift = (item % itemsPerRegister) * bitsPerItem;
        //    var mask = (uint)(1 << bitsPerItem) - 1;
        //    uint* registerPointer = &pointer[index];
        //    var register = *registerPointer;
        //    register &= ~(mask << shift);
        //    register |= value << shift;
        //    *registerPointer = register;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static uint GetBits(UInt32* pointer, int item, int itemsPerSlot, int bitsPerItem)
        //{
        //    var index = item / itemsPerSlot;
        //    var shift = (item % itemsPerSlot) * bitsPerItem;
        //    var mask = (uint)(1 << bitsPerItem) - 1;
        //    var register = pointer[index];
        //    var value = (register >> shift) & mask;
        //    return value;
        //}
    }
}
