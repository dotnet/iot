// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;

namespace Iot.Device.CharacterLcd
{
    public abstract partial class LcdInterface : IDisposable
    {
        /// <summary>
        /// Standard direct pin access to the HD44780 controller.
        /// </summary>
        private class Gpio : LcdInterface
        {
            /// <summary>
            /// Register select pin. Low is for writing to the instruction
            /// register and reading the address counter. High is for reading
            /// and writing to the data register.
            /// </summary>
            private readonly int _rsPin;

            /// <summary>
            /// Read/write pin. Low for write, high for read.
            /// </summary>
            private readonly int _rwPin;

            /// <summary>
            /// Enable pin. Pulse high to process a read/write.
            /// </summary>
            private readonly int _enablePin;

            private readonly int _backlight;

            private readonly int[] _dataPins;

            // We need to add PWM support to make this useful (to drive the VO pin).
            // For now we'll just stash the value and use it to decide the initial
            // backlight state.
            private float _backlightBrightness;

            private byte _lastByte;
            private bool _useLastByte;

            private GpioController _controller;
            private bool _shouldDispose;
            private PinValuePair[] _pinBuffer = new PinValuePair[8];

            public Gpio(int registerSelectPin, int enablePin, int[] dataPins, int backlightPin = -1, float backlightBrightness = 1.0f, int readWritePin = -1, GpioController? controller = null, bool shouldDispose = true)
            {
                _rwPin = readWritePin;
                _rsPin = registerSelectPin;
                _enablePin = enablePin;
                _dataPins = dataPins;
                _backlight = backlightPin;
                _backlightBrightness = backlightBrightness;

                if (dataPins.Length == 8)
                {
                    EightBitMode = true;
                }
                else if (dataPins.Length != 4)
                {
                    throw new ArgumentException($"The length of the array given to parameter {nameof(dataPins)} must be 4 or 8");
                }

                _shouldDispose = controller == null ? true : shouldDispose;
                _controller = controller ?? new GpioController(PinNumberingScheme.Logical);

                Initialize();
            }

            public override bool EightBitMode { get; }

            private void Initialize()
            {
                // Prep the pins
                _controller.OpenPin(_rsPin, PinMode.Output);

                if (_rwPin != -1)
                {
                    _controller.OpenPin(_rwPin, PinMode.Output);

                    // Set to write. Once we enable reading have reading pull high and reset
                    // after reading to give maximum performance to write (i.e. assume that
                    // the pin is low when writing).
                    _controller.Write(_rwPin, PinValue.Low);
                }

                if (_backlight != -1)
                {
                    _controller.OpenPin(_backlight, PinMode.Output);
                    if (_backlightBrightness > 0)
                    {
                        // Turn on the backlight
                        _controller.Write(_backlight, PinValue.High);
                    }
                }

                _controller.OpenPin(_enablePin, PinMode.Output);

                for (int i = 0; i < _dataPins.Length; ++i)
                {
                    _controller.OpenPin(_dataPins[i], PinMode.Output);
                }

                // The HD44780 self-initializes when power is turned on to the following settings:
                //
                //  - 8 bit, 1 line, 5x7 font
                //  - Display, cursor, and blink off
                //  - Increment with no shift
                //
                // It is possible that the initialization will fail if the power is not provided
                // within specific tolerances. As such, we'll always perform the software based
                // initialization as described on pages 45/46 of the HD44780 data sheet. We give
                // a little extra time to the required waits as described.
                if (_dataPins.Length == 8)
                {
                    // Init to 8 bit mode (this is the default, but other drivers
                    // may set the controller to 4 bit mode, so reset to be safe.)
                    DelayHelper.DelayMilliseconds(50, allowThreadYield: true);
                    WriteBits(0b0011_0000, 8);
                    DelayHelper.DelayMilliseconds(5, allowThreadYield: true);
                    WriteBits(0b0011_0000, 8);
                    DelayHelper.DelayMicroseconds(100, allowThreadYield: true);
                    WriteBits(0b0011_0000, 8);
                }
                else
                {
                    // Init to 4 bit mode, setting _rspin to low as we're writing 4 bits directly.
                    // (Send writes the whole byte in two 4bit/nybble chunks)
                    _controller.Write(_rsPin, PinValue.Low);
                    DelayHelper.DelayMilliseconds(50, allowThreadYield: true);
                    WriteBits(0b0011, 4);
                    DelayHelper.DelayMilliseconds(5, allowThreadYield: true);
                    WriteBits(0b0011, 4);
                    DelayHelper.DelayMicroseconds(100, allowThreadYield: true);
                    WriteBits(0b0011, 4);
                    WriteBits(0b0010, 4);
                }

                // The busy flag can NOT be checked until this point.
            }

            public override bool BacklightOn
            {
                get
                {
                    return _backlight != -1 && _controller.Read(_backlight) == PinValue.High;
                }
                set
                {
                    if (_backlight != -1)
                    {
                        _controller.Write(_backlight, value ? PinValue.High : PinValue.Low);
                    }
                }
            }

            public override void SendCommand(byte command)
            {
                _controller.Write(_rsPin, PinValue.Low);
                SendByte(command);
            }

            public override void SendCommands(ReadOnlySpan<byte> commands)
            {
                _controller.Write(_rsPin, PinValue.Low);
                foreach (byte command in commands)
                {
                    SendByte(command);
                }
            }

            public override void SendData(byte value)
            {
                _controller.Write(_rsPin, PinValue.High);
                SendByte(value);
            }

            public override void SendData(ReadOnlySpan<byte> values)
            {
                _controller.Write(_rsPin, PinValue.High);
                foreach (byte value in values)
                {
                    SendByte(value);
                }
            }

            private void SendByte(byte value)
            {
                if (_dataPins.Length == 8)
                {
                    WriteBits(value, 8);
                }
                else
                {
                    WriteBits((byte)(value >> 4), 4);
                    WriteBits(value, 4);
                }

                // Most commands need a maximum of 37μs to complete.
                WaitForNotBusy(37);
            }

            private void WriteBits(byte bits, int count)
            {
                int changedCount = 0;
                for (int i = 0; i < count; i++)
                {
                    int newBit = (bits >> i) & 1;
                    if (!_useLastByte)
                    {
                        _pinBuffer[changedCount++] = new PinValuePair(_dataPins[i], newBit);
                    }
                    else
                    {
                        // Each bit change takes ~23μs, so only change what we have to
                        // This is particularly impactful when using all 8 data lines.
                        int oldBit = (_lastByte >> i) & 1;
                        if (oldBit != newBit)
                        {
                            _pinBuffer[changedCount++] = new PinValuePair(_dataPins[i], newBit);
                        }
                    }
                }

                if (changedCount > 0)
                {
                    _controller.Write(new ReadOnlySpan<PinValuePair>(_pinBuffer, 0, changedCount));
                }

                _useLastByte = true;
                _lastByte = bits;

                // Enable pin needs to be high for at least 450ns when running on 3V
                // and 230ns on 5V. (PWeh on page 49/52 and Figure 25 on page 58)
                _controller.Write(_enablePin, PinValue.High);
                DelayHelper.DelayMicroseconds(1, allowThreadYield: false);
                _controller.Write(_enablePin, PinValue.Low);
            }

            protected override void Dispose(bool disposing)
            {
                if (_shouldDispose)
                {
                    _controller?.Dispose();
                    _controller = null!;
                }

                base.Dispose(disposing);
            }
        }
    }
}
