// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Multiplexing;

namespace Iot.Device.CharacterLcd
{
    public abstract partial class LcdInterface : IDisposable
    {
        /// <summary>
        /// This interface allows the control of a character display using SPI protocol.
        /// The display is controlled using SN74HC595 shift register. This shift register can be controlled
        /// using a minimum of three pins so this method saves pins on the microcontroller compared to direct GPIO
        /// connection. This interface adds support for existing products that allow connect to an HD44780 display
        /// through a shift register e.g. https://www.adafruit.com/product/292
        /// </summary>
        private class Spi : LcdInterface
        {
            private readonly byte _registerSelectPin;
            private readonly byte _enablePin;
            private readonly byte[] _dataPins;
            private readonly byte _backlightPin;

            private readonly bool _shouldDispose;
            private Sn74hc595 _shiftRegister;
            private bool _backlightOn;

            /// <summary>
            /// Creates an LCD interface based on the SN74HC595 shift register.
            /// </summary>
            /// <remarks>
            /// Pin parameters should be set according to which output pin of the shift register they are connected to (0 to 7).
            /// </remarks>
            /// <param name="registerSelectPin">The pin that controls the register select.</param>
            /// <param name="enablePin">The pin that controls the enable switch.</param>
            /// <param name="dataPins">Collection of pins holding the data that will be printed on the screen.</param>
            /// <param name="backlightPin">The optional pin that controls the backlight of the display.</param>
            /// <param name="shiftRegister">The shift register that drives the LCD.</param>
            /// <param name="shouldDispose">True to dispose the shift register.</param>
            public Spi(int registerSelectPin, int enablePin, int[] dataPins, int backlightPin = -1, Sn74hc595? shiftRegister = null, bool shouldDispose = true)
            {
                _registerSelectPin = (byte)(1 << registerSelectPin);
                _enablePin = (byte)(1 << enablePin);

                // Right now only 4bit mode is supported. 8bit mode would require 16bit shift register or 2x 8bit
                // because we would need more than 8 output pins to support 8bit mode. This could be implemented in the future.
                if (dataPins.Length != 4)
                {
                    throw new ArgumentException("The length of the array must be 4.", nameof(dataPins));
                }

                _dataPins = new byte[dataPins.Length];

                for (var i = 0; i < dataPins.Length; i++)
                {
                    _dataPins[i] = (byte)(1 << dataPins[i]);
                }

                if (backlightPin != -1)
                {
                    _backlightPin = (byte)(1 << backlightPin);
                }

                _shouldDispose = shouldDispose || _shiftRegister is null;
                _shiftRegister = shiftRegister ?? new Sn74hc595(Sn74hc595PinMapping.Minimal);

                _backlightOn = true;
                Initialize();
            }

            public override bool EightBitMode => false;

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

            private byte BacklightFlag
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
                // This sequence (copied from a python example) completely resets the display (if it was
                // previously erroneously used with 8 bit access, it may not return to normal operation otherwise)
                SendCommandAndWait(0x3);
                SendCommandAndWait(0x3);
                SendCommandAndWait(0x3);
                SendCommandAndWait(0x2);
            }

            public override void SendCommand(byte command)
            {
                Write4Bits((byte)(0x0 | MapCommandToDataPins((byte)(command >> 4))));
                Write4Bits((byte)(0x0 | MapCommandToDataPins(command)));
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
                Write4Bits((byte)(_registerSelectPin | MapCommandToDataPins((byte)(value >> 4))));
                Write4Bits((byte)(_registerSelectPin | MapCommandToDataPins(value)));
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

            private void Write4Bits(byte command)
            {
                _shiftRegister.ShiftByte((byte)(command | _enablePin | BacklightFlag));
                _shiftRegister.ShiftByte((byte)((command & ~_enablePin) | BacklightFlag));
            }

            // This method iterates through the bits of the 'original' command and sets them to the
            // appropriate position so that they are shifted out to the correct pins.
            private byte MapCommandToDataPins(byte command)
            {
                byte bits = 0x0;

                for (var i = 0; i < _dataPins.Length; i++)
                {
                    var bit = (command >> i) & 1;

                    if (bit == 1)
                    {
                        bits = (byte)(bits | _dataPins[i]);
                    }
                }

                return bits;
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
