// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static Interop;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A generic GPIO driver for Rockchip SoCs.
    /// </summary>
    /// <remarks>
    /// This is a generic GPIO driver for Rockchip SoCs.
    /// It can even drive the internal pins that are not drawn out.
    /// Before you operate, you must be clear about what you are doing.
    /// </remarks>
    public unsafe class RockchipDriver : SysFsDriver
    {
        #pragma warning disable CS1591
        // these variables are used in subclasses
        protected const string GpioMemoryFilePath = "/dev/mem";

        protected static readonly int _mapMask = Environment.SystemPageSize - 1;
        protected static readonly object s_initializationLock = new object();

        protected IDictionary<int, PinState> _pinModes = new Dictionary<int, PinState>();
        protected IntPtr[] _gpioPointers = Array.Empty<IntPtr>();
        #pragma warning restore CS1591

        /// <summary>
        /// Gpio register addresses.
        /// </summary>
        protected virtual uint[] GpioRegisterAddresses { get; } = Array.Empty<uint>();

        /// <inheritdoc/>
        protected override int PinCount => throw new PlatformNotSupportedException("This driver is generic so it can not enumerate how many pins are available.");

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new PlatformNotSupportedException("This driver is generic so it can not perform conversions between pin numbering schemes.");

        /// <summary>
        /// Initializes a new instance of the <see cref="RockchipDriver"/> class.
        /// </summary>
        protected RockchipDriver()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockchipDriver"/>.
        /// </summary>
        /// <param name="gpioRegisterAddresses">Gpio register addresses (This can be found in the corresponding SoC datasheet).</param>
        public RockchipDriver(uint[] gpioRegisterAddresses)
        {
            GpioRegisterAddresses = gpioRegisterAddresses;
            Initialize();
        }

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            SetPinMode(pinNumber, PinMode.Input);
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            if (_pinModes.ContainsKey(pinNumber))
            {
                if (_pinModes[pinNumber].InUseByInterruptDriver)
                {
                    base.ClosePin(pinNumber);
                }

                switch (_pinModes[pinNumber].CurrentPinMode)
                {
                    case PinMode.InputPullDown:
                    case PinMode.InputPullUp:
                        SetPinMode(pinNumber, PinMode.Input);
                        break;
                    case PinMode.Output:
                        Write(pinNumber, PinValue.Low);
                        SetPinMode(pinNumber, PinMode.Input);
                        break;
                    default:
                        break;
                }

                _pinModes.Remove(pinNumber);
            }
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            // different chips have different number of GRFs and offsets
            // this method needs to be overridden in subclasses
            base.OpenPin(pinNumber);
            base.SetPinMode(pinNumber, mode);

            if (_pinModes.ContainsKey(pinNumber))
            {
                _pinModes[pinNumber].CurrentPinMode = mode;
            }
            else
            {
                _pinModes.Add(pinNumber, new PinState(mode));
            }
        }

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);

            // data register (GPIO_SWPORT_DR) offset is 0x0000
            uint* dataPointer = (uint*)_gpioPointers[unmapped.GpioNumber];
            uint dataValue = *dataPointer;

            if (value == PinValue.High)
            {
                dataValue |= 0b1U << (unmapped.Port * 8 + unmapped.PortNumber);
            }
            else
            {
                dataValue &= ~(0b1U << (unmapped.Port * 8 + unmapped.PortNumber));
            }

            *dataPointer = dataValue;
        }

        /// <inheritdoc/>
        protected unsafe override PinValue Read(int pinNumber)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);

            // data register (GPIO_EXT_PORTA) offset is 0x0050
            uint* dataPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + 0x0050);
            uint dataValue = *dataPointer;

            return Convert.ToBoolean((dataValue >> (unmapped.Port * 8 + unmapped.PortNumber)) & 0b1) ? PinValue.High : PinValue.Low;
        }

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            base.OpenPin(pinNumber);
            base.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            _pinModes[pinNumber].InUseByInterruptDriver = false;

            base.OpenPin(pinNumber);
            base.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
        }

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            base.OpenPin(pinNumber);
            return base.WaitForEvent(pinNumber, eventTypes, cancellationToken);
        }

        /// <inheritdoc/>
        protected override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            base.OpenPin(pinNumber);
            return base.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return mode switch
            {
                PinMode.Input or PinMode.Output => true,
                _ => false,
            };
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            return _pinModes[pinNumber].CurrentPinMode;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            foreach (IntPtr pointer in _gpioPointers)
            {
                Interop.munmap(pointer, 0);
            }

            Array.Clear(_gpioPointers, 0, _gpioPointers.Length);
        }

        private void Initialize()
        {
            if (_gpioPointers.Length != 0)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (_gpioPointers.Length != 0)
                {
                    return;
                }

                int fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                _gpioPointers = new IntPtr[GpioRegisterAddresses.Length];

                for (int i = 0; i < GpioRegisterAddresses.Length; i++)
                {
                    IntPtr map = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(GpioRegisterAddresses[i] & ~_mapMask));

                    if (map.ToInt64() == -1)
                    {
                        Interop.munmap(map, 0);
                        throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (GPIO{i} initialize error).");
                    }

                    _gpioPointers[i] = map;
                }

                Interop.close(fileDescriptor);
            }
        }

        /// <summary>
        /// Map pin number with port name to pin number in the driver's logical numbering scheme.
        /// </summary>
        /// <param name="gpioNumber">Number of GPIOs.</param>
        /// <param name="port">Port name, from 'A' to 'D'.</param>
        /// <param name="portNumber">Number of pins.</param>
        /// <returns>Pin number in the driver's logical numbering scheme.</returns>
        public static int MapPinNumber(int gpioNumber, char port, int portNumber)
        {
            // For example, GPIO4_D5 = {4}*32 + {3}*8 + {5} = 157
            // https://wiki.radxa.com/Rockpi4/hardware/gpio
            return 32 * gpioNumber +
                8 * ((port >= 'A' && port <= 'D') ? port - 'A' : throw new Exception()) +
                portNumber;
        }

        /// <summary>
        /// Unmap pin number in the driver's logical numbering scheme to pin number with port name.
        /// </summary>
        /// <param name="pinNumber">Pin number in the driver's logical numbering scheme.</param>
        /// <returns>Pin number with port name.</returns>
        protected (int GpioNumber, int Port, int PortNumber) UnmapPinNumber(int pinNumber)
        {
            int portNumber = pinNumber % 8;
            int port = (pinNumber - portNumber) % 32 / 8;
            int gpioNumber = (pinNumber - portNumber) / 32;

            return (gpioNumber, port, portNumber);
        }
    }
}
