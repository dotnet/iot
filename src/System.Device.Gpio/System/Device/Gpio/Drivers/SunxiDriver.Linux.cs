// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A generic GPIO driver for Allwinner SoCs.
    /// </summary>
    /// <remarks>
    /// This is a generic GPIO driver for Allwinner SoCs.
    /// It can even drive the internal pins that are not drawn out.
    /// Before you operate, you must be clear about what you are doing.
    /// </remarks>
    public unsafe partial class SunxiDriver : GpioDriver
    {
        private IntPtr _gpioPointer0;
        private IntPtr _gpioPointer1;

        /// <summary>
        /// CPUX-PORT base address.
        /// </summary>
        protected virtual int GpioRegisterOffset0 { get; }
        /// <summary>
        /// CPUS-PORT base address.
        /// </summary>
        protected virtual int GpioRegisterOffset1 { get; }
        // final_address = mapped_address + (target_address & map_mask) https://stackoverflow.com/a/37922968
        private readonly int _mapMask = Environment.SystemPageSize - 1;

        private static readonly object s_initializationLock = new object();
        private static readonly object s_sysFsInitializationLock = new object();
        private const string GpioMemoryFilePath = "/dev/mem";
        private UnixDriver _sysFSDriver;
        private readonly IDictionary<int, PinMode> _sysFSModes = new Dictionary<int, PinMode>();

        protected SunxiDriver() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SunxiDriver"/>.
        /// </summary>
        /// <param name="gpioRegisterOffset0">CPUX-PORT base address (This can be find in the corresponding SoC datasheet).</param>
        /// <param name="gpioRegisterOffset1">CPUS-PORT base address (This can be find in the corresponding SoC datasheet).</param>
        public SunxiDriver(int gpioRegisterOffset0, int gpioRegisterOffset1)
        {
            GpioRegisterOffset0 = gpioRegisterOffset0;
            GpioRegisterOffset1 = gpioRegisterOffset1;
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
            Initialize();
            SetPinMode(pinNumber, PinMode.Input);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void ClosePin(int pinNumber)
        {
            if (_sysFSModes.ContainsKey(pinNumber) && _sysFSModes[pinNumber] == PinMode.Output)
            {
                Write(pinNumber, PinValue.Low);
                SetPinMode(pinNumber, PinMode.Input);
            }
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException($"The pin {pinNumber} does not support the selected mode {mode}.");
            }

            // Get port controller, port number and shift
            var unmapped = UnmapPinNumber(pinNumber);
            int cfgNum = unmapped.Port / 8;
            int cfgShift = unmapped.Port % 8;
            int pulNum = unmapped.Port / 16;
            int pulShift = unmapped.Port % 16;

            // Get register address, register pointer
            int cfgAddress, pulAddress;
            uint* cfgPointer, pulPointer;
            if (unmapped.PortController < 10)
            {
                cfgAddress = (GpioRegisterOffset0 + unmapped.PortController * 0x24 + cfgNum * 0x04) & _mapMask;
                pulAddress = (GpioRegisterOffset0 + unmapped.PortController * 0x24 + (pulNum + 7) * 0x04) & _mapMask;

                cfgPointer = (uint*)(_gpioPointer0 + cfgAddress);
                pulPointer = (uint*)(_gpioPointer0 + pulAddress);
            }
            else
            {
                cfgAddress = (GpioRegisterOffset1 + unmapped.PortController * 0x24 + cfgNum * 0x04) & _mapMask;
                pulAddress = (GpioRegisterOffset1 + unmapped.PortController * 0x24 + (pulNum + 7) * 0x04) & _mapMask;

                cfgPointer = (uint*)(_gpioPointer1 + cfgAddress);
                pulPointer = (uint*)(_gpioPointer1 + pulAddress);
            }

            uint cfgValue = *cfgPointer;
            uint pulValue = *pulPointer;

            // Clear register
            cfgValue &= ~(0xFU << (cfgShift * 4));
            pulValue &= ~(0b_11U << (pulShift * 2));

            switch (mode)
            {
                case PinMode.Output:
                    cfgValue |= (0b_001U << (cfgShift * 4));
                    break;
                case PinMode.Input:
                    // After clearing the register, the value is the input mode.
                    break;
                case PinMode.InputPullDown:
                    pulValue |= (0b_10U << (pulShift * 2));
                    break;
                case PinMode.InputPullUp:
                    pulValue |= (0b_01U << (pulShift * 2));
                    break;
                default:
                    throw new ArgumentException();
            }

            *cfgPointer = cfgValue;
            Thread.SpinWait(150);
            *pulPointer = pulValue;

            if (_sysFSModes.ContainsKey(pinNumber))
            {
                _sysFSModes[pinNumber] = mode;
            }
            else
            {
                _sysFSModes.Add(pinNumber, mode);
            }
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal override void Write(int pinNumber, PinValue value)
        {
            var unmapped = UnmapPinNumber(pinNumber);

            int dataAddress;
            uint* dataPointer;
            if (unmapped.PortController < 10)
            {
                dataAddress = (GpioRegisterOffset0 + unmapped.PortController * 0x24 + 0x10) & _mapMask;

                dataPointer = (uint*)(_gpioPointer0 + dataAddress);
            }
            else
            {
                dataAddress = (GpioRegisterOffset1 + unmapped.PortController * 0x24 + 0x10) & _mapMask;

                dataPointer = (uint*)(_gpioPointer1 + dataAddress);
            }

            uint dataValue = *dataPointer;

            if (value == PinValue.High)
            {
                dataValue |= (uint)(1 << unmapped.Port);
            }
            else
            {
                dataValue &= (uint)~(1 << unmapped.Port);
            }

            *dataPointer = dataValue;
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal unsafe override PinValue Read(int pinNumber)
        {
            var unmapped = UnmapPinNumber(pinNumber);

            int dataAddress;
            uint* dataPointer;
            if (unmapped.PortController < 10)
            {
                dataAddress = (GpioRegisterOffset0 + unmapped.PortController * 0x24 + 0x10) & _mapMask;

                dataPointer = (uint*)(_gpioPointer0 + dataAddress);
            }
            else
            {
                dataAddress = (GpioRegisterOffset1 + unmapped.PortController * 0x24 + 0x10) & _mapMask;

                dataPointer = (uint*)(_gpioPointer1 + dataAddress);
            }

            uint dataValue = *dataPointer;

            return Convert.ToBoolean((dataValue >> unmapped.Port) & 1) ? PinValue.High : PinValue.Low;
        }

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            _sysFSDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
        }

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            _sysFSDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            return _sysFSDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Async call until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            return _sysFSDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                case PinMode.Output:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (!_sysFSModes.ContainsKey(pinNumber))
            {
                throw new InvalidOperationException("Can not get a pin mode of a pin that is not open.");
            }

            return _sysFSModes[pinNumber];
        }

        /// <summary>
        /// Gets the mode of a pin for Unix.
        /// </summary>
        /// <param name="mode">The mode of a pin to get.</param>
        /// <returns>The mode of a pin for Unix.</returns>
        private PinMode GetModeForUnixDriver(PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullUp:
                case PinMode.InputPullDown:
                    return PinMode.Input;
                case PinMode.Output:
                    return PinMode.Output;
                default:
                    throw new InvalidOperationException($"Can not parse pin mode {_sysFSModes}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_gpioPointer0 != default)
            {
                Interop.munmap(_gpioPointer0, 0);
                _gpioPointer0 = default;
            }

            if (_gpioPointer1 != default)
            {
                Interop.munmap(_gpioPointer1, 0);
                _gpioPointer1 = default;
            }

            if (_sysFSDriver != default)
            {
                _sysFSDriver.Dispose();
                _sysFSDriver = default;
            }
        }

        private void InitializeSysFS()
        {
            if (_sysFSDriver != default)
            {
                return;
            }

            lock (s_sysFsInitializationLock)
            {
                if (_sysFSDriver != default)
                {
                    return;
                }
                _sysFSDriver = new SysFsDriver();
            }
        }

        private void Initialize()
        {
            if (_gpioPointer0 != default)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (_gpioPointer0 != default)
                {
                    return;
                }

                int fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                IntPtr mapPointer0 = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize - 1, (MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE), MemoryMappedFlags.MAP_SHARED, fileDescriptor, GpioRegisterOffset0 & ~_mapMask);
                IntPtr mapPointer1 = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize - 1, (MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE), MemoryMappedFlags.MAP_SHARED, fileDescriptor, GpioRegisterOffset1 & ~_mapMask);
                if (mapPointer0.ToInt64() == -1 || mapPointer1.ToInt64() == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                Interop.close(fileDescriptor);

                _gpioPointer0 = mapPointer0;
                _gpioPointer1 = mapPointer0;
            }
        }

        /// <summary>
        /// Map pin number with port controller name to pin number in the driver's logical numbering scheme.
        /// </summary>
        /// <param name="portController">Port controller name, like 'A', 'C'.</param>
        /// <param name="port">Number of pins.</param>
        /// <returns>Pin number in the driver's logical numbering scheme.</returns>
        public static int MapPinNumber(char portController, int port)
        {
            int alphabetPosition = MapPortController(portController);

            return alphabetPosition * 32 + port;
        }

        private (int PortController, int Port) UnmapPinNumber(int pinNumber)
        {
            int port = pinNumber % 32;
            int portController = (pinNumber - port) / 32;

            return (portController, port);
        }

        private static int MapPortController(char portController)
        {
            return portController switch
            {
                'A' => 0,
                'B' => 1,
                'C' => 2,
                'D' => 3,
                'E' => 4,
                'F' => 5,
                'G' => 6,
                'H' => 7,
                'I' => 8,
                'J' => 9,
                'K' => 10,
                'L' => 11,
                'M' => 12,
                _ => throw new Exception()
            };
        }
    }
}
