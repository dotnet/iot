// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Display.Internal;
using static System.Linq.Enumerable;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3730 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/microdot-phat
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    // Based on: https://github.com/pimoroni/microdot-phat/blob/master/library/microdotphat/matrix.py
    public class Is31fl3730
    {
        private readonly byte[] _matrix_addresses = new byte[] { Is31fl3730FunctionRegister.Matrix1, Is31fl3730FunctionRegister.Matrix2 };
        private readonly List<byte[]> _buffers = new List<byte[]>
        {
            new byte[8],
            new byte[8]
        };
        private readonly bool[] _enabled = new bool[2];
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initialize IS31FL3730 device.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Is31fl3730(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /*
        The Pimoroni devices support three I2C devices:

            - 0x61 (the default)
            - 0x62
            - 0x63

        For the breakout, this is straightforward. There is a default and you can change the address (in the normal way).

        For the Micro Dot pHAT, there are three pairs installed (making six matrices). The addresses are ordered this way:

           x63       x62        x61
        ________  _________  ________
        m2 | m1 || m2 | m1 || m2 | m1

        This ordering makes sense if you are scrolling content right to left.

        More detailed information about the structure of each pair follows (further down).
        */

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public const int DefaultI2cAddress = 0x61;

        /// <summary>
        /// Enables or disables auto-buffering.
        /// </summary>
        public bool BufferingEnabled { get; set; } = true;

        /// <summary>
        /// Indexer for matrices.
        /// </summary>
        public DotMatrix5x7 this[int index] => GetMatrix(index);

        /// <summary>
        /// Update bright of LED matrix for each of 128 items.
        /// </summary>
        // From datasheet:
        // Set high bit for the 128th item
        // Set high bit low for the remaining 127 items
        // And then set lower bits as appropriate, to represent 0-127
        public void UpdateBrightness(byte value)
        {
            if (value > 128)
            {
                throw new ArgumentException($"{nameof(value)} must be between 0-128. {nameof(value)} was: {value}.");
            }

            Write(Is31fl3730FunctionRegister.Pwm, value);
        }

        /// <summary>
        /// Update current setting for each row output of LED matrix (default value is 40 mA).
        /// </summary>
        public void UpdateCurrent(Current value)
        {
            if (!Enum.IsDefined(typeof(Current), value))
            {
                throw new ArgumentException($"{value} is not a valid integer value for {nameof(Current)}.");
            }

            Write(Is31fl3730FunctionRegister.LightingEffect, (byte)value);
        }

        /// <summary>
        /// Update configuration register, which controls shutdown, matrix, and display modes.
        /// </summary>
        public void UpdateConfiguration(ShowdownMode shutdown, MatrixMode matrix, DisplayMode display)
        {
            byte configuration = 0;
            configuration |= (byte)shutdown;
            configuration |= (byte)matrix;
            configuration |= (byte)display;
            Write(Is31fl3730FunctionRegister.Configuration, configuration);

            // Set number of and which supported matrices (either or both)
            if (display is DisplayMode.MatrixOneOnly)
            {
                _enabled[0] = true;
                _enabled[1] = false;
            }
            else if (display is DisplayMode.MatrixTwoOnly)
            {
                _enabled[0] = false;
                _enabled[1] = true;
            }
            else
            {
                _enabled[0] = true;
                _enabled[1] = true;
            }
        }

        /// <summary>
        /// Resets all registers to default value.
        /// </summary>
        public void ResetRegisters() => Write(Is31fl3730FunctionRegister.Reset, Is31fl3730MatrixValues.EightBitValue);

        /// <summary>
        /// Write LED for matrix.
        /// </summary>
        /// <param name="matrix">The matrix to use.</param>
        /// <param name="x">The x dimension for the LED.</param>
        /// <param name="y">The y dimension for the LED.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int matrix, int x, int y, PinValue value)
        {
            /*
            The following diagrams and information demonstrate how the matrix is structured.

            This in terms of two products:

            - https://shop.pimoroni.com/products/microdot-phat
            - https://shop.pimoroni.com/products/led-dot-matrix-breakout


            Both of these products present pairs of 5x7 LED matrices.

            *matrix 2*
            xxxxx | xxxxx
            xxxxx | xxxxx
            xxxxx | xxxxx
            xxxxx | xxxxx
            xxxxx | xxxxx
            xxxxx | xxxxx
            xxxxx | xxxxx
            x       x
                    *matrix 1*

            Matrix 1 (as demonstrated) is right-most. This makes sense if you scroll content from right to left.

            Each matrices has a (somewhat odd) dot in the bottom left. It has to be enabled in a special way.

            The matrices are updated with quite different data patterns:

            - matrix 1 is row-based.
            - matrix 2 is column based.

            Let's write a byte to both matrices and see what happens.

            Byte: 00011001

            The following is displayed -- "o" means lit; "x" is unlit.

            *matrix 2*
            oxxxx | oxxoo
            xxxxx | xxxxx
            xxxxx | xxxxx
            oxxxx | xxxxx
            oxxxx | xxxxx
            xxxxx | xxxxx
            xxxxx | xxxxx
            x       x
                    *matrix 1*

            If you write two bytes, then you'll write to the first two rows or
            columns, respectively. With the matrix two, you can only write 5 bytes
            and with matrix one, you can write 7, however, the number of bits that
            are used differs.

            Straightforwardly, both matrices start in the top-left, however
            matrix 1 is row-based, left to right and matrix 2 is column-based
            up to down.

            You will notice these matrices are not symmetrical when you consider
            rows vs columns. Theses matrices are 5x7 not 5x5 or 7x7.

            That means that writing a byte -- 1000_0000 -- with just the high bit set
            won't do anything for either matrix since that bit effectively "falls off".

            Does this matter? Sorta.

            If you are updating one pixel at a time, then you just need to update the
            correct bit, one at a time, either row- or column-based (as you see in
            the code below).

            There are those extra pixels at the bottom, one per matrix. They are addressed
            in the following way. Assume, you have a buffer defined like:

            byte[] buffer = new byte[8]

            To set the extra pixel, ensure the following bit is set:

            - For Matrix 1: buffer[6] = 128;
            - For Matrix 2: buffer[7] = 64;

            buffer[6] is also used for the 7th row, on matrix 1. If you want to light up all
            pixels and the special pixel, then set that buffer value to 159. That's all the bits but two of the highest ones (32 and 64).

            If you have a bitmap you want to write all at once, then you can simply write
            rows of bytes to matrix 1. However, you need to ensure your bitmap only uses
            the first 5 bits. If you are writing to matrix 2 then writing one row at a
            time won't work. Your content will be 90d flipped and the content (if you write
            the same 5-bit encoded bitmap) will be 2 pixels short. Instead, you need to
            translate row-based content to columns, similar to the `else` clause below.

            Separately, you can disable one matrix or the other. That's largely a separate
            concern. It has nothing to do with byte structure.
            */

            CheckDimensions(matrix, x, y);
            int logicalRow = matrix is 0 ? y : x;
            int logicalColumn = matrix is 0 ? x : y;
            byte mask = (byte)(1 << logicalColumn);
            byte[] buffer = _buffers[matrix];
            buffer[logicalRow] = UpdateByte(buffer[logicalRow], mask, (int)value);

            AutoFlush(matrix);
        }

        /// <summary>
        /// Read value of LED for matrix.
        /// </summary>
        /// <param name="matrix">The matrix to use.</param>
        /// <param name="x">The x dimension for the LED.</param>
        /// <param name="y">The y dimension for the LED.</param>
        public PinValue Read(int matrix, int x, int y)
        {
            CheckDimensions(matrix, x, y);
            int row = matrix is 0 ? y : x;
            int column = matrix is 0 ? x : y;
            int mask = 1 << column;
            byte[] m = _buffers[matrix];
            int r = m[row];
            return r & mask;
        }

        /// <summary>
        /// Update decimal point for matrix.
        /// </summary>
        public void WriteDecimalPoint(int matrix, PinValue value)
        {
            CheckMatrixIndex(matrix);
            byte[] buffer = _buffers[matrix];
            int row = matrix is 0 ? Is31fl3730MatrixValues.MatrixOneDecimalRow : Is31fl3730MatrixValues.MatrixTwoDecimalRow;
            byte mask = matrix is 0 ? Is31fl3730MatrixValues.MatrixOneDecimalMask : Is31fl3730MatrixValues.MatrixTwoDecimalMask;
            buffer[row] = UpdateByte(buffer[row], mask, value);
            AutoFlush(matrix);
        }

        /// <summary>
        /// Fill LEDs with value, per matrix.
        /// </summary>
        public void Fill(int matrix, PinValue value)
        {
            CheckMatrixIndex(matrix);
            _buffers[matrix].AsSpan().Fill((byte)(value == PinValue.High ? 255 : 0));
            AutoFlush(matrix);
        }

        /// <summary>
        /// Fill LEDs with value.
        /// </summary>
        public void Fill(PinValue value)
        {
            foreach (int index in Range(0, 2))
            {
                if (_enabled[index])
                {
                    Fill(index, value);
                }
            }
        }

        /// <summary>
        /// Fill LEDs with value, per matrix.
        /// </summary>
        public void Flush(int matrix)
        {
            CheckMatrixIndex(matrix);

            if (_enabled[matrix])
            {
                Write(_matrix_addresses[matrix], _buffers[matrix]);
                WriteUpdateRegister();
            }
        }

        /// <summary>
        /// Fill LEDs with value.
        /// </summary>
        public void Flush()
        {
            Flush(0);
            Flush(1);
        }

        private void Write(byte address, ReadOnlySpan<byte> value)
        {
            Span<byte> data = stackalloc byte[value.Length + 1];
            data[0] = address;
            value.CopyTo(data.Slice(1));
            _i2cDevice.Write(data);
        }

        private void Write(byte address, byte value) => _i2cDevice.Write(stackalloc byte[] { address, value });

        private byte UpdateByte(byte data, byte mask, PinValue value)
        {
            if (value == PinValue.High)
            {
                data |= mask;
            }
            else
            {
                data &= (byte)(~mask);
            }

            return data;
        }

        /*
            Per the datasheet:
            The data sent to the Data Registers will be stored in
            temporary registers. A write operation of any 8-bit value
            to the Update Column Register is required to update
            the Data Registers (01h~0Bh, 0Eh~18h).
        */
        private void WriteUpdateRegister() => Write(Is31fl3730FunctionRegister.UpdateColumn, Is31fl3730MatrixValues.EightBitValue);

        private void AutoFlush(int matrix)
        {
            if (BufferingEnabled)
            {
                Flush(matrix);
            }
        }

        private DotMatrix5x7 GetMatrix(int index)
        {
            CheckMatrixIndex(index);
            return new DotMatrix5x7(this, index);
        }

        private void CheckDimensions(int matrix, int x, int y)
        {
            CheckMatrixIndex(matrix);

            if (x < 0 || x >= DotMatrix5x7.BaseWidth)
            {
                throw new ArgumentException($"{nameof(x)} ({x}) value out of range.");
            }

            if (y < 0 || y >= DotMatrix5x7.BaseHeight)
            {
                throw new ArgumentException($"{nameof(y)} ({y}) value out of range.");
            }
        }

        private void CheckMatrixIndex(int index)
        {
            if (index is < 0 or > 1)
            {
                throw new ArgumentException($"{nameof(index)} ({index}) value out of range.");
            }
        }
    }
}
