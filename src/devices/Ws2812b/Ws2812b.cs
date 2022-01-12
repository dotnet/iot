// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;

namespace Iot.Device.Ws2812b
{
    /// <summary>
    /// Driver for WS2812B. Intelligent control LED integrated light source
    /// </summary>
    public class Ws2812b : LedStrip
    {
        /// <summary>
        /// Spi clock frequency for WS2812B
        /// </summary>
        public const int SpiClockFrequency = 3000000;

        private const byte ColorBitSize = 4;
        private const byte PixelBitSize = ColorBitSize * 3;

        private const byte Bit0 = 0b0100; // 333ns low(dummy), 333ns high(T0H), 667ns low(T0L)
        private const byte Bit1 = 0b1100; // 667ns high(T1H), 667ns low(T1L)
        private const byte ResetDelay = 105; // 280us low

        private SpiDevice _spiDevice;
        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the WS2812B device.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="length">Number of LEDs</param>
        public Ws2812b(SpiDevice spiDevice, int length) : base(length)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _buffer = new byte[ResetDelay + length * PixelBitSize];
        }

        /// <summary>
        /// Update color data to LEDs
        /// </summary>
        public override void Flush()
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                Color pixel = Pixels[i];

                WriteSequence(FixGamma(pixel.G), _buffer.AsSpan(ResetDelay + i * PixelBitSize + ColorBitSize * 0));
                WriteSequence(FixGamma(pixel.R), _buffer.AsSpan(ResetDelay + i * PixelBitSize + ColorBitSize * 1));
                WriteSequence(FixGamma(pixel.B), _buffer.AsSpan(ResetDelay + i * PixelBitSize + ColorBitSize * 2));
            }

            _spiDevice.Write(_buffer);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null!;
            _buffer = null!;

            base.Dispose();
        }

        private void WriteSequence(byte b, Span<byte> destination)
        {
            var sequence = new byte[]
            {
                (byte)((((b & 0b10000000) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b01000000) != 0 ? Bit1 : Bit0)),
                (byte)((((b & 0b00100000) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b00010000) != 0 ? Bit1 : Bit0)),
                (byte)((((b & 0b00001000) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b00000100) != 0 ? Bit1 : Bit0)),
                (byte)((((b & 0b00000010) != 0 ? Bit1 : Bit0) << ColorBitSize) | ((b & 0b00000001) != 0 ? Bit1 : Bit0)),
            };

            sequence.CopyTo(destination);
        }
    }
}
