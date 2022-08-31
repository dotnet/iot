// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an 8x16 LED matrix using the HT16K33 LED Matrix driver.
    /// </summary>
    // Product: https://www.adafruit.com/product/2042
    public class Matrix16x8 : Ht16k33
    {
        private byte[] _displayBuffer = new byte[17];

        /// <summary>
        /// Initialize Matrix display
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Matrix16x8(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Indexer for updating matrix.
        /// </summary>
        public int this[int x, int y]
        {
            set
            {
                UpdateBuffer(x, y, value);

                if (BufferingEnabled)
                {
                    Flush();
                }
            }
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void Fill()
        {
            _displayBuffer = new byte[]
            {
                0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
            };

            if (BufferingEnabled)
            {
                _i2cDevice.Write(_displayBuffer);
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            _displayBuffer = new byte[17];

            if (BufferingEnabled)
            {
                _i2cDevice.Write(_displayBuffer);
            }
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
                int column = (rowIndex * 2) + 1 + startAddress;
                _displayBuffer[column] = b;
                rowIndex++;
            }

            if (BufferingEnabled)
            {
                Flush();
            }
        }

        /// <summary>
        /// Writes bytes to matrix, row by row
        /// </summary>
        public void Write(ReadOnlySpan<byte> data)
        {
            int rowIndex = 0;
            foreach (byte b in data)
            {
                int column = rowIndex > 7 ? (rowIndex - 8) * 2 + 2 : rowIndex * 2 + 1;
                _displayBuffer[column] = b;
                rowIndex++;
            }

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

            Note: LED used: https://cdn-shop.adafruit.com/datasheets/KWM-30881XUGB.pdf

            ← x →
            0, 0          7, 0              15, 0
            x x x x x x x x | x x x x x x x x 0 ↑
            x x x x x x x x | x x x x x x x x 1 y
       sdl  x x x x x x x x | x x x x x x x x 2 ↓
       sda  x x x x x x x x | x x x x x x x x 3
       gnd  x x x x x x x x | x x x x x x x x 4
       vcc  x x x x x x x x | x x x x x x x x 5 ↑
            x x x x x x x x | x x x x x x x x 6 y
            x x x x x x x x | x x x x x x x x 7 ↓
            0 1 2 3 4 5 6 7   8 9 101112131415
            ← x →

            The line splits the first and second matrice.

            The underlying data is structured with each
            long side taking two bytes (split between the two matrices).

            For example a value of `1` for the first byte
            will light up the top left LED.
            A value of `128` for the second byte will light up
            the top right value.

            The diagram demonstrates the layout of the unit and
            the underlying data structure that supports it.

          1 x x x x x x x x |  2 x x x x x x x x
          3 x x x x x x x x |  4 x x x x x x x x
          5 x x x x x x x x |  6 x x x x x x x x
          7 x x x x x x x x |  8 x x x x x x x x
          9 x x x x x x x x | 10 x x x x x x x x
         11 x x x x x x x x | 12 x x x x x x x x
         13 x x x x x x x x | 14 x x x x x x x x
         15 x x x x x x x x | 16 x x x x x x x x

            We now need to marry those together.
            We need to accomodate two things:
            - The bytes are structured right/left
            - x values >=8 jump to the next byte

            x - columns
            y - rows

            bytes for rows (counting from top to bottom; y values):
            0 -> 1
            1 -> 3
            2 -> 5
            3 -> 7
            4 -> 9
            5 -> 11
            6 -> 13
            7 -> 15

            x > 7

            0 -> 2
            1 -> 4
            2 -> 6
            3 -> 8
            4 -> 10
            5 -> 12
            6 -> 14
            7 -> 16
            */

            // Is x is greater than one matrix/byte
            // then jump to the next matrix/byte
            int column = x < 8 ? (y * 2) + 1 : (y * 2) + 2;
            // Same thing here; get an 8-bit mask
            int mask = x < 8 ? 1 << x : 1 << (x - 8);
            byte data = _displayBuffer[column];

            if (value > 0)
            {
                // Ensure bit is set
                _displayBuffer[column] = (byte)(data | mask);
            }
            else
            {
                // Unset bit
                _displayBuffer[column] = (byte)(data & ~mask);
            }
        }
    }
}
