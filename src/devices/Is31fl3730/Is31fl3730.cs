// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/microdot-phat
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class Is31fl3730 : IDisposable
    {
        // Function register
        // table 2 in datasheet
        private const byte CONFIGURATION_REGISTER = 0x0;
        private const byte MATRIX_1_REGISTER = 0x1;
        private const byte MATRIX_2_REGISTER = 0x0E;
        private const byte UPDATE_COLUMN_REGISTER = 0x0C;
        private const byte LIGHTING_EFFECT_REGISTER = 0x0D;
        private const byte PWM_REGISTER = 0x19;
        private const byte RESET_REGISTER = 0x0C;

        // Configuration register
        // table 3 in datasheet
        private const byte SHUTDOWN = 0x80;
        private const byte DISPLAY_MATRIX_ONE = 0x0;
        private const byte DISPLAY_MATRIX_TWO = 0x8;
        private const byte DISPLAY_MATRIX_BOTH = 0x18;
        private const byte MATRIX_8x8 = 0x0;
        private const byte MATRIX_7x9 = 0x1;
        private const byte MATRIX_6x10 = 0x2;
        private const byte MATRIX_5x11 = 0x3;

        // Values
        private readonly byte[] _matrix_registers = new byte[] { MATRIX_1_REGISTER, MATRIX_2_REGISTER };
        private readonly byte[] _disable_all_leds_data = new byte[8];
        private readonly byte[] _enable_all_leds_data = new byte[8];
        private readonly Matrix5x7?[] _matrices = new Matrix5x7[2];
        private readonly List<byte[]> _buffers = new List<byte[]>(2);
        private I2cDevice _i2cDevice;
        private byte[] _buffer1 = new byte[8];
        private byte[] _buffer2 = new byte[8];
        private bool _matrix1Enabled = true;
        private bool _matrix2Enabled = false;
        private int _configurationValue = 0;

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Is31fl3730(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _enable_all_leds_data.AsSpan().Fill(0xff);
            _buffers.Add(_buffer1);
            _buffers.Add(_buffer2);
        }

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        /// <param name="width">The width of the LED matrix.</param>
        /// <param name="height">The height of the LED matrix.</param>
        public Is31fl3730(I2cDevice i2cDevice, int width, int height)
        : this(i2cDevice)
        {
            Width = width;
            Height = height;
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
        public static readonly int DefaultI2cAddress = 0x61;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 5;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 7;

        /// <summary>
        /// Brightness of LED matrix (override default value (40 mA); set before calling Initialize method).
        /// </summary>
        public int Brightness = 0;

        /// <summary>
        /// Full current setting for each row output of LED matrix (override default value (128; max brightness); set before calling Initialize method).
        /// </summary>
        public int CurrentSetting = 0;

        /// <summary>
        /// Indexer for matrix.
        /// </summary>
        public Matrix5x7? this[int matrix] => _matrices[matrix];

        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        public void Initialize()
        {
            if (_configurationValue > 0)
            {
                WriteConfiguration();
            }

            if (CurrentSetting > 0)
            {
                _i2cDevice.Write(new byte[] { LIGHTING_EFFECT_REGISTER, (byte)CurrentSetting });
            }

            if (Brightness > 0)
            {
                _i2cDevice.Write(new byte[] { PWM_REGISTER, (byte)Brightness });
            }

            _matrices[0] = _matrix1Enabled ? new Matrix5x7(this, 0) : null;
            _matrices[1] = _matrix2Enabled ? new Matrix5x7(this, 1) : null;
        }

        /// <summary>
        /// Fill all LEDS.
        /// </summary>
        public void Fill(int matrix, int value)
        {
            byte[] buffer = _buffers[matrix];

            if (value > 0)
            {
                buffer.AsSpan().Fill(255);
            }
            else
            {
                buffer.AsSpan().Fill(0);
            }

            Write(_matrix_registers[matrix], _buffer1);
            WriteUpdateRegister();
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void EnableAllLeds()
        {
            if (_matrix1Enabled)
            {
                Write(MATRIX_1_REGISTER, _enable_all_leds_data);
            }

            if (_matrix2Enabled)
            {
                Write(MATRIX_2_REGISTER, _enable_all_leds_data);
            }

            WriteUpdateRegister();
        }

        /// <summary>
        /// Disable all LEDs.
        /// </summary>
        public void DisableAllLeds()
        {
            if (_matrix1Enabled)
            {
                Write(MATRIX_1_REGISTER, _disable_all_leds_data);
            }

            if (_matrix2Enabled)
            {
                Write(MATRIX_2_REGISTER, _disable_all_leds_data);
            }

            WriteUpdateRegister();
        }

        /// <summary>
        /// Set display mode. Call before Initialize method.
        /// </summary>
        public void SetDisplayMode(bool matrixOne, bool matrixTwo)
        {
            _matrix1Enabled = matrixOne;
            _matrix2Enabled = matrixTwo;

            _configurationValue |= (matrixOne, matrixTwo) switch
            {
                (true, true) => DISPLAY_MATRIX_BOTH,
                (true, _) => DISPLAY_MATRIX_ONE,
                (_, true) => DISPLAY_MATRIX_TWO,
                _ => throw new Exception("Invalid input.")
            };
        }

        /// <summary>
        /// Set matrix mode. Call before Initialize method.
        /// </summary>
        public void SetMatrixMode(MatrixMode mode)
        {
            _configurationValue |= mode switch
            {
                MatrixMode.Size5x11 => MATRIX_5x11,
                MatrixMode.Size6x10 => MATRIX_6x10,
                MatrixMode.Size7x9 => MATRIX_7x9,
                MatrixMode.Size8x8 => MATRIX_8x8,
                _ => throw new Exception("Invalid input.")
            };
        }

        /// <summary>
        /// Reset device.
        /// </summary>
        public void Reset()
        {
            // Reset device
            Shutdown(true);
            Thread.Sleep(10);
            Shutdown(false);
        }

        /// <summary>
        /// Set the shutdown mode.
        /// </summary>
        /// <param name="shutdown">Set the showdown mode. `true` sets device into shutdown mode. `false` sets device into normal operation.</param>
        public void Shutdown(bool shutdown)
        {
            // mode values
            // 0 = normal operation
            // 1 = shutdown mode
            if (shutdown)
            {
                _configurationValue |= SHUTDOWN;
            }
            else
            {
                _configurationValue &= ~SHUTDOWN;
            }

            WriteUpdateRegister();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        private void WriteConfiguration()
        {
            Write(CONFIGURATION_REGISTER, (byte)_configurationValue);
        }

        private void Write(byte address, byte[] value)
        {
            byte[] data = new byte[value.Length + 1];
            data[0] = address;
            value.CopyTo(data.AsSpan(1));
            _i2cDevice.Write(data);
        }

        private void Write(byte address, byte value)
        {
            _i2cDevice.Write(new byte[] { address, value });
        }

        internal void WriteLed(int matrix, int x, int y, int enable)
        {
            /*
            The following diagrams and information demonstrate how the matrix is structured.

            This in terms of two products:

            - https://shop.pimoroni.com/products/microdot-phat
            - https://shop.pimoroni.com/products/led-dot-matrix-breakout


            Both of these products present pairs of 5x7 LED mattrices.

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

            int row = matrix is 0 ? y : x;
            int column = matrix is 0 ? x : y;
            byte mask = (byte)(1 << column);
            byte[] m = _buffers[matrix];
            m[row] = UpdateByte(m[row], mask, enable);

            UpdateMatrixRegisters();
        }

        private byte UpdateByte(byte data, byte mask, int enable)
        {
            if (enable is 1)
            {
                data |= mask;
            }
            else
            {
                data &= (byte)(~mask);
            }

            return data;
        }

        internal int ReadLed(int matrix, int x, int y)
        {
            int row = matrix is 0 ? y : x;
            int column = matrix is 0 ? x : y;
            int mask = 1 << column;
            byte[] m = _buffers[matrix];
            int r = m[row];
            return r & mask;
        }

        private void WriteUpdateRegister()
        {
            Write(UPDATE_COLUMN_REGISTER, 0x80);
        }

        private void UpdateMatrixRegisters()
        {
            if (_matrix1Enabled)
            {
                Write(MATRIX_1_REGISTER, _buffer1);
            }

            if (_matrix2Enabled)
            {
                Write(MATRIX_2_REGISTER, _buffer2);
            }

            WriteUpdateRegister();
        }
    }
}
