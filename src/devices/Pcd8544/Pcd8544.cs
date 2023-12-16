// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Device.Pwm;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using Iot.Device.Display.Pcd8544Enums;
using Iot.Device.CharacterLcd;
using Iot.Device.Graphics;

namespace Iot.Device.Display
{
    /// <summary>
    /// PCD8544 - 48 × 84 pixels matrix LCD, famous Nokia 5110 screen
    /// </summary>
    public class Pcd8544 : GraphicDisplay, ICharacterLcd, IDisposable
    {
        private const int CharacterWidth = 6;

        /// <summary>
        /// The size of the screen in terms of pixels
        /// </summary>
        public static Size PixelScreenSize => new Size(84, 48);

        /// <summary>
        /// Size of the screen 48 x 84 / 8 in bytes
        /// </summary>
        public const int ScreenBufferByteSize = 504;

        /// <summary>
        /// The size of the screen in terms of characters
        /// </summary>
        public Size Size => new Size(14, 6);

        /// <inheritdoc />
        public override int ScreenWidth => PixelScreenSize.Width;

        /// <inheritdoc />
        public override int ScreenHeight => PixelScreenSize.Height;

        /// <inheritdoc />
        public override PixelFormat NativePixelFormat => PixelFormat.Format1bppBw;

        /// <summary>
        /// Number of bit per pixel for the color
        /// </summary>
        public const int ColorBitPerPixel = 1;

        private readonly byte[] _byteMap = new byte[504];
        private int _dataCommandPin;
        private int _resetPin;
        private int _backlightPin = -1;
        private PwmChannel? _pwmBacklight;
        private SpiDevice _spiDevice;
        private GpioController _controller;
        private bool _shouldDispose;
        private float _backlightVal = 0;
        private bool _invd = false;
        private byte _contrast = 0;
        private int _position;
        private bool _cursorVisible = false;
        private byte _bias;
        private bool _enabled;
        private ScreenTemperature _temperature;
        private Dictionary<char, byte[]> _font = new Dictionary<char, byte[]>();

        /// <summary>
        /// Create Pcd8544
        /// </summary>
        /// <param name="dataCommandPin">The data command pin.</param>
        /// <param name="spiDevice">The SPI device.</param>
        /// <param name="resetPin">The reset pin. Use a negative number if you don't want to use it</param>
        /// <param name="pwmBacklight">The PWM channel for the back light</param>
        /// <param name="gpioController">The GPIO Controller.</param>
        /// <param name="shouldDispose">True to dispose the GPIO controller</param>
        public Pcd8544(int dataCommandPin, SpiDevice spiDevice, int resetPin = -1, PwmChannel? pwmBacklight = null, GpioController? gpioController = null, bool shouldDispose = true)
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
            if (resetPin >= 0)
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

        /// <summary>
        /// Create Pcd8544
        /// </summary>
        /// <param name="dataCommandPin">The data command pin.</param>
        /// <param name="spiDevice">The SPI device.</param>
        /// <param name="resetPin">The reset pin. Use a negative number if you don't want to use it</param>
        /// <param name="backlightPin">The pin back light</param>
        /// <param name="gpioController">The GPIO Controller.</param>
        /// <param name="shouldDispose">True to dispose the GPIO controller</param>
        public Pcd8544(int dataCommandPin, SpiDevice spiDevice, int resetPin = -1, int backlightPin = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            if (dataCommandPin < 0)
            {
                throw new ArgumentException($"{nameof(dataCommandPin)} must be a valid pin number");
            }

            _dataCommandPin = dataCommandPin;
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _shouldDispose = gpioController == null || shouldDispose;
            _controller = gpioController ?? new();
            _resetPin = resetPin;
            if (resetPin >= 0)
            {
                _controller.OpenPin(resetPin, PinMode.Output);
                _controller.Write(resetPin, PinValue.Low);
                // Doc says at least 100 ns
                Thread.Sleep(1);
                _controller.Write(resetPin, PinValue.High);
            }

            _backlightPin = backlightPin;
            if (backlightPin >= 0)
            {
                _controller.OpenPin(backlightPin, PinMode.Output);
                _controller.Write(backlightPin, PinValue.Low);
            }

            _controller.OpenPin(_dataCommandPin, PinMode.Output);

            Initialize();
        }

        private void Initialize()
        {
            // Load the font
            Font5x8 font8 = new Font5x8();
            foreach (var charac in font8.SupportedChars)
            {
                font8.GetCharData((char)charac, out ReadOnlySpan<ushort> charData);
                byte[] font8Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    font8Bytes[i] = (byte)charData[i];
                }

                CreateCustomCharacter(charac, font8Bytes);
            }

            _bias = 4;
            _temperature = ScreenTemperature.Coefficient0;
            _contrast = 0x30;
            _enabled = true;
            // Extended function, contrast to 0x30, temperature to coef 0, bias to 4, Screen to normal display power on, display to normal mode
            Span<byte> toSend = stackalloc byte[] { (byte)(FunctionSet.PowerOn | FunctionSet.ExtendedMode), (byte)(0x80 | _contrast), (byte)ScreenTemperature.Coefficient0, (byte)(0x10 | _bias), (byte)FunctionSet.PowerOn, (byte)PcdDisplayControl.NormalMode };
            SpiWrite(false, toSend);
            Clear();
            Draw();
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
                Span<byte> toSend = stackalloc byte[] { enab };
                SpiWrite(false, toSend);
            }
        }

        /// <summary>
        /// Change the back light from 0.0 to 1.0.
        /// If a pin is used, the threshold for full light is more then 0.5.
        /// </summary>
        public float BacklightBrightness
        {
            get => _backlightVal;
            set
            {
                _backlightVal = value > 1 ? 1 : value;
                _backlightVal = _backlightVal < 0 ? 0 : _backlightVal;
                if (_pwmBacklight != null)
                {
                    _pwmBacklight.DutyCycle = _backlightVal;
                }

                if (_backlightPin >= 0)
                {
                    _controller.Write(_backlightPin, _backlightVal > 0.5 ? PinValue.High : PinValue.Low);
                }
            }
        }

        /// <summary>
        /// The bias for 0 to 7. Bias represent the voltage applied to the LCD. The highest, the darker the screen will be.
        /// </summary>
        public byte Bias
        {
            get => _bias;
            set
            {
                _bias = value < 8 ? value : throw new ArgumentException($"Bias can't be more than 7");
                Span<byte> toSend = stackalloc byte[] { (byte)(_enabled ? FunctionSet.PowerOn | FunctionSet.ExtendedMode : FunctionSet.PowerOff | FunctionSet.ExtendedMode), (byte)(0x10 | _bias) };
                SpiWrite(false, toSend);
            }
        }

        /// <summary>
        /// True to inverse the screen color
        /// </summary>
        public bool InvertedColors
        {
            get => _invd;
            set
            {
                _invd = value;
                Span<byte> toSend = _invd ? stackalloc byte[] { (byte)(_enabled ? FunctionSet.PowerOn : FunctionSet.PowerOff), (byte)PcdDisplayControl.InverseVideoMode } : stackalloc byte[] { (byte)FunctionSet.PowerOn, (byte)PcdDisplayControl.NormalMode };
                SpiWrite(false, toSend);
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
                Span<byte> toSend = stackalloc byte[] { (byte)(_enabled ? FunctionSet.PowerOn | FunctionSet.ExtendedMode : FunctionSet.PowerOff | FunctionSet.ExtendedMode), (byte)(0x80 | _contrast) };
                SpiWrite(false, toSend);
            }
        }

        /// <summary>
        /// Get or set the temperature coefficient
        /// </summary>
        public ScreenTemperature Temperature
        {
            get => _temperature;
            set
            {
                Span<byte> toSend = stackalloc byte[] { (byte)(_enabled ? FunctionSet.PowerOn | FunctionSet.ExtendedMode : FunctionSet.PowerOff | FunctionSet.ExtendedMode), (byte)value };
                SpiWrite(false, toSend);
            }
        }

        /// <inheritdoc/>
        public bool BacklightOn { get => BacklightBrightness > 0; set => BacklightBrightness = value ? 1.0f : 0.0f; }

        /// <inheritdoc/>
        public bool DisplayOn { get => Enabled; set => Enabled = value; }

        /// <inheritdoc/>
        public bool UnderlineCursorVisible
        {
            get => _cursorVisible;
            set
            {
                // If the cursor was visible, we remove it
                if (_cursorVisible && !value)
                {
                    DrawCursor();
                }

                _cursorVisible = value;
                if (_cursorVisible)
                {
                    DrawCursor();
                }
            }
        }

        /// <summary>
        /// This is not supported on this screen, this function will have no effect
        /// </summary>
        public bool BlinkingCursorVisible { get; set; }

        /// <inheritdoc/>
        public int NumberOfCustomCharactersSupported => char.MaxValue;

        /// <summary>
        /// Add a specific character to the font. It will replace existing embedded font character if it does already exist.
        /// </summary>
        /// <remarks>
        /// Normal font character is a 5 bytes array aligned vertically. If the array is 8 bytes long, it will assume the
        /// font encoding is then on the lower 5 bits of each bytes.
        /// </remarks>
        /// <param name="location">Should be between 0 and <see cref="NumberOfCustomCharactersSupported"/>.</param>
        /// <param name="characterMap">Provide an array of 8 bytes containing the pattern</param>
        public void CreateCustomCharacter(int location, ReadOnlySpan<byte> characterMap)
        {
            if (location >= NumberOfCustomCharactersSupported)
            {
                throw new ArgumentException($"{location} can only be smaller than {NumberOfCustomCharactersSupported}");
            }

            if (characterMap.Length is not 8 or 5)
            {
                throw new ArgumentException($"Character can only be 8 or 5 bytes array");
            }

            byte[] character = characterMap.Length == 8 ?
               LcdCharacterEncodingFactory.ConvertFont8to5bytes(characterMap.ToArray()) :
               characterMap.ToArray();

            if (_font.ContainsKey((char)location))
            {
                _font.Remove((char)location);
            }

            _font.Add((char)location, character);
        }

        /// <summary>
        /// Add a specific character to the font. It will replace existing embedded font character if it does already exist.
        /// </summary>
        /// <remarks>
        /// Normal font character is a 5 bytes array aligned vertically. If the array is 8 bytes long, it will assume the
        /// font encoding is then on the lower 5 bits of each bytes.
        /// </remarks>
        /// <param name="location">Should be between 0 and <see cref="NumberOfCustomCharactersSupported"/>.</param>
        /// <param name="characterMap">Provide an array of 8 bytes containing the pattern</param>
        public void CreateCustomCharacter(int location, byte[] characterMap)
        {
            CreateCustomCharacter(location, new ReadOnlySpan<byte>(characterMap));
        }

        #endregion

        #region Primitive methods

        /// <summary>
        /// Draw what's in memory to the the screen
        /// </summary>
        public void Draw() => SpiWrite(true, _byteMap);

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
            Draw();
        }

        /// <summary>
        /// Set the byte map
        /// </summary>
        /// <param name="byteMap">A 504 sized byte representing the full image</param>
        public void SetByteMap(ReadOnlySpan<byte> byteMap)
        {
            if (byteMap.Length != ScreenBufferByteSize)
            {
                throw new ArgumentException($"{nameof(byteMap)} length have to be {ScreenBufferByteSize} bytes");
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
            foreach (char c in text)
            {
                // We only display specific characters and ignore the rest
                // And only if it's in the screen
                if (_position <= ScreenBufferByteSize - CharacterWidth)
                {
                    WriteChar(c);
                }
            }

            if (_cursorVisible)
            {
                DrawCursor();
            }
        }

        /// <summary>
        /// Write a raw byte stream to the display.
        /// Used if character translation already took place.
        /// </summary>
        /// <param name="text">Text to print</param>
        public void Write(ReadOnlySpan<char> text)
        {
            foreach (var c in text)
            {
                WriteChar(c);
            }

            if (_cursorVisible)
            {
                DrawCursor();
            }
        }

        /// <summary>
        /// Write a raw byte stream to the display.
        /// Used if character translation already took place.
        /// </summary>
        /// <param name="text">Text to print</param>
        public void Write(char[] text)
        {
            foreach (var c in text)
            {
                WriteChar(c);
            }

            if (_cursorVisible)
            {
                DrawCursor();
            }
        }

        private void WriteChar(char c)
        {
            Span<byte> letter = stackalloc byte[CharacterWidth];
            bool isChar = _font.ContainsKey(c);
            if (isChar)
            {
                if (isChar)
                {
                    byte[] font = _font[c];
                    for (int i = 0; i < CharacterWidth - 1; i++)
                    {
                        letter[i] = font[i];
                    }
                }

                letter[5] = 0x00;
                letter.CopyTo(_byteMap.AsSpan(_position));
                SpiWrite(true, letter);
                _position += CharacterWidth;
            }
            else if (c == 0x08)
            {
                // Case of backspace, we go back
                if (_cursorVisible)
                {
                    DrawCursor();
                }

                _position -= CharacterWidth;
                _position = _position < 0 ? 0 : _position;
                SetPosition(_position);
                SpiWrite(true, letter);
                letter.CopyTo(_byteMap.AsSpan(_position));
                SetPosition(_position);
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
            int y = _position / (Size.Width * CharacterWidth) + 1;
            y = y > Size.Height ? Size.Height : y;
            SetCursorPosition(0, y);
        }

        private void DrawCursor()
        {
            // We won't draw the cursor outside of the screen
            if (_position > ScreenBufferByteSize - CharacterWidth)
            {
                return;
            }

            Span<byte> letter = stackalloc byte[CharacterWidth];
            for (int i = 0; i < CharacterWidth - 1; i++)
            {
                _byteMap[_position + i] = (byte)((_byteMap[_position + i] & 0x80) == 0x80 ? _byteMap[_position + i] & 0x7F : _byteMap[_position + i] | 0x80);
                letter[i] = _byteMap[_position + i];
            }

            SpiWrite(true, letter);
            SetPosition(_position);
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
                throw new ArgumentOutOfRangeException(nameof(left), $"The given position is not inside the display. it's 6 raws and 84 columns");
            }

            if (_cursorVisible)
            {
                DrawCursor();
            }

            SetPosition(left * CharacterWidth, top);
            _position = left * CharacterWidth + top * Size.Width * CharacterWidth;
            if (_cursorVisible)
            {
                DrawCursor();
            }
        }

        private void SetPosition(int left, int top)
        {
            Span<byte> toSend = stackalloc byte[] { (byte)(_enabled ? FunctionSet.PowerOn : FunctionSet.PowerOff), (byte)((byte)(SetAddress.XAddress) | left), (byte)((byte)(SetAddress.YAddress) | top) };
            SpiWrite(false, toSend);
        }

        private void SetPosition(int position)
        {
            int top = position / (Size.Width * CharacterWidth);
            int left = position - top * Size.Width * CharacterWidth;
            SetPosition(left, top);
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

            int index = ((x % 84) + (y / 8) * 84);

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

        /// <inheritdoc />
        public override void DrawBitmap(BitmapImage bitmap)
        {
            var data = BitmapToByteArray(bitmap);
            SetByteMap(data);
            Draw();
        }

        private byte[] BitmapToByteArray(BitmapImage bitmap)
        {
            if (bitmap is not object)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if ((bitmap.Width != Pcd8544.PixelScreenSize.Width) || (bitmap.Height != Pcd8544.PixelScreenSize.Height))
            {
                throw new ArgumentException($"{nameof(bitmap)} should be same size as the screen {Pcd8544.PixelScreenSize.Width}x{Pcd8544.PixelScreenSize.Height}");
            }

            byte[] toReturn = new byte[Pcd8544.ScreenBufferByteSize];
            int width = Pcd8544.PixelScreenSize.Width;
            for (int position = 0; position < Pcd8544.ScreenBufferByteSize; position++)
            {
                byte toStore = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    toStore = (byte)(toStore | (((bitmap[position % width, position / width * 8 + bit].ToArgb() & 0xFFFFFF) == 0xFFFFFF ? 0 : 1) << bit));
                }

                toReturn[position] = toStore;
            }

            return toReturn;
        }

        /// <inheritdoc />
        public override bool CanConvertFromPixelFormat(PixelFormat format)
        {
            return format == PixelFormat.Format32bppArgb || format == PixelFormat.Format32bppXrgb;
        }

        /// <inheritdoc />
        public override BitmapImage GetBackBufferCompatibleImage()
        {
            return BitmapImage.CreateBitmap(ScreenWidth, ScreenHeight, PixelFormat.Format32bppArgb);
        }

        #endregion

        private void SpiWrite(bool isData, ReadOnlySpan<byte> toSend)
        {
            _controller.Write(_dataCommandPin, isData ? PinValue.High : PinValue.Low);
            _spiDevice.Write(toSend);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
            else
            {
                if (_controller is object)
                {
                    if (_controller.IsPinOpen(_dataCommandPin))
                    {
                        _controller.ClosePin(_dataCommandPin);
                    }

                    if (_controller.IsPinOpen(_resetPin))
                    {
                        _controller.ClosePin(_resetPin);
                    }

                    if (_controller.IsPinOpen(_backlightPin))
                    {
                        _controller.ClosePin(_backlightPin);
                    }
                }
            }

            _spiDevice?.Dispose();
            _spiDevice = null!;
            _pwmBacklight?.Dispose();
            _pwmBacklight = null!;

            base.Dispose(disposing);
        }
    }
}
