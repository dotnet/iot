// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Runtime.CompilerServices;

namespace Iot.Device.Pcx857x
{
    public abstract class Pcx857x : IGpioController
    {
        protected I2cDevice Device { get; }
        private readonly IGpioController _masterGpioController;
        private readonly int _interrupt;

        // Pin mode bits- 0 for input, 1 for output to match PinMode
        private ushort _pinModes;

        // Pin value bits, 1 for high
        private ushort _pinValues;

        /// <summary>
        /// Remote I/O expander for I2C-bus with interrupt.
        /// </summary>
        /// <param name="device">The I2C device.</param>
        /// <param name="interrupt">The interrupt pin number, if present.</param>
        /// <param name="gpioController">
        /// The GPIO controller for the <paramref name="interrupt"/>.
        /// If not specified, the default controller will be used.
        /// </param>
        public Pcx857x(I2cDevice device, int interrupt = -1, IGpioController gpioController = null)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            _interrupt = interrupt;

            if (_interrupt != -1)
            {
                _masterGpioController = gpioController ?? new GpioController();
                _masterGpioController.OpenPin(_interrupt, PinMode.Input);
            }

            // These controllers do not have commands, setting the pins to high designates
            // them as able to recieve input. As we don't want to set high on pins intended
            // for output we'll set all of the pins to low for our initial state.
            if (PinCount == 8)
            {
                WriteByte(0x00);
            }
            else
            {
                InternalWriteUInt16(0x0000);
            }

            _pinModes = 0xFFFF;
        }

        public byte ReadByte() => Device.ReadByte();

        public void WriteByte(byte value) => Device.WriteByte(value);

        protected ushort InternalReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            Device.Read(buffer);
            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        protected void InternalWriteUInt16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            Device.Write(buffer);
        }

        /// <summary>
        /// The I/O pin count of the device.
        /// </summary>
        public abstract int PinCount { get; }

        public void ClosePin(int pinNumber)
        {
            // No-op
        }

        public void Dispose()
        {
            Device.Dispose();
        }

        public void OpenPin(int pinNumber, PinMode mode) => SetPinMode(pinNumber, mode);

        public PinValue Read(int pinNumber)
        {
            Span<PinValuePair> values = stackalloc PinValuePair[]{ new PinValuePair(pinNumber, default) };
            Read(values);
            return values[0].PinValue;
        }

        public void Read(Span<PinValuePair> pinValues)
        {
            (uint pins, _) = new PinVector32(pinValues);
            if (pins >> PinCount > 0)
                ThrowInvalidPin(nameof(pinValues));

            if ((pins & _pinModes) != 0)
            {
                // One of the specified pins was set to output (1)
                throw new InvalidOperationException("Cannot read from output pins.");
            }

            ushort data = PinCount == 8 ? ReadByte() : InternalReadUInt16();

            for (int i = 0; i < pinValues.Length; i++)
            {
                int pin = pinValues[i].PinNumber;
                pinValues[i] = new PinValuePair(pin, (data >> pin) & 1);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowInvalidPin(string argumentName)
        {
            // This is a helper to allow the JIT to inline calling methods.
            // (Methods with throws cannot be inlined.)
            throw new ArgumentOutOfRangeException(argumentName, $"Pin numbers must be in the range of 0 to {PinCount - 1}.");
        }

        private void ValidatePinNumber(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber >= PinCount)
                ThrowInvalidPin(nameof(pinNumber));
        }

        public void SetPinMode(int pinNumber, PinMode mode)
        {
            ValidatePinNumber(pinNumber);

            ushort SetBit(ushort data, int bitNumber) => (ushort)(data | (1 << bitNumber));
            ushort ClearBit(ushort data, int bitNumber) => (ushort)(data & ~(1 << bitNumber));

            if (mode == PinMode.Input)
            {
                _pinModes = ClearBit(_pinModes, pinNumber);
            }
            else if (mode == PinMode.Output)
            {
                _pinModes = SetBit(_pinModes, pinNumber);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(mode), "Only Input and Output modes are supported.");
            }

            WritePins(_pinValues);
        }

        private void WritePins(ushort value)
        {
            // We need to set all input pins to high
            _pinValues = (ushort)(value | ~_pinModes);

            if (PinCount == 8)
            {
                WriteByte((byte)_pinValues);
            }
            else
            {
                InternalWriteUInt16(_pinValues);
            }
        }

        public void Write(int pinNumber, PinValue value)
        {
            Span<PinValuePair> values = stackalloc PinValuePair[] { new PinValuePair(pinNumber, value) };
            Write(values);
        }

        public void Write(ReadOnlySpan<PinValuePair> pinValues)
        {
            (uint pins, uint values) = new PinVector32(pinValues);
            if (pins >> PinCount > 0)
                ThrowInvalidPin(nameof(pinValues));

            if ((pins & ~_pinModes) != 0)
            {
                // One of the specified pins was set to input (0)
                throw new InvalidOperationException("Cannot write to input pins.");
            }

            ushort SetBits(ushort current, ushort bits, ushort mask)
            {
                current &= (ushort)~mask;
                current |= bits;
                return current;
            }

            WritePins(SetBits(_pinValues, (ushort)values, (ushort)pins));
        }
    }
}
