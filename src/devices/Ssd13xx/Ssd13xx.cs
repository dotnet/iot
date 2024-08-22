// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Device.Spi;
using System.Linq;
using Iot.Device.Graphics;
using Iot.Device.Ssd13xx.Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Represents base class for SSD13xx OLED displays
    /// </summary>
    public abstract class Ssd13xx : GraphicDisplay
    {
        // Multiply of screen resolution plus single command byte.
        private const int DefaultBufferSize = 48 * 96 + 1;
        private const double DefaultThreshold = 0.1;
        private byte[] _genericBuffer;

        /// <summary>
        /// Underlying I2C device
        /// </summary>
        protected I2cDevice? I2cDevice { get; set; }

        /// <summary>
        /// Underlying SPI device
        /// </summary>
        protected SpiDevice? SpiDevice { get; set; }

        /// <summary>
        /// Constructs instance of Ssd13xx using an I2C device
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="width">Width of the display, in pixels</param>
        /// <param name="height">Height of the display, in pixels</param>
        protected Ssd13xx(I2cDevice i2cDevice, int width, int height)
        {
            _genericBuffer = new byte[DefaultBufferSize];
            ScreenHeight = height;
            ScreenWidth = width;
            BrightnessThreshold = DefaultThreshold;
            I2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Constructs instance of Ssd13xx using an SPI device
        /// </summary>
        /// <param name="spiDevice">SPI device used to communicate with the device</param>
        /// <param name="width">Width of the display, in pixels</param>
        /// <param name="height">Height of the display, in pixels</param>
        protected Ssd13xx(SpiDevice spiDevice, int width, int height)
        {
            _genericBuffer = new byte[DefaultBufferSize];
            ScreenHeight = height;
            ScreenWidth = width;
            BrightnessThreshold = DefaultThreshold;
            SpiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
        }

        /// <inheritdoc />
        public override int ScreenHeight { get; }

        /// <inheritdoc />
        public override int ScreenWidth { get; }

        /// <summary>
        /// The format of this display group is 1bpp black and white
        /// </summary>
        public override PixelFormat NativePixelFormat => PixelFormat.Format1bppBw;

        /// <summary>
        /// The brightness used to determine whether a pixel shall be black or white.
        /// Any pixel with a brightness below this value will be black, others will be white.
        /// Only affects the next frame. Default is 0.1.
        /// </summary>
        public double BrightnessThreshold { get; set; }

        /// <summary>
        /// This driver can convert all 32 bit formats
        /// </summary>
        /// <param name="format">The format to query</param>
        /// <returns>True if the format is supported</returns>
        public override bool CanConvertFromPixelFormat(PixelFormat format)
        {
            return format == PixelFormat.Format32bppArgb || format == PixelFormat.Format32bppXrgb;
        }

        /// <inheritdoc />
        public override BitmapImage GetBackBufferCompatibleImage()
        {
            return BitmapImage.CreateBitmap(ScreenWidth, ScreenHeight, PixelFormat.Format32bppXrgb);
        }

        /// <summary>
        /// Verifies value is within a specific range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="start">Starting value of range.</param>
        /// <param name="end">Ending value of range.</param>
        /// <returns>Determines if value is within range.</returns>
        internal static bool InRange(uint value, uint start, uint end)
        {
            return (value - start) <= (end - start);
        }

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        public abstract void SendCommand(ISharedCommand command);

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public virtual void SendData(Span<byte> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Span<byte> writeBuffer = SliceGenericBuffer(data.Length + 1);

            writeBuffer[0] = 0x40; // Control byte.
            data.CopyTo(writeBuffer.Slice(1));

            if (I2cDevice != null)
            {
                I2cDevice.Write(writeBuffer);
            }
            else if (SpiDevice != null)
            {
                SpiDevice.Write(writeBuffer);
            }
            else
            {
                throw new InvalidOperationException("No I2C/SPI device available or it has been disposed.");
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                I2cDevice?.Dispose();
                I2cDevice = null!;

                SpiDevice?.Dispose();
                SpiDevice = null!;
            }
        }

        /// <summary>
        /// Acquires span of specific length pointing to the command buffer.
        /// If length of the command buffer is too small it will be reallocated.
        /// </summary>
        /// <param name="length">Requested length</param>
        /// <returns>Span of bytes pointing to the command buffer</returns>
        protected Span<byte> SliceGenericBuffer(int length) => SliceGenericBuffer(0, length);

        /// <summary>
        /// Acquires span of specific length at specific position in command buffer.
        /// If length of the command buffer is too small it will be reallocated.
        /// </summary>
        /// <param name="start">Start index of the requested span</param>
        /// <param name="length">Requested length</param>
        /// <returns>Span of bytes pointing to the command buffer</returns>
        protected Span<byte> SliceGenericBuffer(int start, int length)
        {
            if (_genericBuffer.Length < length)
            {
                var newBuffer = new byte[_genericBuffer.Length * 2];
                _genericBuffer.CopyTo(newBuffer, 0);
                _genericBuffer = newBuffer;
            }

            return _genericBuffer.AsSpan(start, length);
        }

        /// <summary>
        /// Enables or disables the display
        /// </summary>
        /// <param name="enabled">True to enable false to disable</param>
        public void EnableDisplay(bool enabled)
        {
            if (enabled)
            {
                SendCommand(new SetDisplayOn());
            }
            else
            {
                SendCommand(new SetDisplayOff());
            }
        }

        /// <summary>
        /// Sets the display memory address back to the beginning, to start the next frame
        /// </summary>
        protected abstract void SetStartAddress();

        /// <summary>
        /// Sends the image to the screen
        /// </summary>
        /// <param name="image">Image to display.</param>
        public override void DrawBitmap(BitmapImage image)
        {
            if (!CanConvertFromPixelFormat(image.PixelFormat))
            {
                throw new InvalidOperationException($"{image.PixelFormat} is not a supported pixel format");
            }

            SetStartAddress();

            int width = ScreenWidth;
            Int16 pages = 4;
            List<byte> buffer = new();

            for (int page = 0; page < pages; page++)
            {
                for (int x = 0; x < width; x++)
                {
                    int bits = 0;
                    for (byte bit = 0; bit < 8; bit++)
                    {
                        bits = bits << 1;
                        bits |= image[x, page * 8 + 7 - bit].GetBrightness() > BrightnessThreshold ? 1 : 0;
                    }

                    buffer.Add((byte)bits);
                }
            }

            int chunk_size = 16;
            for (int i = 0; i < buffer.Count; i += chunk_size)
            {
                SendData(buffer.Skip(i).Take(chunk_size).ToArray());
            }
        }

        /// <summary>
        /// Returns the display-ready span of bytes for a bitmap without sending the data to the display
        /// </summary>
        /// <param name="image">Image to render</param>
        public virtual byte[] PreRenderBitmap(BitmapImage image)
        {
            if (!CanConvertFromPixelFormat(image.PixelFormat))
            {
                throw new InvalidOperationException($"{image.PixelFormat} is not a supported pixel format");
            }

            int width = ScreenWidth;
            int pages = image.Height / 8;
            List<byte> buffer = new();

            for (int page = 0; page < pages; page++)
            {
                for (int x = 0; x < width; x++)
                {
                    int bits = 0;
                    for (byte bit = 0; bit < 8; bit++)
                    {
                        bits = bits << 1;
                        bits |= image[x, page * 8 + 7 - bit].GetBrightness() > BrightnessThreshold ? 1 : 0;
                    }

                    buffer.Add((byte)bits);
                }
            }

            return buffer.ToArray();
        }
    }
}
