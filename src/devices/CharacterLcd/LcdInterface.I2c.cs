// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.CharacterLcd
{
    public abstract partial class LcdInterface : IDisposable
    {
        /// <summary>
        /// Built-in I2c access to the Hd44780 compatible controller. The Philips/NXP LCD driver ICs
        /// (such as the PCF2119x) are examples of this support.
        /// </summary>
        private class I2c : LcdInterface
        {
            [Flags]
            private enum ControlByteFlags : byte
            {
                /// <summary>
                /// When set, another control byte will follow the next data/command byte.
                /// Otherwise the last control byte will be used.
                /// </summary>
                /// <remarks>
                /// This is only relevant when sending multiple bytes of data to the register.
                /// When a new I2c transmission is made, the first byte is always assumed to
                /// be a control byte. This flag is needed if you want to flip the "RS" bit
                /// in a stream of bytes.
                /// </remarks>
                ControlByteFollows = 0b_1000_0000,

                /// <summary>
                /// When set the data register will be selected (i.e. equivalent to
                /// RS pin being high). Otherwise the instruction/command register
                /// will be updated.
                /// </summary>
                RegisterSelect = 0b_0100_0000

                // No other bits are used
            }

            // Philips created the I2c bus and likely were the first to provide character LCD
            // controllers with I2c functionality built-in. This driver was written using the
            // PCF2119x datasheet as reference. Other chipsets are compatible with the addressing
            // scheme (Sitronix ST7036, Aiptek AIP31068L, probably others).

            private readonly I2cDevice _device;

            public I2c(I2cDevice device)
            {
                _device = device;

                // While the LCD controller can be set to 4 bit mode there really isn't a way to
                // mess with that from the I2c pins as far as I know. Other drivers try to set the
                // controller up for 8 bit mode, but it appears they are doing so only because they've
                // copied existing HD44780 drivers.
            }

            public override bool EightBitMode => true;

            public override bool BacklightOn 
            {
                get
                {
                    // Not implemented / Not supported, but don't throw exceptions here
                    return true;
                }
                set
                {
                }
            }

            public override void SendCommand(byte command)
            {
                Span<byte> buffer = stackalloc byte[] { 0x00, command };
                _device.Write(buffer);
            }

            public override void SendCommands(ReadOnlySpan<byte> commands)
            {
                // There is a limit to how much data the controller can accept at once. Haven't found documentation
                // for this yet, can probably iterate a bit more on this to find a true "max". Not adding additional
                // logic like SendData as we don't expect a need to send more than a handful of commands at a time.

                if (commands.Length > 20)
                    throw new ArgumentOutOfRangeException(nameof(commands), "Too many commands in one request.");
                Span<byte> buffer = stackalloc byte[commands.Length + 1];
                buffer[0] = 0x00;
                commands.CopyTo(buffer.Slice(1));
                _device.Write(buffer);
            }

            public override void SendData(byte value)
            {
                Span<byte> buffer = stackalloc byte[] { (byte)ControlByteFlags.RegisterSelect, value };
                _device.Write(buffer);
            }

            public override void SendData(ReadOnlySpan<byte> values)
            {
                // There is a limit to how much data the controller can accept at once. Haven't found documentation
                // for this yet, can probably iterate a bit more on this to find a true "max". 40 was too much.

                const int MaxCopy = 20;
                Span<byte> buffer = stackalloc byte[MaxCopy + 1];
                buffer[0] = (byte)ControlByteFlags.RegisterSelect;
                Span<byte> bufferData = buffer.Slice(1);

                while (values.Length > 0)
                {
                    ReadOnlySpan<byte> currentValues = values.Slice(0, values.Length > MaxCopy ? MaxCopy : values.Length);
                    values = values.Slice(currentValues.Length);
                    currentValues.CopyTo(bufferData);
                    _device.Write(buffer.Slice(0, currentValues.Length + 1));
                }
            }
        }
    }
}
