// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Multiplexing;

namespace Iot.Device.CharacterLcd
{
    public abstract partial class LcdInterface : IDisposable
    {
        /// <summary>
        /// This interface allows the control of a character lcd using a shift register.
        /// The shift register can be controlled using a minimum of three pins so using this method
        /// saves pins on the microcontroller compared to direct GPIO connection.
        /// This interface adds support for I/O expanders that allow connection to an HD44780 display
        /// through a shift register e.g. https://www.adafruit.com/product/292
        /// </summary>
        private class ShiftRegisterLcdInterface : LcdInterface
        {
            private readonly long _registerSelectPin;
            private readonly long _enablePin;
            private readonly long[] _dataPins;
            private readonly long _backlightPin;

            private readonly bool _shouldDispose;
            private ShiftRegister _shiftRegister;
            private bool _backlightOn;

            /// <summary>
            /// ShiftRegister based interface for the LCD.
            /// </summary>
            /// <remarks>
            /// Pin parameters should be set according to which output pin of the shift register they are connected to
            /// (e.g. 0 to 7 for 8bit shift register).
            /// </remarks>
            /// <param name="registerSelectPin">The pin that controls the register select.</param>
            /// <param name="enablePin">The pin that controls the enable switch.</param>
            /// <param name="dataPins">Collection of pins holding the data that will be printed on the screen.</param>
            /// <param name="backlightPin">The optional pin that controls the backlight of the display.</param>
            /// <param name="shiftRegister">The shift register that drives the LCD.</param>
            /// <param name="shouldDispose">True to dispose the shift register.</param>
            public ShiftRegisterLcdInterface(int registerSelectPin, int enablePin, int[] dataPins, int backlightPin = -1, ShiftRegister? shiftRegister = null, bool shouldDispose = true)
            {
                _registerSelectPin = 1 << registerSelectPin;
                _enablePin = 1 << enablePin;

                if (dataPins.Length == 8)
                {
                    EightBitMode = true;
                }
                else if (dataPins.Length != 4)
                {
                    throw new ArgumentException("The length of the array must be 4 or 8.", nameof(dataPins));
                }

                _dataPins = new long[dataPins.Length];

                for (var i = 0; i < dataPins.Length; i++)
                {
                    _dataPins[i] = 1 << dataPins[i];
                }

                if (backlightPin != -1)
                {
                    _backlightPin = 1 << backlightPin;
                }

                _shouldDispose = shouldDispose || _shiftRegister is null;
                _shiftRegister = shiftRegister ?? new ShiftRegister(ShiftRegisterPinMapping.Minimal, 8);

                _backlightOn = true;
                Initialize();
            }

            public override bool EightBitMode { get; }

            /// <summary>
            /// This Display supports enabling/disabling the backlight.
            /// The text on the display is not affected by disabling the backlight - it is just very hard to read.
            /// </summary>
            public override bool BacklightOn
            {
                get => _backlightOn;
                set
                {
                    _backlightOn = value;
                    // Need to send a command to make this happen immediately.
                    SendCommandAndWait(0);
                }
            }

            private long BacklightFlag
            {
                get
                {
                    if (BacklightOn)
                    {
                        return _backlightPin;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            private void Initialize()
            {
                // It is possible that the initialization will fail if the power is not provided
                // within specific tolerances. As such, we'll always perform the software based
                // initialization as described on pages 45/46 of the HD44780 data sheet.
                if (_dataPins.Length == 8)
                {
                    // Init to 8 bit mode (this is the default, but other drivers
                    // may set the controller to 4 bit mode, so reset to be safe.)
                    SendCommandAndWait(0x30);
                    SendCommandAndWait(0x30);
                    SendCommandAndWait(0x30);
                }
                else
                {
                    // Init to 4 bit mode
                    SendCommandAndWait(0x3);
                    SendCommandAndWait(0x3);
                    SendCommandAndWait(0x3);
                    SendCommandAndWait(0x2);
                }
            }

            public override void SendCommand(byte command)
            {
                if (_dataPins.Length == 8)
                {
                    WriteBits(0x0 | MapCommandToDataPins(command));
                }
                else
                {
                    WriteBits(0x0 | MapCommandToDataPins((byte)(command >> 4)));
                    WriteBits(0x0 | MapCommandToDataPins(command));
                }
            }

            public override void SendCommands(ReadOnlySpan<byte> commands)
            {
                foreach (byte command in commands)
                {
                    SendCommand(command);
                }
            }

            public override void SendData(byte value)
            {
                if (_dataPins.Length == 8)
                {
                    WriteBits(_registerSelectPin | MapCommandToDataPins(value));
                }
                else
                {
                    WriteBits(_registerSelectPin | MapCommandToDataPins((byte)(value >> 4)));
                    WriteBits(_registerSelectPin | MapCommandToDataPins(value));
                }
            }

            public override void SendData(ReadOnlySpan<byte> values)
            {
                foreach (byte value in values)
                {
                    SendData(value);
                }
            }

            public override void SendData(ReadOnlySpan<char> values)
            {
                foreach (byte value in values)
                {
                    SendData(value);
                }
            }

            private void WriteBits(long command)
            {
                ShiftLong(command | _enablePin | BacklightFlag);
                ShiftLong((command & ~_enablePin) | BacklightFlag);
            }

            // This method iterates through the bits of the 'original' command and sets them to the
            // appropriate position so that they are shifted out to the correct pins.
            private long MapCommandToDataPins(byte command)
            {
                long bits = 0x0;

                for (var i = 0; i < _dataPins.Length; i++)
                {
                    var bit = (command >> i) & 1;

                    if (bit == 1)
                    {
                        bits |= _dataPins[i];
                    }
                }

                return bits;
            }

            private void ShiftLong(long value)
            {
                for (int i = (_shiftRegister.BitLength / 8) - 1; i > 0; i--)
                {
                    int shift = i * 8;
                    long downShiftedValue = value >> shift;
                    _shiftRegister.ShiftByte((byte)downShiftedValue, false);
                }

                _shiftRegister.ShiftByte((byte)value);
            }

            protected override void Dispose(bool disposing)
            {
                if (_shouldDispose)
                {
                    _shiftRegister?.Dispose();
                }

                _shiftRegister = null!;
                base.Dispose(disposing);
            }
        }
    }
}
