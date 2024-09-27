// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Base class for Mcp23xxx GPIO expanders
    /// </summary>
    public abstract partial class Mcp23xxx : GpioDriver
    {
        private readonly int _reset;
        private readonly int _interruptA;
        private readonly int _interruptB;
        private readonly Dictionary<int, PinValue> _pinValues = new Dictionary<int, PinValue>();
        private readonly ConcurrentDictionary<int, PinChangeEventHandler> _eventHandlers = new ConcurrentDictionary<int, PinChangeEventHandler>();
        private BankStyle _bankStyle;
        private GpioController? _controller;
        private bool _shouldDispose;

        /// <summary>
        /// Bus adapter (I2C/SPI) used to communicate with the device
        /// </summary>
        protected BusAdapter _bus;

        private bool _increments = true;

        private ushort _gpioCache;
        private bool _cacheValid;
        private bool _disabled;

        private object _interruptHandlerLock = new object();

        private byte[] _interruptPins;
        private byte[] _interruptLastInputValues;

        /// <summary>
        /// A general purpose parallel I/O expansion for I2C or SPI applications.
        /// </summary>
        /// <param name="bus">The bus the device is connected to.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        /// <param name="gpioController">
        /// The controller for the reset and interrupt pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="bankStyle">
        /// The current bank style of the ports. This does not set the bank style- it tells us what the bank style is.
        /// It is *highly* recommended not to change the bank style from the default as there is no direct way to
        /// detect what style the chip is in and most apps will fail if the chip is not set to defaults. This setting
        /// has no impact on 8-bit expanders.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        protected Mcp23xxx(BusAdapter bus, int reset = -1, int interruptA = -1, int interruptB = -1,
            GpioController? gpioController = null, BankStyle bankStyle = BankStyle.Sequential, bool shouldDispose = true)
        {
            _bus = bus;
            _bankStyle = bankStyle;

            _reset = reset;
            _interruptA = interruptA;
            _interruptB = interruptB;

            _interruptPins = new byte[2];
            _interruptLastInputValues = new byte[2];

            // Only need master controller if there are external pins provided.
            if (_reset != -1 || _interruptA != -1 || _interruptB != -1)
            {
                _shouldDispose = shouldDispose || gpioController is null;
                _controller = gpioController ?? new GpioController();

                if (_interruptA != -1)
                {
                    _controller.OpenPin(_interruptA, PinMode.Input);
                }

                if (_interruptB != -1)
                {
                    _controller.OpenPin(_interruptB, PinMode.Input);
                }

                if (_reset != -1)
                {
                    _controller.OpenPin(_reset, PinMode.Output);
                    Disable();
                }
            }

            if (!_disabled)
            {
                // Set all of the pins to input, GPIO outputs to low, and set input polarity to match the input.
                // This is the normal power-on / reset state of the Mcp23xxx chips.
                if (PinCount == 8)
                {
                    InternalWriteByte(Register.IODIR, 0xFF, Port.PortA);
                    InternalWriteByte(Register.GPIO, 0x00, Port.PortA);
                    InternalWriteByte(Register.IPOL, 0x00, Port.PortA);
                }
                else
                {
                    InternalWriteUInt16(Register.IODIR, 0xFFFF);
                    InternalWriteUInt16(Register.GPIO, 0x0000);
                    InternalWriteUInt16(Register.IPOL, 0x0000);
                }
            }
        }

        private void UpdateCache()
        {
            // Ouput state is read from the latches (OLAT)
            _gpioCache = PinCount == 8
                ? InternalReadByte(Register.OLAT, Port.PortA)
                : InternalReadUInt16(Register.OLAT);
            _cacheValid = true;
        }

        /// <summary>
        /// Reads a number of bytes from registers.
        /// </summary>
        /// <param name="register">The register to read from.</param>
        /// <param name="buffer">The buffer to read bytes into.</param>
        /// <param name="port">The I/O port used with the register.</param>
        protected void InternalRead(Register register, Span<byte> buffer, Port port)
        {
            if (_disabled)
            {
                ThrowDisabled();
            }

            _bus.Read(GetMappedAddress(register, port, _bankStyle), buffer);
        }

        /// <summary>
        /// Writes a number of bytes to registers.
        /// </summary>
        /// <param name="register">The register address to write to.</param>
        /// <param name="data">The data to write to the registers.</param>
        /// <param name="port">The I/O port used with the registers.</param>
        protected void InternalWrite(Register register, Span<byte> data, Port port)
        {
            if (_disabled)
            {
                ThrowDisabled();
            }

            _bus.Write(GetMappedAddress(register, port, _bankStyle), data);
        }

        // Keeping this a separate method to allow the Read/Write methods to inline
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowDisabled() => throw new InvalidOperationException("Chip is disabled");

        /// <summary>
        /// Reads byte from the device register
        /// </summary>
        /// <param name="register">Register to read the value from</param>
        /// <param name="port">Port related with the <paramref name="register"/></param>
        /// <returns>Byte read from the device register</returns>
        protected byte InternalReadByte(Register register, Port port)
        {
            Span<byte> buffer = stackalloc byte[1];
            InternalRead(register, buffer, port);
            return buffer[0];
        }

        /// <summary>
        /// Write byte to device register
        /// </summary>
        /// <param name="register">Register to write the value to</param>
        /// <param name="value">Value to be written to the <paramref name="register"/></param>
        /// <param name="port">Port related with the <paramref name="register"/></param>
        protected void InternalWriteByte(Register register, byte value, Port port)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = value;
            InternalWrite(register, buffer, port);
        }

        /// <summary>
        /// Read a byte from the given register.
        /// </summary>
        /// <remarks>
        /// Reads from the A port registers on 16 bit devices.
        /// </remarks>
        public byte ReadByte(Register register) => InternalReadByte(register, Port.PortA);

        /// <summary>
        /// Write a byte to the given register.
        /// </summary>
        /// <remarks>
        /// Writes to the A port registers on 16 bit devices.
        /// </remarks>
        public void WriteByte(Register register, byte value) => InternalWriteByte(register, value, Port.PortA);

        /// <summary>
        /// Read 16-bit unsigned integer from the device register
        /// </summary>
        /// <param name="register">Register to read the value from</param>
        /// <returns>16-bit unsigned integer read from the device</returns>
        protected ushort InternalReadUInt16(Register register)
        {
            Span<byte> buffer = stackalloc byte[2];
            if (_increments)
            {
                // Can read both bytes at the same time
                InternalRead(register, buffer, Port.PortA);
            }
            else
            {
                // Have to read each separately
                InternalRead(register, buffer.Slice(0, 1), Port.PortA);
                InternalRead(register, buffer.Slice(1), Port.PortB);
            }

            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        /// <summary>
        /// Writes 16-bit unsigned integer to the device register
        /// </summary>
        /// <param name="register">Register to write <paramref name="value"/> to</param>
        /// <param name="value">16-bit unsigned integer to write to the <paramref name="register"/></param>
        protected void InternalWriteUInt16(Register register, ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);

            if (_increments)
            {
                // Can write both at the same time
                InternalWrite(register, buffer, Port.PortA);
            }
            else
            {
                // Have to write each separately
                InternalWrite(register, buffer.Slice(0, 1), Port.PortA);
                InternalWrite(register, buffer.Slice(1), Port.PortB);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;

                _pinValues.Clear();
                _bus?.Dispose();
                _bus = null!;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Disables the device by setting the reset pin low.
        /// </summary>
        public void Disable()
        {
            if (_reset == -1 || _controller is null)
            {
                throw new InvalidOperationException("No reset pin configured.");
            }

            _controller.Write(_reset, PinValue.Low);

            // Registers will all be reset when re-enabled
            _bankStyle = BankStyle.Sequential;
            _increments = true;
            _disabled = true;
        }

        /// <summary>
        /// Enables the device by setting the reset pin high.
        /// </summary>
        public void Enable()
        {
            if (_reset == -1 || _controller is null)
            {
                throw new InvalidOperationException("No reset pin configured.");
            }

            _controller.Write(_reset, PinValue.High);

            _disabled = false;
            _cacheValid = false;
        }

        /// <summary>
        /// Reads interrupt value
        /// </summary>
        /// <param name="port">Port to read interrupt on</param>
        /// <returns>Value of interrupt pin</returns>
        protected PinValue InternalReadInterrupt(Port port)
        {
            int pinNumber = port switch
            {
                Port.PortA => _interruptA,
                Port.PortB => _interruptB,
                _ => throw new ArgumentOutOfRangeException(nameof(port)),
            };

            if (pinNumber == -1 || _controller is null)
            {
                throw new ArgumentException("No interrupt pin configured.", nameof(port));
            }

            return _controller.Read(pinNumber);
        }

        /// <summary>
        /// Returns the value of the interrupt pin if configured.
        /// </summary>
        /// <returns>
        /// Returns the interrupt for port A on 16 bit devices.
        /// </returns>
        public PinValue ReadInterrupt() => InternalReadInterrupt(Port.PortA);

        private byte SetBit(byte data, int bitNumber) => (byte)(data | (1 << bitNumber));

        private byte ClearBit(byte data, int bitNumber) => (byte)(data & ~(1 << bitNumber));

        /// <summary>
        /// Sets a mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="mode">The mode to be set.</param>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            lock (_interruptHandlerLock)
            {
                if (mode != PinMode.Input && mode != PinMode.Output && mode != PinMode.InputPullUp)
                {
                    throw new ArgumentException("The Mcp controller supports the following pin modes: Input, Output and InputPullUp.");
                }

                ValidatePin(pinNumber);

                Port port = GetPortForPinNumber(pinNumber);
                if (port == Port.PortB)
                {
                    pinNumber -= 8;
                }

                byte value;
                if (mode == PinMode.Output)
                {
                    value = ClearBit(InternalReadByte(Register.IODIR, port), pinNumber);
                }
                else
                {
                    value = SetBit(InternalReadByte(Register.IODIR, port), pinNumber);
                }

                InternalWriteByte(Register.IODIR, value, port);

                byte value2;
                if (mode == PinMode.InputPullUp)
                {
                    value2 = SetBit(InternalReadByte(Register.GPPU, port), pinNumber);
                }
                else
                {
                    value2 = ClearBit(InternalReadByte(Register.GPPU, port), pinNumber);
                }

                InternalWriteByte(Register.GPPU, value2, port);
            }
        }

        /// <summary>
        /// Reads the value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <returns>High or low pin value.</returns>
        protected override PinValue Read(int pinNumber)
        {
            lock (_interruptHandlerLock)
            {
                ValidatePin(pinNumber);
                Span<PinValuePair> pinValuePairs = stackalloc PinValuePair[]
                {
                    new PinValuePair(pinNumber, default)
                };
                Read(pinValuePairs);
                _pinValues[pinNumber] = pinValuePairs[0].PinValue;
                return _pinValues[pinNumber];
            }
        }

        /// <inheritdoc/>
        protected override void Toggle(int pinNumber) => Write(pinNumber, !_pinValues[pinNumber]);

        /// <summary>
        /// Reads the value of a set of pins
        /// </summary>
        protected void Read(Span<PinValuePair> pinValuePairs)
        {
            lock (_interruptHandlerLock)
            {
                (uint pins, _) = new PinVector32(pinValuePairs);
                if ((pins >> PinCount) > 0)
                {
                    ThrowBadPin(nameof(pinValuePairs));
                }

                ushort result;
                if (pins < 0xFF + 1)
                {
                    // Only need to get the first 8 pins (PortA)
                    result = InternalReadByte(Register.GPIO, Port.PortA);
                }
                else if ((pins & 0xFF) == 0)
                {
                    // Only need to get the second 8 pins (PortB)
                    result = (ushort)(InternalReadByte(Register.GPIO, Port.PortB) << 8);
                }
                else
                {
                    // Need to get both
                    result = InternalReadUInt16(Register.GPIO);
                }

                for (int i = 0; i < pinValuePairs.Length; i++)
                {
                    int pin = pinValuePairs[i].PinNumber;
                    pinValuePairs[i] = new PinValuePair(pin, result & (1 << pin));
                    _pinValues[pin] = pinValuePairs[i].PinValue;
                }
            }
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The value to be written.</param>
        protected override void Write(int pinNumber, PinValue value)
        {
            lock (_interruptHandlerLock)
            {
                ValidatePin(pinNumber);
                Span<PinValuePair> pinValuePairs = stackalloc PinValuePair[]
                {
                    new PinValuePair(pinNumber, value)
                };
                Write(pinValuePairs);
                _pinValues[pinNumber] = value;
            }
        }

        /// <summary>
        /// Writes values to a set of pins
        /// </summary>
        protected void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
        {
            lock (_interruptHandlerLock)
            {
                (uint mask, uint newBits) = new PinVector32(pinValuePairs);
                if ((mask >> PinCount) > 0)
                {
                    ThrowBadPin(nameof(pinValuePairs));
                }

                if (!_cacheValid)
                {
                    UpdateCache();
                }

                ushort cachedValue = _gpioCache;
                ushort newValue = SetBits(cachedValue, (ushort)newBits, (ushort)mask);
                if (cachedValue == newValue)
                {
                    return;
                }

                if (mask < 0xFF + 1)
                {
                    // Only need to change the first 8 pins (PortA)
                    InternalWriteByte(Register.GPIO, (byte)newValue, Port.PortA);
                }
                else if ((mask & 0xFF) == 0)
                {
                    // Only need to change the second 8 pins (PortB)
                    InternalWriteByte(Register.GPIO, (byte)(newValue >> 8), Port.PortB);
                }
                else
                {
                    // Need to change both
                    InternalWriteUInt16(Register.GPIO, newValue);
                }

                _gpioCache = newValue;
                foreach (PinValuePair pinValuePair in pinValuePairs)
                {
                    _pinValues[pinValuePair.PinNumber] = pinValuePair.PinValue;
                }
            }
        }

        private ushort SetBits(ushort current, ushort bits, ushort mask)
        {
            current &= (ushort)~mask;
            current |= bits;
            return current;
        }

        private void ValidatePin(int pinNumber)
        {
            if (pinNumber >= PinCount || pinNumber < 0)
            {
                ThrowBadPin(nameof(pinNumber));
            }
        }

        private void ThrowBadPin(string argument)
        {
            throw new ArgumentOutOfRangeException(argument, $"Only pins {0} through {PinCount - 1} are valid.");
        }

        /// <summary>
        /// Gets the mapped address for a register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="port">The bank of I/O ports used with the register.</param>
        /// <param name="bankStyle">The bank style that determines how the register addresses are grouped.</param>
        /// <returns>The byte address of the register for the given port bank and bank style.</returns>
        private byte GetMappedAddress(Register register, Port port = Port.PortA,
            BankStyle bankStyle = BankStyle.Sequential)
        {
            if (port != Port.PortA && port != Port.PortB)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (bankStyle != BankStyle.Separated && bankStyle != BankStyle.Sequential)
            {
                throw new ArgumentOutOfRangeException(nameof(bankStyle));
            }

            byte address = (byte)register;

            // There is no mapping for 8 bit expanders
            if (PinCount == 8)
            {
                return address;
            }

            if (bankStyle == BankStyle.Sequential)
            {
                // Registers for each bank are sequential
                // (IODIRA = 0x00, IODIRB = 0x01, IPOLA = 0x02, IPOLB = 0x03, ...)
                address += address;
                return port == Port.PortA ? address : ++address;
            }

            // Registers for each bank are separated
            // (IODIRA = 0x00, ... OLATA = 0x0A, IODIRB = 0x10, ... OLATB = 0x1A)
            return port == Port.PortA ? address : address += 0x10;
        }

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            // No-op
            _pinValues.TryAdd(pinNumber, PinValue.Low);
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            // No-op
            _pinValues.Remove(pinNumber);
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            ValidatePin(pinNumber);

            // IsBitSet returns true if bitNumber is flipped on in data.
            bool IsBitSet(byte data, int bitNumber) => (data & (1 << bitNumber)) != 0;

            if (pinNumber < 8)
            {
                return IsBitSet(InternalReadByte(Register.IODIR, Port.PortA), pinNumber)
                    ? PinMode.Input
                    : PinMode.Output;
            }
            else
            {
                return IsBitSet(InternalReadByte(Register.IODIR, Port.PortB), pinNumber - 8)
                    ? PinMode.Input
                    : PinMode.Output;
            }
        }

        /// <summary>
        /// Enables interrupts for a specified pin. On 16-Pin devices, Pins 0-7 trigger the INTA pin and Pins 8-15
        /// trigger the INTB pin. The interrupt signals are configured as active-low.
        /// </summary>
        /// <param name="pinNumber">The pin number for which an interrupt shall be triggered</param>
        /// <param name="eventTypes">Event(s) that should trigger the interrupt on the given pin</param>
        /// <exception cref="ArgumentException">EventTypes is not valid (must have at least one event type selected)</exception>
        /// <remarks>After calling this method, call <see cref="Read(int)"/> once to make sure the interrupt flag for the given port is cleared</remarks>
        public void EnableInterruptOnChange(int pinNumber, PinEventTypes eventTypes)
        {
            ValidatePin(pinNumber);
            byte oldValue, newValue;
            lock (_interruptHandlerLock)
            {
                if (eventTypes == PinEventTypes.None)
                {
                    throw new ArgumentException("No event type specified");
                }

                Port port = Port.PortA;
                if (pinNumber >= 8)
                {
                    pinNumber -= 8;
                    port = Port.PortB;
                }

                // Set the corresponding bit in the GPINTEN (Interrupt-on-Change) register
                oldValue = InternalReadByte(Register.GPINTEN, port);
                newValue = SetBit(oldValue, pinNumber);
                InternalWriteByte(Register.GPINTEN, newValue, port);
                oldValue = InternalReadByte(Register.INTCON, port);
                // If the interrupt shall happen on either edge, we clear the INTCON (Interrupt-on-Change-Control) register,
                // which will trigger an interrupt on every change. Otherwise, set the INTCON register bit and set the
                // DefVal register.
                if (eventTypes == (PinEventTypes.Falling | PinEventTypes.Rising))
                {
                    newValue = ClearBit(oldValue, pinNumber);
                }
                else
                {
                    newValue = SetBit(oldValue, pinNumber);
                }

                InternalWriteByte(Register.INTCON, newValue, port);

                oldValue = InternalReadByte(Register.DEFVAL, port);
                // If we clear the bit, the interrupt occurs on a rising edge, if we set it, it occurs on a falling edge.
                // If INTCON is clear, the value is ignored.
                if (eventTypes == PinEventTypes.Rising)
                {
                    newValue = ClearBit(oldValue, pinNumber);
                }
                else
                {
                    newValue = SetBit(oldValue, pinNumber);
                }

                InternalWriteByte(Register.DEFVAL, newValue, port);

                // Finally make sure that IOCON.ODR is low and IOCON.INTPOL is low, too (interrupt is low-active, the default)
                // For this register, it doesn't matter which port we use, it exists only once.
                oldValue = InternalReadByte(Register.IOCON, Port.PortA);
                newValue = ClearBit(oldValue, 1);
                newValue = ClearBit(newValue, 2);
                InternalWriteByte(Register.IOCON, newValue, Port.PortA);

                _interruptPins[(int)port] = SetBit(_interruptPins[(int)port], pinNumber);
                _interruptLastInputValues[(int)port] = InternalReadByte(Register.GPIO, port);
            }
        }

        private static Port GetPortForPinNumber(int pinNumber)
        {
            Port port = Port.PortA;
            if (pinNumber >= 8)
            {
                port = Port.PortB;
            }

            return port;
        }

        /// <summary>
        /// Disables triggering interrupts on a certain pin
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        public void DisableInterruptOnChange(int pinNumber)
        {
            ValidatePin(pinNumber);
            byte oldValue, newValue;
            lock (_interruptHandlerLock)
            {
                if (pinNumber < 8)
                {
                    // Set the corresponding bit in the GPINTEN (Interrupt-on-Change) register
                    oldValue = InternalReadByte(Register.GPINTEN, Port.PortA);
                    newValue = ClearBit(oldValue, pinNumber);
                    InternalWriteByte(Register.GPINTEN, newValue, Port.PortA);
                    _interruptPins[0] = ClearBit(_interruptPins[0], pinNumber);
                }
                else
                {
                    oldValue = InternalReadByte(Register.GPINTEN, Port.PortB);
                    newValue = ClearBit(oldValue, pinNumber - 8);
                    InternalWriteByte(Register.GPINTEN, newValue, Port.PortB);
                    _interruptPins[1] = ClearBit(_interruptPins[1], pinNumber - 8);
                }
            }
        }

        private void InterruptHandler(object sender, PinValueChangedEventArgs e)
        {
            Port port;
            int interruptPending;
            int newValues;

            lock (_interruptHandlerLock)
            {
                port = e.PinNumber == _interruptA ? Port.PortA : Port.PortB;

                // It seems that this register has at most 1 bit set - the one that triggered the interrupt.
                // If another pin which has interrupt handling enabled changes until we clear the interrupt flag, that
                // interrupt is lost.
                int pinThatCausedInterrupt = InternalReadByte(Register.INTF, port);
                newValues = InternalReadByte(Register.GPIO, port);

                interruptPending = (newValues ^ _interruptLastInputValues[(int)port]) & _interruptPins[(int)port]; // Which values changed?
                interruptPending |= pinThatCausedInterrupt; // this one certainly did (even if the value is now the same)
                _interruptLastInputValues[(int)port] = (byte)newValues;
            }

            int offset = 0;
            if (port == Port.PortB)
            {
                offset = 8;
            }

            int mask = 1;
            int pin = 0;

            while (mask < 0x10)
            {
                if ((interruptPending & mask) != 0)
                {
                    CallHandlerOnPin(pin + offset, newValues & mask);
                }

                mask = mask << 1;
                pin++;
            }
        }

        /// <summary>
        /// Calls the event handler for the given pin, if any.
        /// </summary>
        /// <param name="pin">Pin to call the event handler on (if any exists)</param>
        /// <param name="valueFlag">Non-zero if the value is currently high (therefore assuming the pin value was rising), otherwise zero</param>
        private void CallHandlerOnPin(int pin, int valueFlag)
        {
            if (_eventHandlers.TryGetValue(pin, out var handler))
            {
                handler.Invoke(this, new PinValueChangedEventArgs(valueFlag != 0 ? PinEventTypes.Rising : PinEventTypes.Falling, pin));
            }
        }

        /// <summary>
        /// Calls an event handler if the given pin changes.
        /// </summary>
        /// <param name="pinNumber">Pin number of the MCP23xxx</param>
        /// <param name="eventTypes">Whether the handler should trigger on rising, falling or both edges</param>
        /// <param name="callback">The method to call when an interrupt is triggered</param>
        /// <exception cref="InvalidOperationException">There's no GPIO controller for the master interrupt configured, or no interrupt lines are configured for the
        /// required port.</exception>
        /// <remarks>Only one event handler can be registered per pin. Calling this again with a different handler for the same pin replaces the handler</remarks>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes,
            PinChangeEventHandler callback)
        {
            if (_controller == null)
            {
                throw new InvalidOperationException("No GPIO controller available. Specify a GPIO controller and the relevant interrupt line numbers in the constructor");
            }

            EnableInterruptOnChange(pinNumber, eventTypes);
            Port port = GetPortForPinNumber(pinNumber);
            if (port == Port.PortA)
            {
                if (_interruptA < 0)
                {
                    throw new InvalidOperationException("No GPIO pin defined for interrupt line A. Please specify an interrupt line in the constructor.");
                }

                if (!_eventHandlers.Any(x => x.Key <= 7))
                {
                    _controller.RegisterCallbackForPinValueChangedEvent(_interruptA, PinEventTypes.Falling, InterruptHandler);
                }

                _eventHandlers[pinNumber] = callback;
                InternalReadByte(Register.GPIO, Port.PortA); // Clear the interrupt flags
            }
            else
            {
                if (_interruptB < 0)
                {
                    throw new InvalidOperationException("No GPIO pin defined for interrupt line B. Please specify an interrupt line in the constructor.");
                }

                if (!_eventHandlers.Any(x => x.Key >= 8))
                {
                    _controller.RegisterCallbackForPinValueChangedEvent(_interruptB, PinEventTypes.Falling, InterruptHandler);
                }

                _eventHandlers[pinNumber] = callback;
                InternalReadByte(Register.GPIO, Port.PortB); // Clear the interrupt flags
            }
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            if (_controller == null)
            {
                // If we had any callbacks registered, this would have thrown up earlier.
                throw new InvalidOperationException("No valid GPIO controller defined. And no callbacks registered either.");
            }

            if (_eventHandlers.TryRemove(pinNumber, out _))
            {
                Port port = GetPortForPinNumber(pinNumber);
                if (port == Port.PortA)
                {
                    if (!_eventHandlers.Any(x => x.Key <= 7))
                    {
                        _controller.UnregisterCallbackForPinValueChangedEvent(_interruptA, InterruptHandler);
                    }
                }
                else
                {
                    if (!_eventHandlers.Any(x => x.Key >= 8))
                    {
                        _controller.UnregisterCallbackForPinValueChangedEvent(_interruptB, InterruptHandler);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        /// <summary>
        /// Waits for an event to occur on the given pin.
        /// </summary>
        /// <param name="pinNumber">The pin on which to wait</param>
        /// <param name="eventTypes">The event to wait for (rising, falling or either)</param>
        /// <param name="cancellationToken">A timeout token</param>
        /// <returns>The wait result</returns>
        /// <remarks>This method should only be used on pins that are not otherwise used in event handling, as it clears any
        /// existing event handlers for the same pin.</remarks>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes,
            CancellationToken cancellationToken)
        {
            ManualResetEventSlim slim = new ManualResetEventSlim();
            slim.Reset();
            PinEventTypes eventTypes1 = PinEventTypes.None;
            void InternalHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
            {
                if (pinValueChangedEventArgs.PinNumber != pinNumber)
                {
                    return;
                }

                if ((pinValueChangedEventArgs.ChangeType & eventTypes) != 0)
                {
                    slim.Set();
                }

                eventTypes1 = pinValueChangedEventArgs.ChangeType;
            }

            AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, InternalHandler);
            slim.Wait(cancellationToken);
            RemoveCallbackForPinValueChangedEvent(pinNumber, InternalHandler);

            if (cancellationToken.IsCancellationRequested)
            {
                return new WaitForEventResult()
                {
                    EventTypes = PinEventTypes.None, TimedOut = true
                };
            }

            return new WaitForEventResult()
            {
                EventTypes = eventTypes1, TimedOut = false
            };
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) =>
            (mode == PinMode.Input || mode == PinMode.Output || mode == PinMode.InputPullUp);
    }
}
