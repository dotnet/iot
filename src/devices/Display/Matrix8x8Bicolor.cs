// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an 8x8 LED matrix using the HT16K33 LED Matrix driver.
    /// </summary>
    // Product: https://www.adafruit.com/product/902
    public class Matrix8x8Bicolor : Ht16k33
    {
        private readonly byte[] _displayBuffer = new byte[17];

        /// <summary>
        /// Initialize Matrix display
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Matrix8x8Bicolor(I2cDevice i2cDevice)
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
        public LedColor this[int x, int y]
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
        public void Fill(LedColor color)
        {
            byte fill = 0xFF;
            byte clear = 0;
            byte red;
            byte green;
            switch (color)
            {
                case LedColor.Red:
                    red = fill;
                    green = clear;
                    break;
                case LedColor.Green:
                    red = clear;
                    green = fill;
                    break;
                case LedColor.Yellow:
                    red = fill;
                    green = fill;
                    break;
                default:
                    red = 0;
                    green = 0;
                    break;
            }

            for (int i = 0; i < 8; i++)
            {
                int row = i * 2 + 1;
                _displayBuffer[row] = green;
                _displayBuffer[row + 1] = red;
            }

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
        public void Write(ReadOnlySpan<byte> data, LedColor color)
        {
            int rowIndex = 0;
            byte clear = 0;
            foreach (byte b in data)
            {
                int row = rowIndex * 2 + 1;
                switch (color)
                {
                    case LedColor.Red:
                        _displayBuffer[row] = clear;
                        _displayBuffer[row + 1] = b;
                        break;
                    case LedColor.Green:
                        _displayBuffer[row] = b;
                        _displayBuffer[row + 1] = clear;
                        break;
                    case LedColor.Yellow:
                        _displayBuffer[row] = b;
                        _displayBuffer[row + 1] = b;
                        break;
                    default:
                        break;
                }

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

        private void UpdateBuffer(int x, int y, LedColor color)
        {
            /*
            Task: Update data for 8x8 matrix

            The following diagram shows the intended orientation of the matrix
            and its x,y address scheme.

            LED used:
            - https://www.adafruit.com/product/902
            - https://cdn-shop.adafruit.com/datasheets/BL-M12A883xx.PDF

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

            The underlying data is structured with each
            row taking two bytes, one for green and the other red.

            For example a value of `1` for the first byte
            will light up the top left LED in green.
            A value of `128` for the second byte will light up
            the top right value in red.

            The diagram demonstrates the layout of the unit and
            the underlying data structure (which byte) that supports it.

            x x x x x x x x Green=1; Red=2
            x x x x x x x x Green=3; Red=4
            x x x x x x x x Green=5; Red=6
            x x x x x x x x Green=7; Red=8
            x x x x x x x x Green=9; Red=10
            x x x x x x x x Green=11; Red=12
            x x x x x x x x Green=13; Red=14
            x x x x x x x x Green=15; Red=16
            */

            int row = (y * 2) + 1;
            int mask = 1 << x;
            int green;
            int red;
            switch (color)
            {
                case LedColor.Green:
                    green = _displayBuffer[row] | mask;
                    red = _displayBuffer[row + 1] & ~mask;
                    break;
                case LedColor.Red:
                    green = _displayBuffer[row] & ~mask;
                    red = _displayBuffer[row + 1] | mask;
                    break;
                case LedColor.Yellow:
                    green = _displayBuffer[row] | mask;
                    red = _displayBuffer[row + 1] | mask;
                    break;
                default:
                    green = _displayBuffer[row] & ~mask;
                    red = _displayBuffer[row + 1] & ~mask;
                    break;
            }

            _displayBuffer[row] = (byte)green;
            _displayBuffer[row + 1] = (byte)red;
        }
    }
}
