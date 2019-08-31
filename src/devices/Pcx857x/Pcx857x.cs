// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Device.Gpio;
using System.Device.I2c;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Base class for PCx857x GPIO expanders
    /// </summary>
    public abstract class Pcx857x : GpioDriver
    {
        /// <summary>
        /// I2C device used for communication with the device
        /// </summary>
        protected I2cDevice Device { get; }
        private readonly GpioController _masterGpioController;
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
        public Pcx857x(I2cDevice device, int interrupt = -1, GpioController gpioController = null)
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

        /// <summary>
        /// Reads byte from the device
        /// </summary>
        /// <returns>Byte read from the device</returns>
        public byte ReadByte() => Device.ReadByte();

        /// <summary>
        /// Writes byte to the device
        /// </summary>
        /// <param name="value">Byte to be written to the device</param>
        public void WriteByte(byte value) => Device.WriteByte(value);

        /// <summary>
        /// Reads 16-bit unsigned integer from the device
        /// </summary>
        /// <returns>16-bit unsigned integer read from the device</returns>
        protected ushort InternalReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            Device.Read(buffer);
            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        /// <summary>
        /// Writes 16-bit unsigned integer to the device
        /// </summary>
        /// <param name="value">16-vit unsigned integer to be written to the device</param>
        protected void InternalWriteUInt16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            Device.Write(buffer);
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            // No-op
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Device.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            // No-op
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            Span<PinValuePair> values = stackalloc PinValuePair[]{ new PinValuePair(pinNumber, default) };
            Read(values);
            return values[0].PinValue;
        }

        /// <summary>
        /// Reads multiple pins from the device
        /// </summary>
        /// <param name="pinValues">Pins and values to be read</param>
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

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
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

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            Span<PinValuePair> values = stackalloc PinValuePair[] { new PinValuePair(pinNumber, value) };
            Write(values);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber) => ((_pinModes & (1 << pinNumber)) == 0) ? PinMode.Input : PinMode.Output;

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) => (mode == PinMode.Output || mode == PinMode.Input);

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        // For now eventing APIs are not supported.
        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
