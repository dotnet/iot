// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.Tca955x
{
    /// <summary>
    /// Base class for the Tca55x I2C I/O Expander
    /// </summary>
    public abstract class Tca955x : GpioDriver
    {
        private readonly int _interrupt;
        private readonly Dictionary<int, PinValue> _pinValues = new Dictionary<int, PinValue>();
        private readonly ConcurrentDictionary<int, PinChangeEventHandler> _eventHandlers = new ConcurrentDictionary<int, PinChangeEventHandler>();
        private readonly Dictionary<int, PinEventTypes> _interruptPins = new Dictionary<int, PinEventTypes>();
        private readonly Dictionary<int, PinValue> _interruptLastInputValues = new Dictionary<int, PinValue>();

        private GpioController? _controller;

        private bool _shouldDispose;

        private I2cDevice _busDevice;

        private object _interruptHandlerLock = new object();

        private ushort _gpioOutputCache;

        /// <summary>
        /// Default Adress of the Tca955X Family.
        /// </summary>
        public const byte DefaultI2cAdress = 0x20;

        /// <summary>
        /// Constructor for the Tca9555 I2C I/O Expander.
        /// </summary>
        /// <param name="device">The I2C device used for communication.</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt.</param>
        /// <param name="gpioController">The controller for the reset and interrupt pins. If not specified, the default controller will be used.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        protected Tca955x(I2cDevice device, int interrupt = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _busDevice = device;
            _interrupt = interrupt;

            if (_busDevice.ConnectionSettings.DeviceAddress < DefaultI2cAdress ||
                _busDevice.ConnectionSettings.DeviceAddress > DefaultI2cAdress + 7)
            {
                new ArgumentOutOfRangeException(nameof(device), "Adress should be in Range 0x20 to 0x27");
            }

            if (_interrupt != -1)
            {
                _shouldDispose = shouldDispose || gpioController is null;
                _controller = gpioController ?? new GpioController();
                if (!_controller.IsPinOpen(_interrupt))
                {
                    _controller.OpenPin(_interrupt);
                }

                if (_controller.GetPinMode(_interrupt) != PinMode.Input)
                {
                    _controller.SetPinMode(interrupt, PinMode.Input);
                }
            }
        }

        /// <summary>
        /// Reads a number of bytes from registers.
        /// </summary>
        /// <param name="register">The register to read from.</param>
        /// <param name="buffer">The buffer to read bytes into.</param>
        protected void InternalRead(byte register, Span<byte> buffer)
        {
            // First send write then read.
            _busDevice.WriteRead(stackalloc byte[1] { register }, buffer);
        }

        /// <summary>
        /// Writes a number of bytes to registers.
        /// </summary>
        /// <param name="register">The register address to write to.</param>
        /// <param name="data">The data to write to the registers.</param>
        protected void InternalWrite(byte register, byte data)
        {
            _busDevice.Write(stackalloc byte[2] { register, data });
        }

        /// <summary>
        /// Reads byte from the device register
        /// </summary>
        /// <param name="register">Register to read the value from</param>
        /// <returns>Byte read from the device register</returns>
        protected byte InternalReadByte(byte register)
        {
            Span<byte> buffer = stackalloc byte[1];
            InternalRead(register, buffer);
            return buffer[0];
        }

        /// <summary>
        /// Write byte to device register
        /// </summary>
        /// <param name="register">Register to write the value to</param>
        /// <param name="value">Value to be written to the <paramref name="register"/></param>
        protected void InternalWriteByte(byte register, byte value)
        {
            InternalWrite(register, value);
        }

        /// <summary>
        /// Read a byte from the given register.
        /// </summary>
        public byte ReadByte(byte register) => InternalReadByte(register);

        /// <summary>
        /// Write a byte to the given register.
        /// </summary>
        public void WriteByte(byte register, byte value) => InternalWriteByte(register, value);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }

            _pinValues.Clear();
            _busDevice?.Dispose();
            _busDevice = null!;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns the value of the interrupt pin if configured
        /// </summary>
        /// <returns>Value of interrupt pin</returns>
        protected PinValue ReadInterrupt()
        {
            if (_interrupt == -1 || _controller is null)
            {
                throw new ArgumentException("No interrupt pin configured.", nameof(_interrupt));
            }

            return _controller.Read(_interrupt);
        }

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

                byte polarityInversionRegister = GetRegisterIndex(pinNumber, Register.PolarityInversionPort);
                byte configurationRegister = GetRegisterIndex(pinNumber, Register.ConfigurationPort);
                ValidatePin(pinNumber);

                byte value;
                if (mode == PinMode.Output)
                {
                    value = ClearBit(InternalReadByte(configurationRegister), GetBitNumber(pinNumber));
                }
                else
                {
                    value = SetBit(InternalReadByte(configurationRegister), GetBitNumber(pinNumber));
                }

                InternalWriteByte(configurationRegister, value);

                byte value2;
                if (mode == PinMode.InputPullUp)
                {
                    value2 = SetBit(InternalReadByte(polarityInversionRegister), GetBitNumber(pinNumber));
                }
                else
                {
                    value2 = ClearBit(InternalReadByte(polarityInversionRegister), GetBitNumber(pinNumber));
                }

                InternalWriteByte(polarityInversionRegister, value2);
            }
        }

        /// <summary>
        /// Converts the pin number to the Register byte.
        /// </summary>
        /// <param name="pinNumber">The  pin number.</param>
        /// <param name="registerType">The register byte.</param>
        /// <returns>The register byte.</returns>
        protected abstract byte GetRegisterIndex(int pinNumber, Register registerType);

        /// <summary>
        /// Converts the pin number to the Bit number.
        /// </summary>
        /// <param name="pinNumber">The  pin number.</param>
        /// <returns>The bit position.</returns>
        protected abstract int GetBitNumber(int pinNumber);

        /// <summary>
        /// Mask the right byte from an input based on the pinnumber.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The reding value of the inputs</param>
        /// <returns>The masked byte of the given value.</returns>
        protected abstract byte GetByteRegister(int pinNumber, ushort value);

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

                for (int i = 0; i < pinValuePairs.Length; i++)
                {
                    int pin = pinValuePairs[i].PinNumber;
                    byte register = GetRegisterIndex(pin, Register.InputPort);
                    byte result = InternalReadByte(register);
                    pinValuePairs[i] = new PinValuePair(pin, result & (1 << GetBitNumber(pin)));
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

                ushort cachedValue = _gpioOutputCache;
                ushort newValue = SetBits(cachedValue, (ushort)newBits, (ushort)mask);
                if (cachedValue == newValue)
                {
                    return;
                }

                _gpioOutputCache = newValue;
                foreach (PinValuePair pinValuePair in pinValuePairs)
                {
                    byte register = GetRegisterIndex(pinValuePair.PinNumber, Register.OutputPort);
                    byte value = GetByteRegister(pinValuePair.PinNumber, newValue);
                    InternalWriteByte(register, value);
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

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            // No-op
            if (!_pinValues.ContainsKey(pinNumber))
            {
                _pinValues.Add(pinNumber, PinValue.Low);
            }
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

            byte register = GetRegisterIndex(pinNumber, Register.ConfigurationPort);
            return IsBitSet(InternalReadByte(register), GetBitNumber(pinNumber))
                ? PinMode.Input
                    : PinMode.Output;
        }

        private void InterruptHandler(object sender, PinValueChangedEventArgs e)
        {
            lock (_interruptHandlerLock)
            {
                if (_interruptPins.Count > 0)
                {
                    foreach (var interruptPin in _interruptPins)
                    {
                        PinValue newValue = Read(interruptPin.Key);
                        PinValue lastValue = _interruptLastInputValues[interruptPin.Key];

                        if ((interruptPin.Value.HasFlag(PinEventTypes.Rising) &&
                            lastValue == PinValue.Low &&
                            newValue == PinValue.High) ||
                            (interruptPin.Value.HasFlag(PinEventTypes.Falling) &&
                            lastValue == PinValue.High &&
                            newValue == PinValue.Low))
                        {
                            CallHandlerOnPin(interruptPin.Key, interruptPin.Value);
                        }

                        _interruptLastInputValues[interruptPin.Key] = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// Calls the event handler for the given pin, if any.
        /// </summary>
        /// <param name="pin">Pin to call the event handler on (if any exists)</param>
        /// <param name="pinEvent">Non-zero if the value is currently high (therefore assuming the pin value was rising), otherwise zero</param>
        private void CallHandlerOnPin(int pin, PinEventTypes pinEvent)
        {
            if (_eventHandlers.TryGetValue(pin, out var handler))
            {
                handler.Invoke(this, new PinValueChangedEventArgs(pinEvent, pin));
            }
        }

        /// <summary>
        /// Calls an event handler if the given pin changes.
        /// </summary>
        /// <param name="pinNumber">Pin number of the MCP23xxx</param>
        /// <param name="eventType">Whether the handler should trigger on rising, falling or both edges</param>
        /// <param name="callback">The method to call when an interrupt is triggered</param>
        /// <exception cref="InvalidOperationException">There's no GPIO controller for the master interrupt configured, or no interrupt lines are configured for the
        /// required port.</exception>
        /// <remarks>Only one event handler can be registered per pin. Calling this again with a different handler for the same pin replaces the handler</remarks>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            if (_controller == null)
            {
                throw new InvalidOperationException("No GPIO controller available. Specify a GPIO controller and the relevant interrupt line numbers in the constructor");
            }

            if (_interrupt == -1)
            {
                throw new InvalidOperationException("No interrupt pin configured");
            }

            _interruptPins.Add(pinNumber, eventType);
            _interruptLastInputValues.Add(pinNumber, Read(pinNumber));
            _controller.RegisterCallbackForPinValueChangedEvent(_interrupt, PinEventTypes.Falling, InterruptHandler);

            _eventHandlers[pinNumber] = callback;
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
                _interruptPins.Remove(pinNumber);
                _interruptLastInputValues.Remove(pinNumber);
                _controller.UnregisterCallbackForPinValueChangedEvent(_interrupt, InterruptHandler);
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
                    EventTypes = PinEventTypes.None,
                    TimedOut = true
                };
            }

            return new WaitForEventResult()
            {
                EventTypes = eventTypes1,
                TimedOut = false
            };
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) =>
            (mode == PinMode.Input || mode == PinMode.Output || mode == PinMode.InputPullUp);

    }
}
