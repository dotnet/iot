﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// A GPIO driver for the Raspberry Pi 3 or 4, running Raspbian (or, with some limitations, ubuntu)
/// </summary>
internal unsafe class RaspberryPi3LinuxDriver : GpioDriver
{
    private const int ENOENT = 2; // error indicates that an entity doesn't exist
    private const uint PeripheralBaseAddressBcm2835 = 0x2000_0000;
    private const uint PeripheralBaseAddressBcm2836 = 0x3F00_0000;
    private const uint PeripheralBaseAddressBcm2838 = 0xFE00_0000;
    private const uint PeripheralBaseAddressVideocore = 0x7E00_0000;
    private const uint InvalidPeripheralBaseAddress = 0xFFFF_FFFF;
    private const uint GpioPeripheralOffset = 0x0020_0000; // offset from the peripheral base address of the GPIO registers
    private const string GpioMemoryFilePath = "/dev/gpiomem";
    private const string MemoryFilePath = "/dev/mem";
    private const string DeviceTreeRanges = "/proc/device-tree/soc/ranges";
    private const string ModelFilePath = "/proc/device-tree/model";

    private static readonly object s_initializationLock = new object();

    private readonly PinState?[] _pinModes;
    private RegisterView* _registerViewPointer = null;

    private UnixDriver? _interruptDriver = null;

    private string? _detectedModel;

    public RaspberryPi3LinuxDriver()
    {
        _pinModes = new PinState[PinCount];
    }

    /// <summary>
    /// Raspberry Pi 3 has 28 GPIO pins.
    /// </summary>
    protected internal override int PinCount => 28;

    /// <summary>
    /// Returns true if this is a Raspberry Pi4
    /// </summary>
    private bool IsPi4
    {
        get;
        set;
    }

    private void ValidatePinNumber(int pinNumber)
    {
        if (pinNumber < 0 || pinNumber >= PinCount)
        {
            throw new ArgumentException("The specified pin number is invalid.", nameof(pinNumber));
        }
    }

    /// <summary>
    /// Converts a board pin number to the driver's logical numbering scheme.
    /// </summary>
    /// <param name="pinNumber">The board pin number to convert.</param>
    /// <returns>The pin number in the driver's logical numbering scheme.</returns>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        return pinNumber switch
        {
            3 => 2,
            5 => 3,
            7 => 4,
            8 => 14,
            10 => 15,
            11 => 17,
            12 => 18,
            13 => 27,
            15 => 22,
            16 => 23,
            18 => 24,
            19 => 10,
            21 => 9,
            22 => 25,
            23 => 11,
            24 => 8,
            26 => 7,
            27 => 0,
            28 => 1,
            29 => 5,
            31 => 6,
            32 => 12,
            33 => 13,
            35 => 19,
            36 => 16,
            37 => 26,
            38 => 20,
            40 => 21,
            _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
        };
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

        _interruptDriver!.OpenPin(pinNumber);
        _pinModes[pinNumber]!.InUseByInterruptDriver = true;
        _interruptDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
    }

    /// <summary>
    /// Closes an open pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    protected internal override void ClosePin(int pinNumber)
    {
        ValidatePinNumber(pinNumber);

        if (_pinModes[pinNumber]?.InUseByInterruptDriver ?? false)
        {
            _interruptDriver!.ClosePin(pinNumber);
        }

        _pinModes[pinNumber] = null;
    }

    /// <summary>
    /// Checks if a pin supports a specific mode.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="mode">The mode to check.</param>
    /// <returns>The status if the pin supports the mode.</returns>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => mode switch
    {
        PinMode.Input or PinMode.InputPullDown or PinMode.InputPullUp or PinMode.Output => true,
        _ => false,
    };

    /// <summary>
    /// Opens a pin in order for it to be ready to use.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    protected internal override void OpenPin(int pinNumber)
    {
        ValidatePinNumber(pinNumber);
        Initialize();
        GetPinModeFromHardware(pinNumber);
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

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber)
    {
        ValidatePinNumber(pinNumber);
        _interruptDriver!.Toggle(pinNumber);
    }

    /// <summary>
    /// Removes a handler for a pin value changed event.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        ValidatePinNumber(pinNumber);

        _interruptDriver!.OpenPin(pinNumber);
        _pinModes[pinNumber]!.InUseByInterruptDriver = true;

        _interruptDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
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

        if (_pinModes[pinNumber] != null)
        {
            _pinModes[pinNumber]!.CurrentPinMode = mode;
        }
        else
        {
            _pinModes[pinNumber] = new PinState(mode);
        }

        if (mode != PinMode.Output)
        {
            SetInputPullMode(pinNumber, mode);
        }
    }

    /// <summary>
    /// Gets the pin mode directly from the hardware. Assumes that its in a valid GPIO mode
    /// </summary>
    private PinMode GetPinModeFromHardware(int pinNumber)
    {
        ValidatePinNumber(pinNumber);

        RaspberryPi3Driver.AltMode altMode = GetAlternatePinMode(pinNumber);
        PinMode mode = altMode switch
        {
            RaspberryPi3Driver.AltMode.Output => PinMode.Output,
            RaspberryPi3Driver.AltMode.Input => PinMode.Input,
            _ => PinMode.Input
        };

        if (IsPi4 && mode == PinMode.Input)
        {
            int shift = (pinNumber & 0xf) << 1;
            uint bits = 0;

            // Read back the register
            var gpioReg = _registerViewPointer;
            bits = (gpioReg->GPPUPPDN[(pinNumber >> 4)]);
            bits &= (3u << shift);
            bits >>= shift;
            mode = bits switch
            {
                0 => PinMode.Input,
                1 => PinMode.InputPullUp,
                2 => PinMode.InputPullDown,
                _ => PinMode.Input,
            };
        }
        else
        {
            // Pi3. We can't detect the pull mode, since it cannot be read back according to the documentation
        }

        if (_pinModes[pinNumber] is object)
        {
            _pinModes[pinNumber]!.CurrentPinMode = mode;
        }
        else
        {
            _pinModes[pinNumber] = new PinState(mode);
        }

        return mode;
    }

    protected internal override void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue)
    {
        // On the Raspberry Pi, we can Write the out value even if the mode is something other than out. It will take effect once we change the mode
        Write(pinNumber, initialValue);
        SetPinMode(pinNumber, mode);
    }

    /// <summary>
    /// Sets the resistor pull up/down mode for an input pin.
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="mode">The mode of a pin to set the resistor pull up/down mode.</param>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private void SetInputPullMode(int pinNumber, PinMode mode)
    {
        /*
         * NoOptimization is needed to force wait time to be at least minimum required cycles.
         * Also to ensure that pointer operations optimizations won't be using any locals
         * which would introduce time period where multiple threads could override value set
         * to this register.
         */
        if (IsPi4)
        {
            SetInputPullModePi4(pinNumber, mode);
            return;
        }

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
        *gppudPointer &= ~0b11U;
        *gppudPointer |= modeToPullMode;

        // Wait 150 cycles – this provides the required set-up time for the control signal
        for (int i = 0; i < 150; i++)
        {
        }

        int index = pinNumber / 32;
        int shift = pinNumber % 32;
        uint* gppudclkPointer = &_registerViewPointer->GPPUDCLK[index];
        uint pinBit = 1U << shift;
        *gppudclkPointer |= pinBit;

        // Wait 150 cycles – this provides the required hold time for the control signal
        for (int i = 0; i < 150; i++)
        {
        }

        // Spec calls to reset clock after the control signal
        // Since context switch between those two instructions can potentially
        // change pull up/down value we reset the clock first.
        *gppudclkPointer &= ~pinBit;
        *gppudPointer &= ~0b11U;

        // This timeout is not documented in the spec
        // but lack of it is causing intermittent failures when
        // pull up/down is changed frequently.
        for (int i = 0; i < 150; i++)
        {
        }
    }

    /// <summary>
    /// Sets the resistor pull up/down mode for an input pin on the Raspberry Pi4.
    /// The above, complex method doesn't do anything on a Pi4 (it doesn't cause any harm, though)
    /// </summary>
    /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
    /// <param name="mode">The mode of a pin to set the resistor pull up/down mode.</param>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private void SetInputPullModePi4(int pinNumber, PinMode mode)
    {
        /*
         * NoOptimization is needed to force wait time to be at least minimum required cycles.
         * Also to ensure that pointer operations optimizations won't be using any locals
         * which would introduce time period where multiple threads could override value set
         * to this register.
         */
        int shift = (pinNumber & 0xf) << 1;
        uint bits = 0;
        uint pull = mode switch
        {
            PinMode.Input => 0,
            PinMode.InputPullUp => 1,
            PinMode.InputPullDown => 2,
            _ => 0,
        };

        var gpioReg = _registerViewPointer;
        bits = (gpioReg->GPPUPPDN[(pinNumber >> 4)]);
        bits &= ~(3u << shift);
        bits |= (pull << shift);
        gpioReg->GPPUPPDN[(pinNumber >> 4)] = bits;
        for (int i = 0; i < 150; i++)
        {
        }
    }

    /// <summary>
    /// Set the specified alternate mode for the given pin.
    /// Check the manual to know what each pin can do.
    /// </summary>
    /// <param name="pinNumber">Pin number in the logcal scheme of the driver</param>
    /// <param name="altPinMode">Alternate mode to set</param>
    /// <exception cref="NotSupportedException">This mode is not supported by this driver (or by the given pin)</exception>
    /// <remarks>The method is intended for usage by higher-level abstraction interfaces. User code should be very careful when using this method.</remarks>
    protected internal void SetAlternatePinMode(int pinNumber, RaspberryPi3Driver.AltMode altPinMode)
    {
        Initialize();
        ValidatePinNumber(pinNumber);

        /*
         * There are 6 registers (4-byte ints) that control the mode for all pins. Each
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
        uint modeBits = 0; // Default: Gpio input

        modeBits = altPinMode switch
        {
            RaspberryPi3Driver.AltMode.Input => 0b000,
            RaspberryPi3Driver.AltMode.Output => 0b001,
            RaspberryPi3Driver.AltMode.Alt0 => 0b100,
            RaspberryPi3Driver.AltMode.Alt1 => 0b101,
            RaspberryPi3Driver.AltMode.Alt2 => 0b110,
            RaspberryPi3Driver.AltMode.Alt3 => 0b111,
            RaspberryPi3Driver.AltMode.Alt4 => 0b011,
            RaspberryPi3Driver.AltMode.Alt5 => 0b010,
            _ => throw new InvalidOperationException($"Unknown Alternate pin mode value: {altPinMode}")
        };

        register |= (modeBits) << shift;
        *registerPointer = register;
    }

    /// <summary>
    /// Retrieve the current alternate pin mode for a given logical pin.
    /// This works also with closed pins.
    /// </summary>
    /// <param name="pinNumber">Pin number in the logical scheme of the driver</param>
    /// <returns>Current pin mode</returns>
    protected internal RaspberryPi3Driver.AltMode GetAlternatePinMode(int pinNumber)
    {
        Initialize();
        ValidatePinNumber(pinNumber);
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
        // get the three bits of the register
        register = (register >> shift) & 0b111;

        switch (register)
        {
            case 0b000:
                // Input
                return RaspberryPi3Driver.AltMode.Input;
            case 0b001:
                return RaspberryPi3Driver.AltMode.Output;
            case 0b100:
                return RaspberryPi3Driver.AltMode.Alt0;
            case 0b101:
                return RaspberryPi3Driver.AltMode.Alt1;
            case 0b110:
                return RaspberryPi3Driver.AltMode.Alt2;
            case 0b111:
                return RaspberryPi3Driver.AltMode.Alt3;
            case 0b011:
                return RaspberryPi3Driver.AltMode.Alt4;
            case 0b010:
                return RaspberryPi3Driver.AltMode.Alt5;
        }

        // This cannot happen.
        throw new InvalidOperationException("Invalid register value");
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

        _interruptDriver!.OpenPin(pinNumber);
        _pinModes[pinNumber]!.InUseByInterruptDriver = true;

        return _interruptDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);
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

        _interruptDriver!.OpenPin(pinNumber);
        _pinModes[pinNumber]!.InUseByInterruptDriver = true;

        return _interruptDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
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

    protected internal ulong SetRegister
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return *(ulong*)(_registerViewPointer->GPSET); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { *(ulong*)(_registerViewPointer->GPSET) = value; }
    }

    protected internal ulong ClearRegister
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return *(ulong*)(_registerViewPointer->GPCLR); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { *(ulong*)(_registerViewPointer->GPCLR) = value; }
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

    private void InitializeInterruptDriver()
    {
        try
        {
            _interruptDriver = new LibGpiodDriver(0);
        }
        catch (PlatformNotSupportedException)
        {
            _interruptDriver = new InterruptSysFsDriver(this);
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

            // Detect whether we're running on a Raspberry Pi 4
            IsPi4 = false;
            try
            {
                if (File.Exists(ModelFilePath))
                {
                    string model = File.ReadAllText(ModelFilePath, Text.Encoding.ASCII);
                    if (model.Contains("Raspberry Pi 4") || model.Contains("Raspberry Pi Compute Module 4"))
                    {
                        IsPi4 = true;
                    }

                    _detectedModel = model;
                }
            }
            catch (Exception x)
            {
                // This should not normally fail, but we currently don't know how this behaves on different operating systems. Therefore, we ignore
                // any exceptions in release and just continue as Pi3 if something fails.
                // If in debug mode, we might want to check what happened here (i.e unsupported OS, incorrect permissions)
                Debug.Fail($"Unexpected exception: {x}");
            }

            InitializeInterruptDriver();
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

        var entry = _pinModes[pinNumber];
        if (entry == null)
        {
            throw new InvalidOperationException("Can not get a pin mode of a pin that is not open.");
        }

        return entry.CurrentPinMode;
    }

    protected override void Dispose(bool disposing)
    {
        if (_registerViewPointer != null)
        {
            Interop.munmap((IntPtr)_registerViewPointer, 0);
            _registerViewPointer = null;
        }

        _interruptDriver?.Dispose();
        _interruptDriver = null;
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        StringBuilder sb = new StringBuilder();
        Initialize();
        if (_detectedModel != null)
        {
            sb.Append(_detectedModel);
        }
        else
        {
            sb.Append($"Raspberry Pi {(IsPi4 ? "4" : "3")}");
        }

        sb.Append($" linux driver with {PinCount} pins");
        if (_interruptDriver != null)
        {
            sb.Append(" and an interrupt driver");
        }

        ComponentInformation ci = new ComponentInformation(this, sb.ToString());
        ci.Properties["Model"] = _detectedModel ?? string.Empty;

        if (_interruptDriver != null)
        {
            ci.AddSubComponent(_interruptDriver.QueryComponentInformation());
        }

        return ci;
    }

    private class PinState
    {
        public PinState(PinMode currentMode)
        {
            CurrentPinMode = currentMode;
            InUseByInterruptDriver = false;
        }

        public PinMode CurrentPinMode
        {
            get;
            set;
        }

        public bool InUseByInterruptDriver
        {
            get;
            set;
        }
    }
}
