// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iot.Device.Graphics;

namespace Iot.Device.Ili934x
{
    public partial class Ili9341
    {
        /// <summary>
        /// Send a bitmap to the Ili9341 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="sourcePoint">A coordinate point in the source bitmap where copying starts from.</param>
        /// <param name="destinationRect">A rectangle that defines where in the display the bitmap is written. Note that no scaling is done.</param>
        /// <param name="update">True to immediately send the new backbuffer to the screen</param>
        public void DrawBitmap(BitmapImage bm, Point sourcePoint, Rectangle destinationRect, bool update)
        {
            if (bm is null)
            {
                throw new ArgumentNullException(nameof(bm));
            }

            FillBackBufferFromImage(bm, sourcePoint, destinationRect);

            if (update)
            {
                SendFrame(false);
            }
        }

        /// <inheritdoc />
        public override void DrawBitmap(BitmapImage bm)
        {
            int width = (int)ScreenWidth;
            if (width > bm.Width)
            {
                width = bm.Width;
            }

            int height = (int)ScreenHeight;
            if (height > bm.Height)
            {
                height = bm.Height;
            }

            DrawBitmap(bm, new Point(0, 0), new Rectangle(0, 0, width, height), true);
        }

        private void FillBackBufferFromImage(BitmapImage image, Point sourcePoint, Rectangle destinationRect)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!CanConvertFromPixelFormat(image.PixelFormat))
            {
                throw new InvalidOperationException($"{image.PixelFormat} is not a supported pixel format");
            }

            Converters.AdjustImageDestination(image, ref sourcePoint, ref destinationRect);

            Parallel.For(destinationRect.Y, destinationRect.Height + destinationRect.Y, y =>
            {
                var row = _screenBuffer.AsSpan(y * ScreenWidth, ScreenWidth);
                for (int i = destinationRect.X; i < destinationRect.Width + destinationRect.X; i++)
                {
                    int xSource = sourcePoint.X + i - destinationRect.X;
                    int ySource = sourcePoint.Y + y - destinationRect.Y;
                    row[i] = Rgb565.FromRgba32(image[xSource, ySource]);
                }
            });
        }

        /// <summary>
        /// Updates the display with the current screen buffer.
        /// <param name="forceFull">Forces a full update, otherwise only changed screen contents are updated</param>
        /// </summary>
        public void SendFrame(bool forceFull)
        {
            if (forceFull)
            {
                SetWindow(0, 0, ScreenWidth, ScreenHeight);
                SendSPI(MemoryMarshal.Cast<Rgb565, byte>(_screenBuffer));
            }
            else
            {
                int topRow = 0;
                int bottomRow = ScreenHeight;
                int w = ScreenWidth;
                for (int y = 0; y < ScreenHeight; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (!Rgb565.AlmostEqual(_screenBuffer[x + y * w], _previousBuffer[x + y * w], 2))
                        {
                            topRow = y;
                            goto reverse;
                        }
                    }
                }

                // if we get here, there were no screen changes
                UpdateFps();
                return;

                reverse:

                for (int y = ScreenHeight - 1; y >= topRow; y--)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (!Rgb565.AlmostEqual(_screenBuffer[x + y * w], _previousBuffer[x + y * w], 2))
                        {
                            bottomRow = y;
                            goto end;
                        }
                    }
                }

                end:

                SetWindow(0, topRow, w, bottomRow);
                // Send the given number of rows (+1, because including the end row)
                var partialSpan = MemoryMarshal.Cast<Rgb565, byte>(_screenBuffer.AsSpan().Slice(topRow * w, (bottomRow - topRow + 1) * w));
                SendSPI(partialSpan);
            }

            _screenBuffer.CopyTo(_previousBuffer.AsSpan());
            UpdateFps();
        }

        private void UpdateFps()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            TimeSpan ts = now - _lastUpdate;
            if (ts <= TimeSpan.FromMilliseconds(1))
            {
                ts = TimeSpan.FromMilliseconds(1);
            }

            _fps = 1.0 / ts.TotalSeconds;
            _lastUpdate = now;
        }

        /// <summary>
        /// Send an array of pixel data to the display.
        /// </summary>
        /// <param name="pixelData">The data to be sent to the display.</param>
        /// <param name="destinationRect">A rectangle that defines where in the display the data is to be written.</param>
        /// <remarks>This directly sends the data, circumventing the screen buffer</remarks>
        public void SendBitmapPixelData(Span<byte> pixelData, Rectangle destinationRect)
        {
            SetWindow(destinationRect.X, destinationRect.Y, (destinationRect.Right - 1), (destinationRect.Bottom - 1));   // specifiy a location for the rows and columns on the display where the data is to be written
            SendData(pixelData);
            UpdateFps();
        }
    }
}
