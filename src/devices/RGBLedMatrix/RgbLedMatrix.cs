﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Runtime.CompilerServices;
using Iot.Device.Graphics;

namespace Iot.Device.LEDMatrix
{
    /// <summary>
    /// Represents RGB LED matrix
    /// </summary>
    public class RGBLedMatrix
    {
        private GpioController _controller;
        private Gpio _gpio; // Gpio handling.
        private PinMapping _mapping; // mapping the pin numbers to the Gpio (R1, G1, B1, OE, Clock, Latch...etc.).
        private int _width; // number of visual pixel columns (as passed by the caller).
        private int _fullChainWidth; // number of physical columns in pixel for the whole chain.
        private int _rows; // number of visual pixel rows.
        private int _deviceRows; // number of rows per one matrix.
        private int _isRendering; // true if we already rendering, false otherwise.
        private bool _safeToDispose; // true if it is safe to dispose the whole object.
        private bool _swapRequested; // used internally when switching between the forground and background buffers.
        private byte[] _colorsBuffer; // Buffer to store all color data used for drawing on the matrix.
        private byte[] _colorsBackBuffer; // Background buffer to store all color data used for drawing on the matrix.
        private ulong[] _rowSetMasks; // masks used to quickly set the row number to Gpio pins.
        private ulong[] _colorsMake; // masks used to quickly set the R, G, B values to Gpio pins.

        private long
            _duration = (long)(Stopwatch.Frequency * 1800 /
                               1E9); // time interval in 100 nanoseconds used in the pulse width modulation (PWM)

        private long _frameTime; // store the time spent to render one frame.
        private bool _startMeasureFrameTime; // enable to measure the time frame
        private int _chainRows; // how many matrices in one column of the chaining.
        private int _chainColumns; // how many matrices in one row of the chaining.

        /// <summary>
        /// Construct a new RGBLedMatrix object
        /// </summary>
        /// <param name="mapping">The Gpio pin mapping</param>
        /// <param name="width">The width in pixels of the matrix display area</param>
        /// <param name="height">The height in pixels of the matrix display area</param>
        /// <param name="chainRows">Number of the matrices rows in the chain</param>
        /// <param name="chainColumns">Number of the matrices columns in the chain</param>
        public RGBLedMatrix(PinMapping mapping, int width, int height, int chainRows = 1, int chainColumns = 1)
        {
            _chainRows = chainRows;
            _chainColumns = chainColumns;

            _mapping = mapping;

            if (width < 8 || height < 8 || height % chainRows != 0)
            {
                throw new ArgumentException("Invalid rows or width values");
            }

            _deviceRows = height / chainRows;

            _gpio = new Gpio(mapping, _deviceRows);
            _controller = new GpioController(_gpio);

            OpenAndWriteToPin(_mapping.A, PinValue.Low);
            OpenAndWriteToPin(_mapping.B, PinValue.Low);
            OpenAndWriteToPin(_mapping.C, PinValue.Low);

            if (_deviceRows > 16)
            {
                OpenAndWriteToPin(_mapping.D, PinValue.Low);
            }

            if (_deviceRows > 32)
            {
                _duration = (long)((double)Stopwatch.Frequency * 400 / 1E9);
                OpenAndWriteToPin(_mapping.E, PinValue.Low);
            }

            _fullChainWidth = width * chainRows;

            if (_fullChainWidth > 32)
            {
                _duration = (long)((double)Stopwatch.Frequency * 400 / 1E9);
            }

            // OE set High means disable output (confusing)
            OpenAndWriteToPin(_mapping.OE, PinValue.High);
            OpenAndWriteToPin(_mapping.Clock, PinValue.Low);
            OpenAndWriteToPin(_mapping.Latch, PinValue.Low);

            OpenAndWriteToPin(_mapping.R1, PinValue.Low);
            OpenAndWriteToPin(_mapping.G1, PinValue.Low);
            OpenAndWriteToPin(_mapping.B1, PinValue.Low);
            OpenAndWriteToPin(_mapping.R2, PinValue.Low);
            OpenAndWriteToPin(_mapping.G2, PinValue.Low);
            OpenAndWriteToPin(_mapping.B2, PinValue.Low);

            _rowSetMasks = new ulong[_deviceRows >> 1];

            for (int i = 1; i < _deviceRows >> 1; i++)
            {
                if ((i & 1) != 0)
                {
                    _rowSetMasks[i] |= _gpio.AMask;
                }

                if ((i & 2) != 0)
                {
                    _rowSetMasks[i] |= _gpio.BMask;
                }

                if ((i & 4) != 0)
                {
                    _rowSetMasks[i] |= _gpio.CMask;
                }

                if ((i & 8) != 0)
                {
                    _rowSetMasks[i] |= _gpio.DMask;
                }

                if ((i & 0x10) != 0)
                {
                    _rowSetMasks[i] |= _gpio.EMask;
                }
            }

            _colorsMake = new ulong[16]; // 8 for RGB1 and 8 for RGB2
            for (int i = 1; i < 8; i++)
            {
                if ((i & 1) != 0)
                {
                    _colorsMake[i] |= _gpio.R1Mask;
                    _colorsMake[i + 8] |= _gpio.R2Mask;
                }

                if ((i & 2) != 0)
                {
                    _colorsMake[i] |= _gpio.G1Mask;
                    _colorsMake[i + 8] |= _gpio.G2Mask;
                }

                if ((i & 4) != 0)
                {
                    _colorsMake[i] |= _gpio.B1Mask;
                    _colorsMake[i + 8] |= _gpio.B2Mask;
                }
            }

            _colorsBuffer = new byte[8 * _fullChainWidth * (_deviceRows >> 1)];
            _colorsBackBuffer = new byte[8 * _fullChainWidth * (_deviceRows >> 1)];

            _width = width;
            _rows = height;

            _safeToDispose = true;
            _swapRequested = false;
            _startMeasureFrameTime = true;
        }

        /// <summary>
        /// Return the width in pixels of the display
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Return the height in pixels of the display
        /// </summary>
        public int Height => _rows;

        /// <summary>
        /// The height in pixels of one matrix in the chain
        /// </summary>
        public int MatrixHeight => _deviceRows;

        /// <summary>
        /// Set or get the time duration in nanoseconds used in the Pulse Width Modulation (PWM).
        /// </summary>
        /// <value></value>
        public long PWMDuration
        {
            get => (long)(((double)_duration / Stopwatch.Frequency) * 1E9);
            set
            {
                _duration = (long)((double)Stopwatch.Frequency * value / 1E9); // value nanoseconds;
                _startMeasureFrameTime = true;
            }
        }

        /// <summary>
        /// Return the time in microseconds used to draw a full display frame
        /// </summary>
        /// <value></value>
        public long FrameTime => _frameTime;

        /// <summary>
        /// Fill a rectangle on the display with specific color
        /// </summary>
        /// <param name="x">Upper left rectangle x coordinate</param>
        /// <param name="y">Upper left rectangle y coordinate</param>
        /// <param name="width">The rectangle width</param>
        /// <param name="height">The rectangle height</param>
        /// <param name="red">red color value</param>
        /// <param name="green">green color value</param>
        /// <param name="blue">blue color value</param>
        /// <param name="backBuffer">true if to draw on back buffer, false to draw on the forground buffer</param>
        public void FillRectangle(int x, int y, int width, int height, byte red, byte green, byte blue,
            bool backBuffer = false)
        {
            byte[] buffer = backBuffer ? _colorsBackBuffer : _colorsBuffer;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    SetPixel(i + x, j + y, red, green, blue, buffer);
                }
            }
        }

        /// <summary>
        /// Fill the whole display area with a specific color
        /// </summary>
        /// <param name="red">red color value</param>
        /// <param name="green">green color value</param>
        /// <param name="blue">blue color value</param>
        /// <param name="backBuffer">true if to draw on back buffer, false to draw on the forground buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Fill(byte red, byte green, byte blue, bool backBuffer = false) =>
            FillRectangle(0, 0, Width, Height, red, green, blue, backBuffer);

        /// <summary>
        /// Set color of specific pixel on the forground buffer display
        /// </summary>
        /// <param name="column">x coordinate of the pixel</param>
        /// <param name="row">y coordinate of the pixel</param>
        /// <param name="red">red color value</param>
        /// <param name="green">green color value</param>
        /// <param name="blue">blue color value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int column, int row, byte red, byte green, byte blue) =>
            SetPixel(column, row, red, green, blue, _colorsBuffer);

        /// <summary>
        /// Set color of specific pixel on the background buffer display
        /// </summary>
        /// <param name="column">x coordinate of the pixel</param>
        /// <param name="row">y coordinate of the pixel</param>
        /// <param name="red">red color value</param>
        /// <param name="green">green color value</param>
        /// <param name="blue">blue color value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBackBufferPixel(int column, int row, byte red, byte green, byte blue) =>
            SetPixel(column, row, red, green, blue, _colorsBackBuffer);

        private void SetPixel(int column, int row, byte red, byte green, byte blue, byte[] colorsBuffer)
        {
            if ((column | row) < 0 || column >= Width || row >= Height)
            {
                return;
            }

            if (_chainRows > 1)
            {
                int panelRow = row / _deviceRows;

                if ((_chainRows & 1U) == 1 ^ (panelRow & 1U) == 0)
                {
                    int panelWidth = Width / _chainColumns;
                    int panelColumn = column / panelWidth;

                    column = column % panelWidth + (_chainColumns - 1 - panelColumn) * panelWidth;
                }

                row = row % _deviceRows;
                column = panelRow * Width + column;
            }

            int pos = 8 * column + 8 * (row % (_deviceRows >> 1)) * _fullChainWidth;
            byte mask = (byte)(row >= (_deviceRows >> 1) ? 0x08 : 0x01);

            for (int i = 0; i < 8; i++)
            {
                int bit = 1 << i;

                if ((red & bit) != 0)
                {
                    colorsBuffer[pos + i] |= mask;
                }
                else
                {
                    colorsBuffer[pos + i] &= (byte)(~mask);
                }

                if ((green & bit) != 0)
                {
                    colorsBuffer[pos + i] |= (byte)(mask << 1);
                }
                else
                {
                    colorsBuffer[pos + i] &= (byte)~(mask << 1);
                }

                if ((blue & bit) != 0)
                {
                    colorsBuffer[pos + i] |= (byte)(mask << 2);
                }
                else
                {
                    colorsBuffer[pos + i] &= (byte)~(mask << 2);
                }
            }
        }

        private void SwapBuffersInternal()
        {
            var temp = _colorsBackBuffer;
            _colorsBackBuffer = _colorsBuffer;
            _colorsBuffer = temp;
        }

        /// <summary>
        /// Swap the forground and background buffers
        /// </summary>
        public void SwapBuffers()
        {
            _swapRequested = true;

            while (_swapRequested)
            {
            }
        }

        /// <summary>
        /// Dispose the object after done using it.
        /// </summary>
        public void Dispose()
        {
            if (_controller is object)
            {
                StopRendering();

                while (!_safeToDispose)
                {
                    Thread.SpinWait(1);
                }

                _controller.Dispose();
                _controller = null!;
            }
        }

        /// <summary>
        /// Start rendering on the display area.
        /// </summary>
        public void StartRendering()
        {
            if (Interlocked.CompareExchange(ref _isRendering, 1, 0) != 0)
            {
                return; // we already rendering
            }

            Task.Factory.StartNew(
                Render,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Stop rendering on the display area.
        /// </summary>
        public void StopRendering() => Interlocked.CompareExchange(ref _isRendering, 0, 1);

        /// <summary>
        /// Draw a bitmap on the matrix display area
        /// </summary>
        /// <param name="x">Upper left x coordinate on the display</param>
        /// <param name="y">Upper left y coordinate on the display</param>
        /// <param name="bitmap">System.Drawing.Bitmap object to draw</param>
        /// <param name="backBuffer">true if want use back buffer, false otherwise</param>
        public void DrawBitmap(int x, int y, BitmapImage bitmap, bool backBuffer = false)
        {
            if (y >= Height || x >= Width || x + bitmap.Width <= 0 || y + bitmap.Height <= 0)
            {
                return;
            }

            byte[] buffer = backBuffer ? _colorsBackBuffer : _colorsBuffer;

            Rectangle partialBitmap = new Rectangle(x, y, bitmap.Width, bitmap.Height);
            partialBitmap.Intersect(new Rectangle(0, 0, Width, Height));

            int bmpStride = bitmap.Stride;
            int pos = 3 * ((y < 0 ? Math.Abs(y) * bitmap.Width : 0) + (x < 0 ? Math.Abs(x) : 0));
            int stride = (bmpStride - 3 * bitmap.Width) + 3 * (bitmap.Width - partialBitmap.Width);

            Span<byte> span = bitmap.AsByteSpan();

            for (int j = 0; j < partialBitmap.Height; j++)
            {
                for (int i = 0; i < partialBitmap.Width; i++)
                {
                    SetPixel(partialBitmap.X + i, partialBitmap.Y + j, span[pos + 2], span[pos + 1], span[pos],
                        buffer);
                    pos += 3;
                }

                pos += stride;
            }
        }

        /// <summary>
        /// Draw a bitmap on the matrix display area.
        /// The drawing will replace any pixel with the color (red, green, blue) by the color (repRed, repGreen, repBlue)
        /// </summary>
        /// <param name="x">Upper left x coordinate on the display</param>
        /// <param name="y">Upper left y coordinate on the display</param>
        /// <param name="bitmap">System.Drawing.Bitmap object to draw</param>
        /// <param name="red">red color to replace</param>
        /// <param name="green">green color to replace</param>
        /// <param name="blue">blue color to replace</param>
        /// <param name="repRed">replacement color for the red color</param>
        /// <param name="repGreen">replacement color for the green color</param>
        /// <param name="repBlue">replacement color for the blue color</param>
        /// <param name="backBuffer">true if want use back buffer, false otherwise</param>
        public void DrawBitmap(int x, int y, BitmapImage bitmap, byte red, byte green, byte blue, byte repRed,
            byte repGreen, byte repBlue, bool backBuffer = false)
        {
            if (y >= Height || x >= Width || x + bitmap.Width <= 0 || y + bitmap.Height <= 0)
            {
                return;
            }

            byte[] buffer = backBuffer ? _colorsBackBuffer : _colorsBuffer;

            int bitmapX = x < 0 ? -x : 0;
            int bitmapY = y < 0 ? -y : 0;
            int bitmapWidth = Math.Min(bitmap.Width - bitmapX, x < 0 ? Width : Width - x);
            int bitmapHeight = Math.Min(bitmap.Height - bitmapY, y < 0 ? Height : Height - y);
            int coorX = Math.Max(0, x);
            int coorY = Math.Max(0, y);

            int bmpStride = bitmap.Stride;
            int pos = 3 * (bitmapY * bitmap.Width + bitmapX);
            int stride = (bmpStride - 3 * bitmap.Width) + 3 * (bitmap.Width - bitmapWidth);

            Span<byte> span = bitmap.AsByteSpan();

            for (int j = 0; j < bitmapHeight; j++)
            {
                for (int i = 0; i < bitmapWidth; i++)
                {
                    if (red == span[pos + 2] && green == span[pos + 1] && blue == span[pos])
                    {
                        SetPixel(coorX + i, coorY + j, repRed, repGreen, repBlue, buffer);
                    }
                    else
                    {
                        SetPixel(coorX + i, coorY + j, span[pos + 2], span[pos + 1], span[pos], buffer);
                    }

                    pos += 3;
                }

                pos += stride;
            }
        }

        /// <summary>
        /// Draws text on the display at a specified position
        /// </summary>
        /// <param name="x">X coordinate of the text position</param>
        /// <param name="y">Y coordinate of the text position</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">Font to use to draw the text</param>
        /// <param name="textR">Red channel of the text color</param>
        /// <param name="textG">Green channel of the text color</param>
        /// <param name="textB">Blue channel of the text color</param>
        /// <param name="bkR">Red channel of the text background</param>
        /// <param name="bkG">Green channel of the text background</param>
        /// <param name="bkB">Blue channel of the text background</param>
        /// <param name="backBuffer">Set to true if drawing on the backing buffer. Defaults to false.</param>
        public void DrawText(int x, int y, ReadOnlySpan<char> text, BdfFont font, byte textR, byte textG, byte textB,
            byte bkR, byte bkG, byte bkB, bool backBuffer = false)
        {
            int charWidth = font.Width;
            int totalTextWith = charWidth * text.Length;

            if (y <= -font.Height || y >= Height || x >= Width || x + totalTextWith <= 0)
            {
                return;
            }

            byte[] buffer = backBuffer ? _colorsBackBuffer : _colorsBuffer;

            int index = 0;
            while (index < text.Length)
            {
                if (x + charWidth < 0)
                {
                    x += charWidth;
                    index++;
                    continue;
                }

                DrawChar(x, y, text[index], font, textR, textG, textB, bkR, bkG, bkB, buffer);

                x += charWidth;
                index++;
            }
        }

        /// <summary>
        /// Write a text at specific position to the display using the input font and the colors
        /// </summary>
        /// <param name="x">Upper left x coordinate to start drawing the text at</param>
        /// <param name="y">Upper left y coordinate to start drawing the text at</param>
        /// <param name="text">The text to draw</param>
        /// <param name="font">The drawing font</param>
        /// <param name="textR">text red color</param>
        /// <param name="textG">text green color</param>
        /// <param name="textB">text blue color</param>
        /// <param name="bkR">text background red color</param>
        /// <param name="bkG">text background green color</param>
        /// <param name="bkB">text background blue color</param>
        /// <param name="backBuffer">true if want use back buffer, false otherwise</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawText(int x, int y, string text, BdfFont font, byte textR, byte textG, byte textB, byte bkR,
            byte bkG, byte bkB, bool backBuffer = false)
        {
            DrawText(x, y, text.AsSpan(), font, textR, textG, textB, bkR, bkG, bkB, backBuffer);
        }

        /// <summary>
        /// Draw a Circle on the display
        /// </summary>
        /// <param name="xCenter">x coordinate of the center</param>
        /// <param name="yCenter">y coordinate of the center</param>
        /// <param name="radius">radius length</param>
        /// <param name="red">red color of circle arc</param>
        /// <param name="green">green color of circle arc</param>
        /// <param name="blue">blue color of circle arc</param>
        /// <param name="backBuffer">true if want use back buffer, false otherwise</param>
        public void DrawCircle(int xCenter, int yCenter, int radius, byte red, byte green, byte blue,
            bool backBuffer = false)
        {
            byte[] buffer = backBuffer ? _colorsBackBuffer : _colorsBuffer;

            for (double angle = 0.0; angle < 6.2832; angle += 1.0 / radius)
            {
                SetPixel((int)Math.Round(xCenter + radius * Math.Cos(angle)),
                    (int)Math.Round(yCenter + radius * Math.Sin(angle)), red, green, blue, buffer);
            }
        }

        private void DrawChar(int x, int y, char c, BdfFont font, byte textR, byte textG, byte textB, byte bkR,
            byte bkG, byte bkB, byte[] buffer)
        {
            int hightToDraw = Math.Min(Height - y, font.Height);
            int firstColumnToDraw = x < 0 ? Math.Abs(x) : 0;
            int lastColumnToDraw = x + font.Width > Width ? Width - x : font.Width;

            font.GetCharData(c, out ReadOnlySpan<ushort> charData);

            int b = 8 * (sizeof(ushort) - (int)Math.Ceiling(((double)font.Width) / 8)) + firstColumnToDraw;

            for (int j = firstColumnToDraw; j < lastColumnToDraw; j++)
            {
                for (int i = 0; i < hightToDraw; i++)
                {
                    int value = charData[i] << (b + j - firstColumnToDraw);

                    if ((value & 0x8000) != 0)
                    {
                        SetPixel(x + j, y + i, textR, textG, textB, buffer);
                    }
                    else
                    {
                        SetPixel(x + j, y + i, bkR, bkG, bkB, buffer);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Sleep(long duration)
        {
            long startTicks = Stopwatch.GetTimestamp();
            while (Stopwatch.GetTimestamp() - startTicks < duration)
            {
            }
        }

        private void Render()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Interop.ThreadHelper.SetCurrentThreadHighPriority();

            _safeToDispose = false;

            // Line 0
            _gpio.WriteClear(_gpio.AMask | _gpio.BMask | _gpio.CMask | _gpio.DMask);

            long startTime = 0;
            bool showFrameTime = false;
            int row = 0;

            while (_isRendering == 1)
            {
                RenderRow(row);

                if (_startMeasureFrameTime && row == (_deviceRows >> 1) - 1)
                {
                    if (showFrameTime)
                    {
                        long totalTime = Stopwatch.GetTimestamp() - startTime;
                        _frameTime = (long)(((double)totalTime / Stopwatch.Frequency) * 1E6);
                        showFrameTime = false;
                        _startMeasureFrameTime = false;
                    }
                    else
                    {
                        showFrameTime = true;
                        startTime = Stopwatch.GetTimestamp();
                    }
                }

                row = (row + 1) % (_deviceRows >> 1);

                if (row == 0 && _swapRequested)
                {
                    SwapBuffersInternal();
                    _swapRequested = false;
                }

                _gpio.WriteSet(_gpio.OEMask | _rowSetMasks[row]); // Disable the output and push the next row
                _gpio.WriteClear((~_rowSetMasks[row]) & _gpio.ABCDEMask);
            }

            _safeToDispose = true;
        }

        private void RenderRow(int row)
        {
            int pos = (row % (_deviceRows >> 1)) * _fullChainWidth * 8;
            for (int bit = 0; bit < 8; bit++)
            {
                for (int column = 0; column < _fullChainWidth; column++)
                {
                    byte colorsBits = _colorsBuffer[pos + (column << 3) + bit];

                    ulong mask = _colorsMake[colorsBits & 0x07] | _colorsMake[8 + ((colorsBits >> 3) & 0x07)];
                    _gpio.WriteSet(mask);
                    _gpio.WriteClear((~mask) & _gpio.AllColorsMask);

                    _gpio.WriteSet(_gpio.ClockMask);
                    _gpio.WriteClear(_gpio.ClockMask);
                }

                _gpio.WriteSet(_gpio.LatchMask);
                _gpio.WriteClear(_gpio.LatchMask | _gpio.OEMask);
                Sleep(_duration * (1 << bit));
                _gpio.WriteSet(_gpio.OEMask);
            }
        }

        private void OpenAndWriteToPin(int pinNumber, PinValue value)
        {
            _controller.OpenPin(pinNumber, PinMode.Output);
            _controller.Write(pinNumber, value);
        }
    }
}
