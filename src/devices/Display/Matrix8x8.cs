// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an 8x8 LED matrix using the HT16K33 LED Matrix driver.
    /// </summary>
    // Product: https://www.adafruit.com/product/1049
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
            // startAddress is ignored
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
            Task: Update data for 8x8 matrix

            The following diagram shows the intended orientation of the matrix
            and its x,y address scheme.

            LED used:
            - https://www.adafruit.com/product/1819
            - https://www.adafruit.com/product/871

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

            The "Mini" and "Small" LED matriced are structed 90d different.
            The diagram above is for the "Small" LED matrix.
            For the "Mini" matrix, the 0,0 position is 90d to the left.
            The binding hasn't been updated to accomodate that difference.
            If you look at the Adafruit images, you'll see this same 90d difference.

            The underlying data is structured with every second byte affecting a
            row in the matrix. This is demonstrated below.

          1 x x x x x x x x
          3 x x x x x x x x
          5 x x x x x x x x
          7 x x x x x x x x
          9 x x x x x x x x
         11 x x x x x x x x
         13 x x x x x x x x
         15 x x x x x x x x

            The underlying bits in each byte you assigned are off by one.
            The bit for the first (left-most) LED is not the highest bit but the second one.
            The right-most LED is controlled by the highest bit.

            For example, if you set the buferr, to the following value,
            it will create a diagonal line from top left to bottom right.

            0, 64, 0, 32, 0, 16, 0, 8, 0, 4, 0, 2, 0, 1, 0, 128

            The following buffer would create a diagonal line in the opposite direction,
            starting from top right towards bottom left.

            0, 128, 0, 1, 0, 2, 0, 4, 0, 8, 0, 16, 0, 32, 0, 64
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
