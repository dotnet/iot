// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Device;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Iot.Device.Il03xx
{
    public abstract class Il03xx
    {
        private SpiDevice _spi;
        public int Width { get; }
        public int Height { get; }
        private int _busyPin;
        private int _resetPin;
        private int _dataCommandPin;
        private IGpioController _gpioController;


        public Il03xx(SpiDevice spiDevice, int busyPin, int resetPin, int dataCommandPin, Size resolution, IGpioController gpioController = null)
        {
            _spi = spiDevice;
            Width = resolution.Width;
            Height = resolution.Height;
            _busyPin = busyPin;
            _resetPin = resetPin;
            _dataCommandPin = dataCommandPin;
            _gpioController = gpioController ?? new GpioController();
            _gpioController.OpenPin(_resetPin, PinMode.Output);
            _gpioController.Write(_resetPin, PinValue.High);
            _gpioController.OpenPin(_busyPin, PinMode.Input);
            _gpioController.OpenPin(_dataCommandPin, PinMode.Output);
        }

        public abstract void PowerOn();

        protected abstract byte BitsPerBlackPixel { get; }

        public virtual void Clear()
        {
            // Blast ones to clear (white and red1 lookup tables as DDX == 1).
            // Black is two bits per pixel and red is one. If we don't have an
            // even multiple of 8, just send an extra full byte of data.

            int totalPixels = Width * Height;
            int blackPixelsPerByte = 8 / BitsPerBlackPixel;

            int colorBytes = totalPixels % blackPixelsPerByte == 0 ? totalPixels / blackPixelsPerByte : totalPixels / blackPixelsPerByte + 1;
            Send(Commands.DisplayStartTransmission1, 0xFF, colorBytes);
            DelayHelper.DelayMilliseconds(2, allowThreadYield: false);

            // Clear red (1bpp)
            colorBytes = totalPixels % 8 == 0 ? totalPixels / 8 : totalPixels / 8 + 1;
            Send(Commands.DisplayStartTransmission2, 0xFF, colorBytes);
            DelayHelper.DelayMilliseconds(2, allowThreadYield: false);

            Send(Commands.DisplayRefresh);
            WaitForIdle();
        }

        public virtual void SendBitmap(Bitmap bitmap)
        {
            if (bitmap.Width != Width)
                throw new ArgumentOutOfRangeException("bitmap", $"Bitmap width of {bitmap.Width} does not match display width.");

            if (bitmap.Height != Height)
                throw new ArgumentOutOfRangeException("bitmap", $"Bitmap height of {bitmap.Height} does not match display height.");

            // This would likely be much more efficient with LockBits. We can also potentially take a red and a black bitmap.

            byte blackBitsPerPixel = BitsPerBlackPixel;
            int blackPixelsPerByte = 8 / BitsPerBlackPixel;
            Debug.Assert(BitsPerBlackPixel == 1 || BitsPerBlackPixel == 2);

            int totalPixels = Width * Height;
            int blackBytes = totalPixels % blackPixelsPerByte == 0 ? totalPixels / blackPixelsPerByte : totalPixels / blackPixelsPerByte + 1;
            byte blackPixelMask = (byte)(BitsPerBlackPixel == 1 ? 0b_1000_0000 : 0b_1100_0000);
            int colorBytes = totalPixels % 8 == 0 ? totalPixels / 8 : totalPixels / 8 + 1;

            // We don't care about transparency
            const int RgbMask = 0x00FFFFFF;
            const int White = 0x00FFFFFF;
            const int Black = 0x00000000;
            const int Red = 0x00FF0000;
            const int HighBits = 0x00808080;

            byte[] blackData = ArrayPool<byte>.Shared.Rent(blackBytes);
            byte[] colorData = ArrayPool<byte>.Shared.Rent(colorBytes);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int argb = bitmap.GetPixel(x, y).ToArgb() & RgbMask;
                    bool black = false;
                    bool color = false;

                    if (argb == Black)
                    {
                        black = true;
                    }
                    else if (argb == Red)
                    {
                        color = true;
                    }
                    else if (argb == White || (argb & HighBits) == HighBits)
                    {
                        // Everything above 127, consider it white (both false)
                    }
                    else
                    {
                        // Unfortunately text antialiasing options aren't currently enabled for
                        // libgdiplus. https://github.com/mono/libgdiplus/issues/535 This causes
                        // text to be rendered with subpixel rendering which uses color to antialias.
                        int blueValue = argb & 0xFF;
                        if (((argb >> 8) & 0xFF) == blueValue && ((argb >> 16) & 0xFF) == blueValue)
                        {
                            // R, G, and B are equal, some shade of gray
                            black = true;
                        }
                        else
                        {
                            // Has a tint
                            color = true;
                        }
                    }

                    if (black)
                    {
                        // 0 for black
                        blackData[(x + y * Width) / blackPixelsPerByte] &= (byte)~(blackPixelMask >> (x % blackPixelsPerByte) * blackBitsPerPixel);
                    }
                    else
                    {
                        // 1 for white
                        blackData[(x + y * Width) / blackPixelsPerByte] |= (byte)(blackPixelMask >> (x % blackPixelsPerByte) * blackBitsPerPixel);
                    }

                    if (color)
                    {
                        // 0 for color
                        colorData[(x + y * Width) / 8] &= (byte)~(0b_1000_0000 >> x % 8);
                    }
                    else
                    {
                        // 1 for white
                        colorData[(x + y * Width) / 8] |= (byte)(0b_1000_0000 >> x % 8);
                    }
                }
            }

            Send(Commands.DisplayStartTransmission1, new Span<byte>(blackData, 0, blackBytes));
            DelayHelper.DelayMilliseconds(2, allowThreadYield: false);

            Send(Commands.DisplayStartTransmission2, new Span<byte>(colorData, 0, colorBytes));
            DelayHelper.DelayMilliseconds(2, allowThreadYield: false);

            ArrayPool<byte>.Shared.Return(blackData);
            ArrayPool<byte>.Shared.Return(colorData);

            Send(Commands.DisplayRefresh);
            WaitForIdle();
        }

        public virtual void PowerOff()
        {
            Send(Commands.PowerOff);
        }

        public void Reset()
        {
            // Take the reset pin low to reset (what is the exact timing here?)
            _gpioController.Write(_resetPin, PinValue.Low);
            DelayHelper.DelayMilliseconds(200, allowThreadYield: true);
            _gpioController.Write(_resetPin, PinValue.High);
            DelayHelper.DelayMilliseconds(200, allowThreadYield: true);
        }

        public void Send(Commands command)
        {
            _gpioController.Write(_dataCommandPin, PinValue.Low);
            _spi.WriteByte((byte)command);
        }

        public void Send(Commands command, byte value)
        {
            _gpioController.Write(_dataCommandPin, PinValue.Low);
            _spi.WriteByte((byte)command);
            _gpioController.Write(_dataCommandPin, PinValue.High);
            _spi.WriteByte(value);
        }

        public void Send(Commands command, byte value, int count)
        {
            _gpioController.Write(_dataCommandPin, PinValue.Low);
            _spi.WriteByte((byte)command);
            for (int i = 0; i < count; i++)
            {
                _gpioController.Write(_dataCommandPin, PinValue.High);
                _spi.WriteByte(value);
            }
        }

        public void Send(Commands command, Span<byte> data)
        {
            _gpioController.Write(_dataCommandPin, PinValue.Low);
            _spi.WriteByte((byte)command);
            if (!data.IsEmpty)
            {
                Span<byte> remaining = data;
                do
                {
                    data = remaining.Length > 1024 ? remaining.Slice(0, 1024) : remaining;
                    _gpioController.Write(_dataCommandPin, PinValue.High);
                    _spi.Write(data);
                    remaining = remaining.Slice(data.Length);
                } while (!remaining.IsEmpty);
            }
        }

        public void WaitForIdle()
        {
            SpinWait spinWait = new SpinWait();
            while (_gpioController.Read(_busyPin) == PinValue.Low)
            {
                spinWait.SpinOnce();
            }
        }
    }
}
