// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
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
        private BankStyle _bankStyle;
        private GpioController _masterGpioController;

        /// <summary>
        /// Bus adapter (I2C/SPI) used to communicate with the device
        /// </summary>
        protected BusAdapter _bus;

        private bool _increments = true;

        private ushort _gpioCache;
        private bool _cacheValid;
        private bool _disabled;

        /// <summary>
        /// A general purpose parallel I/O expansion for I2C or SPI applications.
        /// </summary>
        /// <param name="bus">The bus the device is connected to.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        /// <param name="masterController">
        /// The controller for the reset and interrupt pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="bankStyle">
        /// The current bank style of the ports. This does not set the bank style- it tells us what the bank style is.
        /// It is *highly* recommended not to change the bank style from the default as there is no direct way to
        /// detect what style the chip is in and most apps will fail if the chip is not set to defaults. This setting
        /// has no impact on 8-bit expanders.
        /// </param>
        protected Mcp23xxx(BusAdapter bus, int reset = -1, int interruptA = -1, int interruptB = -1,
            GpioController masterController = null, BankStyle bankStyle = BankStyle.Sequential)
        {
            _bus = bus;
            _bankStyle = bankStyle;

            _reset = reset;
            _interruptA = interruptA;
            _interruptB = interruptB;

            // Only need master controller if there are external pins provided.
            if (_reset != -1 || _interruptA != -1 || _interruptB != -1)
            {
                _masterGpioController = masterController ?? new GpioController();

                if (_interruptA != -1)
                {
                    _masterGpioController.OpenPin(_interruptA, PinMode.Input);
                }

                if (_interruptB != -1)
                {
                    _masterGpioController.OpenPin(_interruptB, PinMode.Input);
                }

                if (_reset != -1)
                {
                    _masterGpioController.OpenPin(_reset, PinMode.Output);
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
        /// Writes to the A port registers on 16 bit devices.
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
            _masterGpioController?.Dispose();
            _masterGpioController = null;
            _bus?.Dispose();
            _bus = null;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Disables the device by setting the reset pin low.
        /// </summary>
        public void Disable()
        {
            if (_reset == -1)
            {
                throw new InvalidOperationException("No reset pin configured.");
            }

            _masterGpioController.Write(_reset, PinValue.Low);

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
            if (_reset == -1)
            {
                throw new InvalidOperationException("No reset pin configured.");
            }

            _masterGpioController.Write(_reset, PinValue.High);

            _disabled = false;
            _cacheValid = false;
        }

        /// <summary>
        /// Reads interrupt value
        /// </summary>
        /// <param name="port">Port to read interrupt on</param>
        /// <returns>Value of intterupt pin</returns>
        protected PinValue InternalReadInterrupt(Port port)
        {
            int pinNumber;
            switch (port)
            {
                case Port.PortA:
                    pinNumber = _interruptA;
                    break;
                case Port.PortB:
                    pinNumber = _interruptB;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (pinNumber == -1)
            {
                throw new ArgumentException("No interrupt pin configured.", nameof(port));
            }

            return _masterGpioController.Read(pinNumber);
        }

        /// <summary>
        /// Returns the value of the interrupt pin if configured.
        /// </summary>
        /// <returns>
        /// Returns the interrupt for port A on 16 bit devices.
        /// </returns>
        public PinValue ReadInterrupt() => InternalReadInterrupt(Port.PortA);

        /// <summary>
        /// Sets a mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="mode">The mode to be set.</param>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (mode != PinMode.Input && mode != PinMode.Output)
            {
                throw new ArgumentException("The Mcp controller supports Input and Output modes only.");
            }

            ValidatePin(pinNumber);

            byte SetBit(byte data, int bitNumber) => (byte)(data | (1 << bitNumber));

            byte ClearBit(byte data, int bitNumber) => (byte)(data & ~(1 << bitNumber));

            if (pinNumber < 8)
            {
                byte value = mode == PinMode.Output
                    ? ClearBit(InternalReadByte(Register.IODIR, Port.PortA), pinNumber)
                    : SetBit(InternalReadByte(Register.IODIR, Port.PortA), pinNumber);
                InternalWriteByte(Register.IODIR, value, Port.PortA);
            }
            else
            {
                byte value = mode == PinMode.Output
                    ? ClearBit(InternalReadByte(Register.IODIR, Port.PortB), pinNumber - 8)
                    : SetBit(InternalReadByte(Register.IODIR, Port.PortB), pinNumber - 8);
                InternalWriteByte(Register.IODIR, value, Port.PortB);
            }
        }

        /// <summary>
        /// Reads the value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <returns>High or low pin value.</returns>
        protected override PinValue Read(int pinNumber)
        {
            ValidatePin(pinNumber);
            Span<PinValuePair> pinValuePairs = stackalloc PinValuePair[]
            {
                new PinValuePair(pinNumber, default)
            };
            Read(pinValuePairs);
            return pinValuePairs[0].PinValue;
        }

        /// <summary>
        ///Reads the value of a set of pins
        /// </summary>
        protected void Read(Span<PinValuePair> pinValuePairs)
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
            }
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The value to be written.</param>
        protected override void Write(int pinNumber, PinValue value)
        {
            ValidatePin(pinNumber);
            Span<PinValuePair> pinValuePairs = stackalloc PinValuePair[]
            {
                new PinValuePair(pinNumber, value)
            };
            Write(pinValuePairs);
        }

        /// <summary>
        /// Writes values to a set of pins
        /// </summary>
        protected void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
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
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            // No-op
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

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes,
            PinChangeEventHandler callback) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes,
            CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) =>
            (mode == PinMode.Input || mode == PinMode.Output);
    }
}
