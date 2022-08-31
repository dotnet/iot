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
            // Note: first byte is command data; does not affect display
            // As structured, this API is an advanced scenario, based
            // on the complex data structure described in `UpdateBuffer`.
            // Instread, this API could be updated to write "rows of data".
            foreach (byte b in data)
            {
                _displayBuffer[startAddress++] = b;
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

            The underlying data is structured with each
            long side taking two bytes.

            For example a value of `1` for the first byte
            will light up the top right LED.
            A value of `1` for the second byte will light up
            the right-most value in the 8th row.

            In terms of orientation, this is the top of the unit
            where the pins are.

            The diagram demonstrates the layout of the unit and
            the underlying data structure that supports it.

                      ← y →
            1513119 7 5 3 1 ↑
            x x x x x x x x x
            x x x x x x x x ↓
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            161412108 6 4 2
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x
            x x x x x x x x

            In terms of addressing (for this class), we'll use the
            expected/intuitive scheme.

            7 6 5 4 3 2 1 0
            x x x x x x x x 0
            x x x x x x x x 15

            We now need to marry those together.
            We need to accomodate two things:
            - The bytes are structured first up/down not right/left
            - x values >=8 jump to the next byte

            y values -> bytes
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
            int column = x > 7 ? (y * 2) + 2 : (y * 2) + 1;
            // Same thing here; get an 8-bit mask
            int mask = x > 7 ? 1 << x - 8 : 1 << x;
            byte data = _displayBuffer[column];

            if (value > 0)
            {
                // Ensure bit is set
                _displayBuffer[column] = (byte)(data ^ mask);
            }
            else
            {
                // Unset bit
                _displayBuffer[column] = (byte)(data & ~mask);
            }
        }
    }
}
