// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    public abstract class Mcp23xxx : IDisposable, IGpioController
    {
        protected int DeviceAddress { get; }
        private GpioController _masterGpioController;
        private readonly int _reset;
        private readonly int _interruptA;
        private readonly int _interruptB;
        private BankStyle _bankStyle;
        protected readonly IBusDevice _device;
        private bool _increments = true;

        private ushort _gpioCache;
        private bool _cacheValid;

        /// <summary>
        /// A general purpose parallel I/O expansion for I2C or SPI applications.
        /// </summary>
        /// <param name="deviceAddress">The device address for the connection on the I2C or SPI bus.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        /// <param name="bankStyle">
        /// The current bank style of the ports. This does not set the bank style- it tells us what the bank style is.
        /// It is *highly* recommended not to change the bank style from the default as there is no direct way to
        /// detect what style the chip is in and most apps will fail if the chip is not set to defaults. This setting
        /// has no impact on 8 bit expanders.
        /// </param>
        public Mcp23xxx(IBusDevice device, int deviceAddress, int reset = -1, int interruptA = -1, int interruptB = -1, BankStyle bankStyle = BankStyle.Sequential)
        {
            ValidateDeviceAddress(deviceAddress);
            DeviceAddress = deviceAddress;

            _device = device;
            _bankStyle = bankStyle;

            _reset = reset;
            _interruptA = interruptA;
            _interruptB = interruptB;

            InitializeMasterGpioController();

            // Set all of the pins to input, GPIO outputs to low, and
            // disable all of the pull-ups
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

        private void UpdateCache()
        {
            // Ouput state is read from the latches (OLAT)
            _gpioCache = PinCount == 8
                ? InternalReadByte(Register.OLAT, Port.PortA)
                : InternalReadUInt16(Register.OLAT);
            _cacheValid = true;
        }

        private void ValidateDeviceAddress(int deviceAddress)
        {
            if (deviceAddress < 0x20 || deviceAddress > 0x27)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress), deviceAddress, "The Mcp23xxx address must be a value of 32 (0x20) - 39 (0x27).");
            }
        }

        private void InitializeMasterGpioController()
        {
            // Only need master controller if there are external pins provided.
            if (_reset != -1 || _interruptA != -1 || _interruptB != -1)
            {
                _masterGpioController = new GpioController();

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
        }

        /// <summary>
        /// The I/O pin count of the device.
        /// </summary>
        public abstract int PinCount { get; }

        /// <summary>
        /// Reads a number of bytes from registers.
        /// </summary>
        /// <param name="register">The register to read from.</param>
        /// <param name="buffer">The buffer to read bytes into.</param>
        /// <param name="port">The I/O port used with the register.</param>
        /// <returns>The data read from the registers.</returns>
        protected void InternalRead(Register register, Span<byte> buffer, Port port)
        {
            _device.Read(GetMappedAddress(register, port, _bankStyle), buffer);
        }

        /// <summary>
        /// Writes a number of bytes to registers.
        /// </summary>
        /// <param name="register">The register address to write to.</param>
        /// <param name="buffer">The data to write to the registers.</param>
        /// <param name="port">The I/O port used with the registers.</param>
        protected void InternalWrite(Register register, Span<byte> data, Port port)
        {
            _device.Write(GetMappedAddress(register, port, _bankStyle), data);
        }

        protected byte InternalReadByte(Register register, Port port)
        {
            Span<byte> buffer = stackalloc byte[1];
            InternalRead(register, buffer, port);
            return buffer[0];
        }

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

        public virtual void Dispose()
        {
            _masterGpioController?.Dispose();
            _masterGpioController = null;
            _device?.Dispose();
        }

        /// <summary>
        /// Disables the device by setting the reset pin LOW.
        /// </summary>
        public void Disable()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_reset == -1)
            {
                throw new Exception("Reset pin has not been initialized.");
            }

            _masterGpioController.Write(_reset, PinValue.Low);

            // Registers will all be reset when re-enabled
            _bankStyle = BankStyle.Sequential;
            _increments = true;
        }

        /// <summary>
        /// Enables the device by setting the reset pin HIGH.
        /// </summary>
        public void Enable()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_reset == -1)
            {
                throw new Exception("Reset pin has not been initialized.");
            }

            _masterGpioController.Write(_reset, PinValue.High);
        }

        protected PinValue InternalReadInterrupt(Port port)
        {
            if (_masterGpioController == null)
                throw new Exception("Master controller has not been initialized.");

            int pinNumber = 0;
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
                throw new ArgumentException("No interrupt pin configured.", nameof(port));

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
        public void SetPinMode(int pinNumber, PinMode mode)
        {
            ValidateMode(mode);
            ValidatePin(pinNumber);

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
        public PinValue Read(int pinNumber)
        {
            ValidatePin(pinNumber);
            Span<PinValuePair> values = stackalloc PinValuePair[1];
            values[0] = new PinValuePair(pinNumber, default);
            Read(values);
            return values[0].PinValue;
        }

        public void Read(Span<PinValuePair> pinValues)
        {
            ushort pins = 0;
            foreach (PinValuePair pair in pinValues)
            {
                ValidatePin(pair.PinNumber);
                pins |= (ushort)(1 << pair.PinNumber);
            }

            ushort result = 0;
            if (pins < 0xFF + 1)
            {
                // Only need to get the first 8 pins (PortA)
                result = InternalReadByte(Register.OLAT, Port.PortA);
            }
            else if ((pins & 0xFF) == 0)
            {
                // Only need to get the second 8 pins (PortB)
                result = InternalReadByte(Register.OLAT, Port.PortB);
            }
            else
            {
                // Need to get both
                result = InternalReadUInt16(Register.OLAT);
            }

            for (int i = 0; i < pinValues.Length; i++)
            {
                int pin = pinValues[i].PinNumber;
                pinValues[i]= new PinValuePair(pin, result & (1 << pin));
            }
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The value to be written.</param>
        public void Write(int pinNumber, PinValue value)
        {
            ValidatePin(pinNumber);
            Span<PinValuePair> values = stackalloc PinValuePair[1];
            values[0] = new PinValuePair(pinNumber, default);
            Write(values);
        }

        public void Write(ReadOnlySpan<PinValuePair> pinValues)
        {
            ushort mask = 0;
            ushort newBits = 0;

            foreach (PinValuePair pair in pinValues)
            {
                ValidatePin(pair.PinNumber);
                ushort bit = (ushort)(1 << pair.PinNumber);
                mask |= bit;
                if (pair.PinValue == PinValue.High)
                {
                    newBits |= bit;
                }
            }

            if (!_cacheValid)
                UpdateCache();

            ushort cachedValue = _gpioCache;
            ushort newValue = SetBits(cachedValue, newBits, mask);
            if (cachedValue == newValue)
                return;

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

        private byte SetBit(byte data, int bitNumber) => data |= (byte)(1 << bitNumber);

        private byte ClearBit(byte data, int bitNumber) => data &= (byte)~(1 << bitNumber);

        private byte SetBits(byte current, byte bits, byte mask)
        {
            current &= (byte)~mask;
            current |= bits;
            return current;
        }

        private ushort SetBits(ushort current, ushort bits, ushort mask)
        {
            current &= (ushort)~mask;
            current |= bits;
            return current;
        }

        private static void ValidateMode(PinMode mode)
        {
            if (mode != PinMode.Input && mode != PinMode.Output)
            {
                throw new ArgumentException("Mcp supports Input and Output modes only.");
            }
        }

        private void ValidatePin(int pinNumber)
        {
            if (pinNumber >= PinCount || pinNumber < 0)
            {
                throw new ArgumentOutOfRangeException($"{pinNumber} is not a valid pin on the Mcp controller.");
            }
        }

        private static void ThrowArgumentOutOfRange(string argument)
        {
            throw new ArgumentOutOfRangeException(argument);
        }

        /// <summary>
        /// Gets the mapped address for a register.
        /// </summary>
        /// <param name="register">The register.</param>
        /// <param name="port">The bank of I/O ports used with the register.</param>
        /// <param name="bankStyle">The bank style that determines how the register addresses are grouped.</param>
        /// <returns>The byte address of the register for the given port bank and bank style.</returns>
        private byte GetMappedAddress(Register register, Port port = Port.PortA, BankStyle bankStyle = BankStyle.Sequential)
        {
            if (port != Port.PortA && port != Port.PortB)
                ThrowArgumentOutOfRange(nameof(port));
            if (bankStyle != BankStyle.Separated && bankStyle != BankStyle.Sequential)
                ThrowArgumentOutOfRange(nameof(bankStyle));

            byte address = (byte)register;

            // There is no mapping for 8 bit expanders
            if (PinCount == 8)
                return address;

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

        private static string GetBits(byte value) => Convert.ToString(value, 2).PadLeft(8, '0');
        private static string GetBits(ushort value) => Convert.ToString(value, 2).PadLeft(16, '0');

        public void OpenPin(int pinNumber, PinMode mode) => SetPinMode(pinNumber, mode);

        public void ClosePin(int pinNumber)
        {
            // No-op
        }
    }
}
