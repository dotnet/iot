// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - LED matrix (I2C)
    /// </summary>
    public class SenseHatLedMatrixI2c : SenseHatLedMatrix
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x46;

        private const int PixelLength = 3;
        private const int FrameBufferLength = PixelLength * NumberOfPixelsPerRow * NumberOfPixelsPerColumn;
        private const int ROffset = 0;
        private const int GOffset = 8;
        private const int BOffset = 16;
        private I2cDevice _i2c;

        /// <summary>
        /// Constructs instance of SenseHatLedMatrixI2c
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        public SenseHatLedMatrixI2c(I2cDevice i2cDevice = null)
        {
            _i2c = i2cDevice ?? CreateDefaultI2cDevice();
            Fill(Color.Black);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<Color> colors)
        {
            if (colors.Length != NumberOfPixels)
            {
                throw new ArgumentException($"`{nameof(colors)}` must have exactly {NumberOfPixels} elements.");
            }

            Span<byte> buffer = stackalloc byte[FrameBufferLength + 1];

            // Register address of first pixel
            buffer[0] = 0;

            Span<byte> frameBuffer = buffer.Slice(1);

            for (int i = 0; i < NumberOfPixels; i++)
            {
                (int x, int y) = IndexToPosition(i);
                int address = PositionToAddress(x, y);
                (byte r, byte g, byte b) = DestructColor(colors[i]);
                frameBuffer[address + ROffset] = r;
                frameBuffer[address + GOffset] = g;
                frameBuffer[address + BOffset] = b;
            }

            _i2c.Write(buffer);
        }

        /// <inheritdoc/>
        public override void Fill(Color color = default(Color))
        {
            Span<byte> buffer = stackalloc byte[FrameBufferLength + 1];

            // Register address of first pixel
            buffer[0] = 0;

            Span<byte> frameBuffer = buffer.Slice(1);
            const int channelLength = NumberOfPixelsPerRow;
            (byte r, byte g, byte b) = DestructColor(color);

            for (int y = 0; y < NumberOfPixelsPerColumn; y++)
            {
                for (int x = 0; x < NumberOfPixelsPerRow; x++)
                {
                    frameBuffer[x] = r;
                }

                frameBuffer = frameBuffer.Slice(channelLength);

                for (int x = 0; x < NumberOfPixelsPerRow; x++)
                {
                    frameBuffer[x] = g;
                }

                frameBuffer = frameBuffer.Slice(channelLength);

                for (int x = 0; x < NumberOfPixelsPerRow; x++)
                {
                    frameBuffer[x] = b;
                }

                frameBuffer = frameBuffer.Slice(channelLength);
            }

            _i2c.Write(buffer);
        }

        /// <inheritdoc/>
        public override void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= NumberOfPixelsPerRow)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0 || y >= NumberOfPixelsPerColumn)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            (byte r, byte g, byte b) = DestructColor(color);

            int address = PositionToAddress(x, y);
            Span<byte> encoded = stackalloc byte[2];

            encoded[0] = (byte)(address + ROffset);
            encoded[1] = r;
            _i2c.Write(encoded);

            encoded[0] = (byte)(address + GOffset);
            encoded[1] = g;
            _i2c.Write(encoded);

            encoded[0] = (byte)(address + BOffset);
            encoded[1] = b;
            _i2c.Write(encoded);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (byte r, byte g, byte b) DestructColor(Color color)
        {
            // 5-bit (shift by 3) look much closer to native driver (SysFs implementation)
            // but this driver is directly reflecting the registers so we will leave the
            // original version
            byte r = (byte)(color.R >> 2);
            byte g = (byte)(color.G >> 2);
            byte b = (byte)(color.B >> 2);
            return (r, g, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int PositionToAddress(int x, int y)
        {
            return y * 24 + x;
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }
    }
}
