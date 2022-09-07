// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an 8x8 LED matrix using the HT16K33 LED Matrix driver.
    /// </summary>
    // Product: https://www.adafruit.com/product/2042
    public class Matrix8x8 : Ht16k33
    {
        private readonly byte[] _displayBuffer = new byte[17];

        /// <summary>
        /// Initialize Matrix display
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Matrix8x8(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Width of matrix.
        /// </summary>
        public int Width => 8;

        /// <summary>
        /// Height of matrix.
        /// </summary>
        public int Height => 8;

        /// <summary>
        /// Indexer for updating matrix.
        /// </summary>
        public int this[int x, int y]
        {
            set
            {
                UpdateBuffer(x, y, value);
                FlushIfBuffering();
            }
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void Fill()
        {
            Span<byte> displayBuffer = _displayBuffer;
            displayBuffer.Fill(0xFF);
            displayBuffer[0] = 0x00;
            FlushIfBuffering();
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            _displayBuffer.AsSpan().Clear();
            FlushIfBuffering();
        }

        /// <inheritdoc/>
        public override void Flush() => _i2cDevice.Write(_displayBuffer);

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> data, int startAddress = 0)
        {
            // startAddress is intended to mean which matrix to write to
            // only 8 bytes are supported at a time
            int rowIndex = 0;
            foreach (byte b in data)
            {
                int row = rowIndex * 2 + 1 + startAddress;
                _displayBuffer[row] = b;
                rowIndex++;
            }

            FlushIfBuffering();
        }

        /// <summary>
        /// Writes bytes to matrix, row by row
        /// </summary>
        public void Write(ReadOnlySpan<byte> data)
        {
            int rowIndex = 0;

            foreach (byte b in data)
            {
                int row = (rowIndex * 2) + 1;
                int highBit = b & 128;
                int shiftByte = b >> 1 | highBit;
                _displayBuffer[row] = (byte)shiftByte;
                rowIndex++;
            }

            FlushIfBuffering();
        }

        private void FlushIfBuffering()
        {
            if (BufferingEnabled)
            {
                Flush();
            }
        }

        private void UpdateBuffer(int x, int y, int value)
        {
            /*
            Task: Update data for 8x16 matrix

            The following diagram shows the intended orientation of the matrix
            and its x,y address scheme.

            LED used:
            - https://www.adafruit.com/product/2043
            - https://cdn-shop.adafruit.com/datasheets/KWM-30881XUGB.pdf

            ← x →
            0,0           7,0
            x x x x x x x x 0 ↑
            x x x x x x x x 1 y
       sdl  x x x x x x x x 2 ↓
       sda  x x x x x x x x 3
       gnd  x x x x x x x x 4
       vcc  x x x x x x x x 5 ↑
            x x x x x x x x 6 y
            x x x x x x x x 7 ↓
            0 1 2 3 4 5 6 7
            ← x →         7,7

            The line splits the first and second matrice.

            The underlying data is structured with each
            long side taking two bytes (split between the two matrices).

            For example a value of `1` for the first byte
            will light up the top left LED.
            A value of `128` for the second byte will light up
            the top right value.

            The diagram demonstrates the layout of the unit and
            the underlying data structure that supports it.

          1 x x x x x x x x
          3 x x x x x x x x
          5 x x x x x x x x
          7 x x x x x x x x
          9 x x x x x x x x
         11 x x x x x x x x
         13 x x x x x x x x
         15 x x x x x x x x

            */

            // Is x is greater than one matrix/byte
            // then jump to the next matrix/byte
            int row = y * 2 + 1;
            // Same thing here; get an 8-bit mask
            int mask = 1 << (x + 7) % 8;
            byte data = _displayBuffer[row];

            if (value > 0)
            {
                // Ensure bit is set
                _displayBuffer[row] = (byte)(data | mask);
            }
            else
            {
                // Unset bit
                _displayBuffer[row] = (byte)(data & ~mask);
            }
        }
    }
}
