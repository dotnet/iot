// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    public unsafe partial class RaspberryPi3Driver : GpioDriver
    {
        private const int ENOENT = 2; // error indicates that an entity doesn't exist
        private RegisterView* _registerViewPointer = null;
        private static readonly object s_initializationLock = new object();
        private static readonly object s_sysFsInitializationLock = new object();
        private const uint PeripheralBaseAddressBcm2835 = 0x2000_0000;
        private const uint PeripheralBaseAddressBcm2836 = 0x3F00_0000;
        private const uint PeripheralBaseAddressBcm2838 = 0xFE00_0000;
        private const uint PeripheralBaseAddressVideocore = 0x7E00_0000;
        private const uint InvalidPeripheralBaseAddress = 0xFFFF_FFFF;
        private const uint GpioPeripheralOffset = 0x0020_0000; // offset from the peripheral base address of the GPIO registers
        private const string GpioMemoryFilePath = "/dev/gpiomem";
        private const string MemoryFilePath = "/dev/mem";
        private const string DeviceTreeRanges = "/proc/device-tree/soc/ranges";
        private UnixDriver _sysFSDriver = null;
        private readonly IDictionary<int, PinMode> _sysFSModes = new Dictionary<int, PinMode>();

        protected override void Dispose(bool disposing)
        {
            if (_registerViewPointer != null)
            {
                Interop.munmap((IntPtr)_registerViewPointer, 0);
                _registerViewPointer = null;
            }
            if (_sysFSDriver != null)
            {
                _sysFSDriver.Dispose();
                _sysFSDriver = null;
            }
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

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            _sysFSDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void ClosePin(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            if (_sysFSModes.ContainsKey(pinNumber) && _sysFSModes[pinNumber] == PinMode.Output)
            {
                Write(pinNumber, PinValue.Low);
                SetPinMode(pinNumber, PinMode.Input);
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
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void OpenPin(int pinNumber)
        {
            ValidatePinNumber(pinNumber);
            Initialize();
            SetPinMode(pinNumber, PinMode.Input);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal unsafe override PinValue Read(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            /*
             * There are two registers that contain the value of a pin. Each hold the value of 32
             * different pins. 1 bit represents the value of a pin, 0 is PinValue.Low and 1 is PinValue.High
             */

            uint register = _registerViewPointer->GPLEV[pinNumber / 32];
            return Convert.ToBoolean((register >> (pinNumber % 32)) & 1) ? PinValue.High : PinValue.Low;
        }

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            _sysFSDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            ValidatePinNumber(pinNumber);

            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException($"The pin {pinNumber} does not support the selected mode {mode}.");
            }

            /*
             * There are 6 registers(4-byte ints) that control the mode for all pins. Each
             * register controls the mode for 10 pins. Each pin uses 3 bits in the register
             * containing the mode.
             */

            // Define the shift to get the right 3 bits in the register
            int shift = (pinNumber % 10) * 3;
            // Gets a pointer to the register that holds the mode for the pin
            uint* registerPointer = &_registerViewPointer->GPFSEL[pinNumber / 10];
            uint register = *registerPointer;
            // Clear the 3 bits to 0 for the pin Number.
            register &= ~(0b111U << shift);
            // Set the 3 bits to the desired mode for that pin.
            register |= (mode == PinMode.Output ? 1u : 0u) << shift;
            *registerPointer = register;

            if (_sysFSModes.ContainsKey(pinNumber))
            {
                _sysFSModes[pinNumber] = mode;
            }
            else
            {
                _sysFSModes.Add(pinNumber, mode);
            }

            if (mode != PinMode.Output)
            {
                SetInputPullMode(pinNumber, mode);
            }
        }

        /// <summary>
        /// Sets the resistor pull up/down mode for an input pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode of a pin to set the resistor pull up/down mode.</param>
        private void SetInputPullMode(int pinNumber, PinMode mode)
        {
            byte modeToPullMode = mode switch
            {
                PinMode.Input => (byte)0,
                PinMode.InputPullDown => (byte)1,
                PinMode.InputPullUp => (byte)2,
                _ => throw new ArgumentException($"{mode} is not supported as a pull up/down mode.")
            };

            /*
             * This is the process outlined by the BCM2835 datasheet on how to set the pull mode.
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

            uint* gppudPointer = &_registerViewPointer->GPPUD;
            uint register = *gppudPointer;
            register &= ~0b11U;
            register |= modeToPullMode;
            *gppudPointer = register;

            // Wait 150 cycles – this provides the required set-up time for the control signal
            Thread.SpinWait(150);

            int index = pinNumber / 32;
            int shift = pinNumber % 32;
            uint* gppudclkPointer = &_registerViewPointer->GPPUDCLK[index];
            register = *gppudclkPointer;
            register |= 1U << shift;
            *gppudclkPointer = register;

            // Wait 150 cycles – this provides the required hold time for the control signal
            Thread.SpinWait(150);

            register = *gppudPointer;
            register &= ~0b11U;
            *gppudPointer = register;
            *gppudclkPointer = 0;
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
            ValidatePinNumber(pinNumber);
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
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            return _sysFSDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal override void Write(int pinNumber, PinValue value)
        {
            ValidatePinNumber(pinNumber);

            /*
             * If the value is High, GPSET register is used. Otherwise, GPCLR will be used. For
             * both cases, a 1 is set on the corresponding bit in the register in order to set
             * the desired value.
             */

            uint* registerPointer = (value == PinValue.High) ? &_registerViewPointer->GPSET[pinNumber / 32] : &_registerViewPointer->GPCLR[pinNumber / 32];
            uint register = *registerPointer;
            register = 1U << (pinNumber % 32);
            *registerPointer = register;
        }

        protected ulong SetRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return *(ulong*)(_registerViewPointer->GPSET); }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {  *(ulong*)(_registerViewPointer->GPSET) = value; }
        }

        protected ulong ClearRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return *(ulong*)(_registerViewPointer->GPCLR); }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {  *(ulong*)(_registerViewPointer->GPCLR) = value; }
        }

        /// <summary>
        /// Returns the peripheral base address on the CPU bus of the raspberry pi based on the ranges set within the device tree.
        /// </summary>
        /// <remarks>
        /// The range examined in this method is essentially a mapping between where the peripheral base address on the videocore bus and its
        /// address on the cpu bus. The return value is 32bit (is in the first 4GB) even on 64 bit operating systems (debian / ubuntu tested) but may change in the future
        /// This method is based on bcm_host_get_peripheral_address() in libbcm_host which may not exist in all linux distributions.
        /// </remarks>
        /// <returns>This returns the peripheral base address as a 32 bit address or 0xFFFFFFFF when in error.</returns>
        private uint GetPeripheralBaseAddress()
        {
            uint cpuBusPeripheralBaseAddress = InvalidPeripheralBaseAddress;
            uint vcBusPeripheralBaseAddress;

            using (BinaryReader rdr = new BinaryReader(File.Open(DeviceTreeRanges, FileMode.Open, FileAccess.Read)))
            {
                // get the Peripheral Base Address on the VC bus from the device tree this is to be used to verify that
                // the right thing is being read and should always be 0x7E000000
                vcBusPeripheralBaseAddress = BinaryPrimitives.ReadUInt32BigEndian(rdr.ReadBytes(4));

                // get the Peripheral Base Address on the CPU bus from the device tree.
                cpuBusPeripheralBaseAddress = BinaryPrimitives.ReadUInt32BigEndian(rdr.ReadBytes(4));

                // if the CPU bus Peripheral Base Address is 0 then assume that this is a 64 bit address and so read the next 32 bits.
                if (cpuBusPeripheralBaseAddress == 0)
                {
                    cpuBusPeripheralBaseAddress = BinaryPrimitives.ReadUInt32BigEndian(rdr.ReadBytes(4));
                }

                // if the address values don't fall withing known values for the chipsets associated with the Pi2, Pi3 and Pi4 then assume an error
                // These addresses are coded into the device tree and the dts source for the device tree is within https://github.com/raspberrypi/linux/tree/rpi-4.19.y/arch/arm/boot/dts
                if (vcBusPeripheralBaseAddress != PeripheralBaseAddressVideocore || !(cpuBusPeripheralBaseAddress == PeripheralBaseAddressBcm2835 || cpuBusPeripheralBaseAddress == PeripheralBaseAddressBcm2836 || cpuBusPeripheralBaseAddress == PeripheralBaseAddressBcm2838))
                {
                    cpuBusPeripheralBaseAddress = InvalidPeripheralBaseAddress;
                }
            }

            return cpuBusPeripheralBaseAddress;
        }

        private void InitializeSysFS()
        {
            if (_sysFSDriver != null)
            {
                return;
            }
            lock (s_sysFsInitializationLock)
            {
                if (_sysFSDriver != null)
                {
                    return;
                }
                _sysFSDriver = new SysFsDriver();
            }
        }

        private void Initialize()
        {
            uint gpioRegisterOffset = 0;
            int fileDescriptor;
            int win32Error;

            if (_registerViewPointer != null)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (_registerViewPointer != null)
                {
                    return;
                }

                // try and open /dev/gpiomem
                fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    win32Error = Marshal.GetLastWin32Error();

                    // if the failure is NOT because /dev/gpiomem doesn't exist then throw an exception at this point.
                    // if it were anything else then it is probably best not to try and use /dev/mem on the basis that
                    // it would be better to solve the issue rather than use a method that requires root privileges
                    if (win32Error != ENOENT)
                    {
                        throw new IOException($"Error {win32Error} initializing the Gpio driver.");
                    }

                    // if /dev/gpiomem doesn't seem to be available then let's try /dev/mem
                    fileDescriptor = Interop.open(MemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                    if (fileDescriptor == -1)
                    {
                        throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                    }
                    else // success so set the offset into memory of the gpio registers
                    {
                        gpioRegisterOffset = InvalidPeripheralBaseAddress;

                        try
                        {
                            // get the periphal base address from the libbcm_host library which is the reccomended way
                            // according to the RasperryPi website
                            gpioRegisterOffset = Interop.libbcmhost.bcm_host_get_peripheral_address();

                            // if we get zero back then we use our own internal method. This can happen 
                            // on a Pi4 if the userland libraries haven't been updated and was fixed in Jul/Aug 2019.
                            if (gpioRegisterOffset == 0)
                            {
                                gpioRegisterOffset = GetPeripheralBaseAddress();
                            }
                        }
                        catch (DllNotFoundException)
                        {
                            // if the code gets here then then use our internal method as libbcm_host isn't available.
                            gpioRegisterOffset = GetPeripheralBaseAddress();
                        }

                        if (gpioRegisterOffset == InvalidPeripheralBaseAddress)
                        {
                            throw new InvalidOperationException("Error - Unable to determine peripheral base address.");
                        }

                        // add on the offset from the peripheral base address to point to the gpio registers
                        gpioRegisterOffset += GpioPeripheralOffset;
                    }
                }

                IntPtr mapPointer = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize, (MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE), MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)gpioRegisterOffset);
                if (mapPointer.ToInt64() == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                Interop.close(fileDescriptor);
                _registerViewPointer = (RegisterView*)mapPointer;
            }
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            if (!_sysFSModes.ContainsKey(pinNumber))
            {
                throw new InvalidOperationException("Can not get a pin mode of a pin that is not open.");
            }
            return _sysFSModes[pinNumber];
        }
    }
}
