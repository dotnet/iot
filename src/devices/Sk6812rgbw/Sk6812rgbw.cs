// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;

namespace Iot.Device.Sk6812rgbw
{
    /// <summary>
    /// Driver for SK6812RGBW. acknowledgment chip-on-top SMD type LED
    /// </summary>
    public class Sk6812rgbw : IDisposable
    {
        /// <summary>
        /// Spi clock frequency for SK6812RGBW
        /// </summary>
        public const int SpiClockFrequency = 3500000;

        /// <summary>
        /// Colors of LEDs
        /// <para>Channel alpha is for white</para>
        /// </summary>
        public Color[] Pixels { get; private set; }

        private const byte ColorBitSize = 4;
        private const byte PixelBitSize = ColorBitSize * 4;

        private const byte Bit0 = 0b1000; // about 300ns high(T0H), 900ns low(T0L)
        private const byte Bit1 = 0b1100; // about 600ns high(T1H), 600ns low(T1L)
        private const byte ResetDelay = 50; // more than 80us low

        private SpiDevice _spiDevice;
        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the SK6812RGBW device.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="length">Number of LEDs</param>
        /// <param name="resetSignal">Transmits a reset signal after the data transmission. For some devices, it can be set to false to improve transmission efficiency</param>
        public Sk6812rgbw(SpiDevice spiDevice, int length, bool resetSignal = true)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _buffer = new byte[(length * PixelBitSize) + (resetSignal ? ResetDelay : 0)];
            Pixels = new Color[length];
        }

        /// <summary>
        /// Update color data to LEDs
        /// </summary>
        public void Flush()
        {
            for (var i = 0; i < Pixels.Length; i++)
            {
                var pixel = Pixels[i];

                WriteSequence(pixel.G, _buffer.AsSpan((i * PixelBitSize) + (ColorBitSize * 0)));
                WriteSequence(pixel.R, _buffer.AsSpan((i * PixelBitSize) + (ColorBitSize * 1)));
                WriteSequence(pixel.B, _buffer.AsSpan((i * PixelBitSize) + (ColorBitSize * 2)));
                WriteSequence(pixel.A, _buffer.AsSpan((i * PixelBitSize) + (ColorBitSize * 3)));
            }

            _spiDevice.Write(_buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null!;
            _buffer = null!;
        }

        private static void WriteSequence(byte b, Span<byte> destination)
        {
            destination[0] = (byte)((((b & 0b10000000) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b01000000) != 0 ? Bit1 : Bit0));
            destination[1] = (byte)((((b & 0b00100000) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b00010000) != 0 ? Bit1 : Bit0));
            destination[2] = (byte)((((b & 0b00001000) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b00000100) != 0 ? Bit1 : Bit0));
            destination[3] = (byte)((((b & 0b00000010) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b00000001) != 0 ? Bit1 : Bit0));
        }
    }
}
