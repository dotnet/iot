// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.CharacterLcd
{
    public abstract partial class LcdInterface : IDisposable
    {
        /// <summary>
        /// Built-in I2c access to the Hd44780 compatible controller. The Philips/NXP LCD driver ICs
        /// (such as the PCF2119x) are examples of this support.
        /// This driver uses 4-Bit access (each character/command is split into 2x4 bits for transmission)
        /// </summary>
        private class I2c4Bit : LcdInterface
        {
            private const byte ENABLE = 0b0000_0100;
            private const byte READWRITE = 0b0000_0010;
            private const byte REGISTERSELECT = 0b0000_0001;

            private const byte LCD_BACKLIGHT = 0x08;
            private const byte LCD_FUNCTIONSET = 0x20;
            private const byte LCD_DISPLAYCONTROL = 0x08;
            private const byte LCD_CLEARDISPLAY = 0x01;
            private const byte LCD_DISPLAYON = 0x04;
            private const byte LCD_2LINE = 0x08;
            private const byte LCD_5x8DOTS = 0x00;
            private const byte LCD_ENTRYMODESET = 0x04;
            private const byte LCD_ENTRYLEFT = 0x02;
            private const byte LCD_4BITMODE = 0x00;

            private readonly I2cDevice _device;
            private bool _backlightOn;

            public I2c4Bit(I2cDevice device)
            {
                _device = device ?? throw new ArgumentNullException(nameof(device));
                _backlightOn = true;
                InitDisplay();
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
                    SendCommand(0);
                }
            }

            private byte BacklightFlag
            {
                get
                {
                    if (BacklightOn)
                    {
                        return LCD_BACKLIGHT;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            private void InitDisplay()
            {
                // This sequence (copied from a python example) completely resets the display (if it was
                // previously erroneously used with 8 bit access, it may not return to normal operation otherwise)
                SendCommandAndWait(0x3);
                SendCommandAndWait(0x3);
                SendCommandAndWait(0x3);
                SendCommandAndWait(0x2);
                SendCommandAndWait(LCD_FUNCTIONSET | LCD_2LINE | LCD_5x8DOTS | LCD_4BITMODE);
                SendCommandAndWait(LCD_DISPLAYCONTROL | LCD_DISPLAYON);
                SendCommandAndWait(LCD_CLEARDISPLAY);
                SendCommandAndWait(LCD_ENTRYMODESET | LCD_ENTRYLEFT);
            }

            private void SendCommandAndWait(byte command)
            {
                // Must not run the init sequence to fast or undefined behavior may occur
                SendCommand(command);
                Thread.Sleep(1);
            }

            public override void SendCommand(byte command)
            {
                Write4Bits((byte)(0x00 | (command & 0xF0)));
                Write4Bits((byte)(0x00 | ((command << 4) & 0xF0)));
            }

            private void Write4Bits(byte command)
            {
                _device.WriteByte((byte)(command | ENABLE | BacklightFlag));
                _device.WriteByte((byte)((command & ~ENABLE) | BacklightFlag));
            }

            public override void SendCommands(ReadOnlySpan<byte> commands)
            {
                foreach (var c in commands)
                {
                    SendCommand(c);
                }
            }

            public override void SendData(byte value)
            {
                Write4Bits((byte)(REGISTERSELECT | (value & 0xF0)));
                Write4Bits((byte)(REGISTERSELECT | ((value << 4) & 0xF0)));
            }

            public override void SendData(ReadOnlySpan<byte> values)
            {
                foreach (var c in values)
                {
                    Write4Bits((byte)(REGISTERSELECT | (c & 0xF0)));
                    Write4Bits((byte)(REGISTERSELECT | ((c << 4) & 0xF0)));
                }
            }
        }
    }
}
