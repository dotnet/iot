// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Device.Pwm;
using System.Threading;
using System.Drawing;
using Iot.Device.Display.Pcd8544Enums;

namespace Iot.Device.Display
{
    /// <summary>
    /// PCD8544 - 48 × 84 pixels matrix LCD
    /// </summary>
    public class Pcd8544 : IDisposable
    {
        /// <summary>
        /// Size of the screen 48 x 84 / 8
        /// </summary>
        public const int ScreenSizeBytes = 504;

        /// <summary>
        /// The size of the screen in terms of characters
        /// </summary>
        public static Size Size => new Size(84, 6);

        /// <summary>
        /// The size of the screen in terms of characters
        /// </summary>
        public static Size PixelScreenSize => new Size(84, 48);

        /// <summary>
        /// Number of color per pixel
        /// </summary>
        public const int ColorPerPixel = 1;

        private int _dataCommandPin;
        private int _resetPin;
        private PwmChannel? _pwmBacklight;
        private SpiDevice _spiDevice;
        private GpioController _controller;
        private bool _shouldDispose;
        private float _backlightVal = 0;
        private bool _invd = false;
        private byte _contrast = 0;
        private int _position;

        private byte[] _byteMap = new byte[504];
        private byte _bias;
        private bool _enabled;
        private Temperature _temperature;

        /// <summary>
        /// Create Pcd8544
        /// </summary>
        /// <param name="dataCommandPin">The data command pin.</param>
        /// <param name="resetPin">The reset pin. Use a negative number if you don't want to use it</param>
        /// <param name="spiDevice">The SPI device.</param>
        /// <param name="pwmBacklight">The PWM channel for the back light</param>
        /// <param name="gpioController">The GPIO Controller.</param>
        /// <param name="shouldDispose">True to dispose the GPIO controller</param>
        public Pcd8544(int dataCommandPin, int resetPin, SpiDevice spiDevice, PwmChannel? pwmBacklight = null, GpioController? gpioController = null, bool shouldDispose = true)
        {
            if (dataCommandPin < 0)
            {
                throw new ArgumentException($"{nameof(dataCommandPin)} must be a valid pin number");
            }

            _dataCommandPin = dataCommandPin;
            _pwmBacklight = pwmBacklight;
            _pwmBacklight?.Start();
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _shouldDispose = gpioController == null || shouldDispose;
            _controller = gpioController ?? new();
            _resetPin = resetPin;
            if (resetPin > 0)
            {
                _controller.OpenPin(resetPin, PinMode.Output);
                _controller.Write(resetPin, PinValue.Low);
                // Doc says at least 100 ns
                Thread.Sleep(1);
                _controller.Write(resetPin, PinValue.High);
            }

            _controller.OpenPin(_dataCommandPin, PinMode.Output);

            Initialize();
        }

        private void Initialize()
        {
            _bias = 4;
            _temperature = Temperature.Coefficient0;
            _contrast = 0x30;
            _enabled = true;
            // Extended function, contrast to 0x30, temperature to coef 0, bias to 4, Screen to normal display power on, display to normal mode
            SpiWrite(false, new byte[] { (byte)(FunctionSet.PowerOn | FunctionSet.ExtendedMode), (byte)(0x80 | _contrast), (byte)Temperature.Coefficient0, (byte)(0x10 | _bias), (byte)FunctionSet.PowerOn, (byte)DisplayControl.NormalMode });
            Clear();
            Refresh();
        }

        #region properties

        /// <summary>
        /// Enable the screen
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                byte enab = (byte)(_enabled ? FunctionSet.PowerOn : FunctionSet.PowerOff);
                SpiWrite(false, new byte[] { enab });
            }
        }

        /// <summary>
        /// Change the back light from 0 to 100
        /// </summary>
        public float BacklightBrightness
        {
            get => _backlightVal;
            set
            {
                if (_pwmBacklight != null)
                {
                    _backlightVal = value > 1 ? 1 : value;
                    _backlightVal = _backlightVal < 0 ? 0 : _backlightVal;
                    _pwmBacklight.DutyCycle = _backlightVal;
                }
            }
        }

        /// <summary>
        /// The bias for 0 to 7
        /// </summary>
        public byte Bias
        {
            get => _bias;
            set
            {
                _bias = value < 8 ? value : throw new ArgumentException($"Bias can't be more than 7");
                SpiWrite(false, new byte[] { (byte)(_enabled ? FunctionSet.PowerOn | FunctionSet.ExtendedMode : FunctionSet.PowerOff | FunctionSet.ExtendedMode), (byte)(0x10 | _bias) });
            }
        }

        /// <summary>
        /// True to inverse the screen color
        /// </summary>
        public bool InverseMode
        {
            get => _invd;
            set
            {
                _invd = value;
                SpiWrite(false, _invd ? new byte[] { (byte)(_enabled ? FunctionSet.PowerOn : FunctionSet.PowerOff), (byte)DisplayControl.InverseVideoMode } : new byte[] { (byte)FunctionSet.PowerOn, (byte)DisplayControl.NormalMode });
            }
        }

        /// <summary>
        /// Get or set the contrast from 0 to 127
        /// </summary>
        public byte Contrast
        {
            get => _contrast;
            set
            {
                _contrast = value <= 127 ? value : throw new ArgumentException($"Contrast can only be between 0 and 127");
                SpiWrite(false, new byte[] { (byte)(_enabled ? FunctionSet.PowerOn | FunctionSet.ExtendedMode : FunctionSet.PowerOff | FunctionSet.ExtendedMode), (byte)(0x80 | _contrast) });
            }
        }

        /// <summary>
        /// Get or set the temperature coefficient
        /// </summary>
        public Temperature Temperature
        {
            get => _temperature;
            set => SpiWrite(false, new byte[] { (byte)(_enabled ? FunctionSet.PowerOn | FunctionSet.ExtendedMode : FunctionSet.PowerOff | FunctionSet.ExtendedMode), (byte)value });
        }

        #endregion

        #region Primitive methods

        /// <summary>
        /// Refresh the screen
        /// </summary>
        public void Refresh() => SpiWrite(true, _byteMap);

        /// <summary>
        /// Clear the screen
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _byteMap.Length; i++)
            {
                _byteMap[i] = 0;
            }

            SetCursorPosition(0, 0);
            Refresh();
        }

        /// <summary>
        /// Set the byte map
        /// </summary>
        /// <param name="byteMap">A 504 sized byte representing the full image</param>
        public void SetByteMap(ReadOnlySpan<byte> byteMap)
        {
            if (byteMap.Length != ScreenSizeBytes)
            {
                throw new ArgumentException($"{nameof(byteMap)} length have to be {ScreenSizeBytes} bytes");
            }

            SetCursorPosition(0, 0);
            byteMap.CopyTo(_byteMap);
        }

        #endregion

        #region Text

        /// <summary>
        /// Write text
        /// </summary>
        /// <param name="text">The text to write</param>
        public void Write(string text)
        {
            byte[] letter = new byte[6];
            foreach (char c in text.ToCharArray())
            {
                // We only display specific characters and ignore the rest
                // And only if it's in the screen
                if (_position <= ScreenSizeBytes - 5)
                {
                    if (c >= 0x20 && c <= 0x7F)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            letter[i] = NokiaCharacters.Ascii[c - 0x20][i];
                        }

                        letter[5] = 0x00;
                        SpiWrite(true, letter);
                        _position += 5;
                    }
                }
            }
        }

        /// <summary>
        /// Write text and set cursor position to next line
        /// </summary>
        /// <param name="text">The text to write</param>
        public void WriteLine(string text)
        {
            Write(text);
            // calculate the position
            int y = _position / Size.Width + 1;
            y = y > Size.Height ? Size.Height : y;
            SetCursorPosition(0, y);
        }

        /// <summary>
        /// Write a raw byte stream to the display.
        /// Used if character translation already took place.
        /// </summary>
        /// <param name="text">Text to print</param>
        public void Write(ReadOnlySpan<byte> text)
        {
            var length = text.Length > ScreenSizeBytes - _position ? ScreenSizeBytes - _position : text.Length;
            text.Slice(0, length).CopyTo(_byteMap.AsSpan(_position));
            Refresh();
        }

        /// <summary>
        /// Moves the cursor to an explicit column and row position.
        /// </summary>
        /// <param name="left">The column position from left to right starting with 0 to 83.</param>
        /// <param name="top">The row position from the top starting with 0 to 5.</param>
        /// <exception cref="ArgumentOutOfRangeException">The given position is not inside the display.</exception>
        public void SetCursorPosition(int left, int top)
        {
            if ((left < 0 || left > Size.Width) || (top < 0 || top > Size.Height))
            {
                throw new ArgumentOutOfRangeException($"The given position is not inside the display. it's 6 raws and 84 columns");
            }

            _position = left + top * Size.Width;
            SpiWrite(false, new byte[] { (byte)(_enabled ? FunctionSet.PowerOn : FunctionSet.PowerOff), (byte)((byte)(SetAddress.XAddress) | left), (byte)((byte)(SetAddress.YAddress) | top) });
        }

        #endregion

        #region Drawing points, lines and rectangles

        /// <summary>
        /// Draw a point
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="isOn">True if the point has pixels on, false for off.</param>
        /// <returns>True if success</returns>
        public bool DrawPoint(int x, int y, bool isOn)
        {
            if (x < 0 || x >= 84 || y < 0 || y >= 48)
            {
                return false;
            }

            int index = ((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            if (isOn)
            {
                _byteMap[index] |= bitMask;
            }
            else
            {
                _byteMap[index] &= (byte)~bitMask;
            }

            return true;
        }

        /// <summary>
        /// Draw a point
        /// </summary>
        /// <param name="point">The point to draw.</param>
        /// <param name="isOn">True if the point has pixels on, false for off.</param>
        /// <returns></returns>
        public bool DrawPoint(Point point, bool isOn) => DrawPoint(point.X, point.Y, isOn);

        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="x1">The first point X coordinate.</param>
        /// <param name="y1">The first point Y coordinate.</param>
        /// <param name="x2">The second point X coordinate.</param>
        /// <param name="y2">The second point Y coordinate.</param>
        /// <param name="isOn">True if the line has pixels on, false for off.</param>
        public void DrawLine(int x1, int y1, int x2, int y2, bool isOn)
        {
            // This is a common line drawing algorithm. Read about it here:
            // http://en.wikipedia.org/wiki/Bresenham's_line_algorithm
            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;

            int dx = x2 > x1 ? x2 - x1 : x1 - x2;
            int dy = y2 > x1 ? y2 - y1 : y1 - y2;

            float err = dx - dy, e2;

            // if there is an error with drawing a point or the line is finished get out of the loop!
            while (!((x1 == x2 && y1 == y2) || !DrawPoint(x1, y1, isOn)))
            {
                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="p1">First point coordinate.</param>
        /// <param name="p2">Second point coordinate.</param>
        /// <param name="isOn">True if the line has pixels on, false for off.</param>
        public void DrawLine(Point p1, Point p2, bool isOn) => DrawLine(p1.X, p1.Y, p2.X, p2.Y, isOn);

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="isOn">True if the rectangle has pixels on, false for off.</param>
        /// <param name="isFilled">If it's filled or not.</param>
        public void DrawRectangle(int x, int y, int width, int height, bool isOn, bool isFilled)
        {
            // This will draw points
            int xe = x + width;
            int ye = y + height;

            if (isFilled)
            {
                for (int yy = y; yy != ye; yy++)
                {
                    for (int xx = x; xx != xe; xx++)
                    {
                        DrawPoint(xx, yy, isOn);
                    }
                }
            }
            else
            {
                xe -= 1;
                ye -= 1;

                for (int xx = x; xx != xe; xx++)
                {
                    DrawPoint(xx, y, isOn);
                }

                for (int xx = x; xx <= xe; xx++)
                {
                    DrawPoint(xx, ye, isOn);
                }

                for (int yy = y; yy != ye; yy++)
                {
                    DrawPoint(x, yy, isOn);
                }

                for (int yy = y; yy <= ye; yy++)
                {
                    DrawPoint(xe, yy, isOn);
                }
            }
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="p">The coordinate of the point.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="isOn">True if the rectangle has pixels on, false for off.</param>
        /// <param name="isFilled">If it's filled or not.</param>
        public void DrawRectangle(Point p, Size size, bool isOn, bool isFilled) => DrawRectangle(p.X, p.Y, size.Width, size.Height, isOn, isFilled);

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="isOn">True if the rectangle has pixels on, false for off.</param>
        /// <param name="isFilled">If it's filled or not.</param>
        public void DrawRectangle(Rectangle rectangle, bool isOn, bool isFilled) => DrawRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, isOn, isFilled);

        #endregion

        private void SpiWrite(bool isData, byte[] toSend)
        {
            _controller.Write(_dataCommandPin, isData ? PinValue.High : PinValue.Low);
            _spiDevice.Write(toSend);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller.Dispose();
            }
            else
            {
                if (_controller.IsPinOpen(_dataCommandPin))
                {
                    _controller.ClosePin(_dataCommandPin);
                }

                if (_controller.IsPinOpen(_resetPin))
                {
                    _controller.ClosePin(_resetPin);
                }
            }

            _spiDevice.Dispose();
            _pwmBacklight?.Dispose();
        }
    }
}
