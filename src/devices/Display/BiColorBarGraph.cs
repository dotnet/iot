// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents a 24-Segment bargraph that can display multiple colors.
    /// </summary>
    // Product: https://www.adafruit.com/product/1721
    public class BiColorBarGraph : Ht16k33
    {
        private readonly byte[] _displayBuffer = new byte[7];
        private readonly LedColor[] _biColorSegment = new LedColor[24];

        /// <summary>
        /// Initialize BarGraph display
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public BiColorBarGraph(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Indexer for bargraph.
        /// </summary>
        public LedColor this[int index]
        {
            get => _biColorSegment[index];
            set
            {
                _biColorSegment[index] = value;
                UpdateBuffer(index);
                if (BufferingEnabled)
                {
                    Flush();
                }
            }
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void Fill(LedColor color)
        {
            byte fill = 0xFF;
            switch (color)
            {
                case LedColor.Red:
                    _displayBuffer[1] = fill;
                    _displayBuffer[3] = fill;
                    _displayBuffer[5] = fill;
                    break;
                case LedColor.Green:
                    _displayBuffer[2] = fill;
                    _displayBuffer[4] = fill;
                    _displayBuffer[6] = fill;
                    break;
                case LedColor.Yellow:
                    Span<byte> displayBuffer = _displayBuffer;
                    displayBuffer.Fill(fill);
                    displayBuffer[0] = 0x00;
                    break;
                default:
                    break;
            }

            if (BufferingEnabled)
            {
                _i2cDevice.Write(_displayBuffer);
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            _displayBuffer.AsSpan().Clear();
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
            foreach (byte b in data)
            {
                _displayBuffer[startAddress++] = b;
            }

            if (BufferingEnabled)
            {
                Flush();
            }
        }

        /*
            Task: Update data for the bargraph

            The following diagram shows the intended orientation of the bargraph.

      pins  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x  x
            23 22 21 20 19 18 17 16 15 14 13 12 11 10 9  8  7  6  5  4  3  2  1  0

            Each bargraph has three segments, four LEDs each.
            For the 24-segment bargraph, that's six segments of four LEDs.
            Each segment is addressed separately, with 4 bits (of a byte).
            The first and last 4 bits of each byte represent different segments.
            Each bit represents an LED. If the bits are on, the LEDs are on.

            Each bar contains two LEDs. These are separately controlled.
            That means that there are eight bits to consider for each segement,
            four for each color. All of which can be separately on or off.

            There are seven (7) bytes in the buffer.

            The first byte of the buffer is for control/command information.
            It should always be `0` unless specifically sending commands (like for blinking).

            Each of the following bytes are paired, the first for red and the second for green.
            Each of the bytes in the pair are split, the first half for one segment,
            and the second half for the matching segement on the other bargraph.

            The bytes are laid out this way:
            - first segment: first four bits of bytes[2] for red; first four bits of bytes[3] for green
            - second segment: first four bits of bytes[4] for red; first four bits of bytes[5] for green
            - third segment: first four bits of bytes[6] for red; first four bits of bytes[7] for green
            ------------- // boundary of the bargraph units
            - fourth segment: second four bits of bytes[2] for red; second four bits of bytes[3] for green
            - fifth segment: second four bits of bytes[4] for red; second four bits of bytes[5] for green
            - sixth segment: second four bits of bytes[6] for red; second four bits of bytes[7] for green

            This is more obvious if you write some variation of value to the i2cdevice:

            byte[] buffer =
            {
                0, 255, 0, 0, 255, 255, 255
            };
        */
        private void UpdateBuffer(int index)
        {
            // Tasks:
            // Determine the location of the bar (for `index`).
            // Produce a mask for the correct bit (within four bits).
            // bitmask the correct bits dependending on the desired result
            // Some basic math to use:
            // x = index % 4 // which third (for example, for the 24 bar graph, there are six thirds)
            // y = x % 3 // which third of the bar segment to use
            // z = index / 12 // which of the bar segments to use
            LedColor value = _biColorSegment[index];
            int unit = index / 12;
            int segment = index / 4;
            int third = segment % 3;
            int bit = index % 4 + (index / 12) * 4;
            int mask = 1 << bit;
            int bufferIndex = (third * 2) + 1;
            byte red = _displayBuffer[bufferIndex];
            byte green = _displayBuffer[bufferIndex + 1];

            switch (value)
            {
                case LedColor.Off:
                    _displayBuffer[bufferIndex] = (byte)(red & ~mask);
                    _displayBuffer[bufferIndex + 1] = (byte)(green & ~mask);
                    break;
                case LedColor.Red:
                    _displayBuffer[bufferIndex] = (byte)(red ^ mask);
                    break;
                case LedColor.Green:
                    _displayBuffer[bufferIndex + 1] = (byte)(green ^ mask);
                    break;
                case LedColor.Yellow:
                    _displayBuffer[bufferIndex] = (byte)(red ^ mask);
                    _displayBuffer[bufferIndex + 1] = (byte)(green ^ mask);
                    break;
                default:
                    break;
            }
        }
    }
}
