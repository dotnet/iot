// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Device.Model;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// Base class for SenseHAT LED matrix
    /// </summary>
    [Interface("SenseHat LED Matrix")]
    public abstract class SenseHatLedMatrix : IDisposable
    {
        /// <summary>
        /// Total number of pixels
        /// </summary>
        public const int NumberOfPixels = 64;

        /// <summary>
        /// Number of pixels per row
        /// </summary>
        public const int NumberOfPixelsPerRow = 8;

        // does not need to be public since it should not be used

        /// <summary>
        /// Number of pixels per column
        /// </summary>
        protected const int NumberOfPixelsPerColumn = 8;

        /// <summary>
        /// Lazily intialized pixel font reference. Initialized when rendering text.
        /// </summary>
        protected SenseHatTextFont? _pixelFont = null;

        /// <summary>
        /// Lazily initialized text render state. Initialized when rendering text.
        /// </summary>
        protected SenseHatTextRenderState? _textRenderState = null;

        /// <summary>
        /// Text color when rendering text.
        /// </summary>
        protected Color _textColor = Color.DarkBlue;

        /// <summary>
        /// Text background color when rendering text.
        /// </summary>
        protected Color _textBackgroundColor = Color.Black;

        /// <summary>
        /// Text rotation, counterclockwise.
        /// </summary>
        protected SenseHatTextRotation _textRotation = SenseHatTextRotation.Rotate_0_Degrees;

        /// <summary>
        /// Text scroll speed when the rendered text does not fit the 8x8 LED matrix
        /// </summary>
        protected double _textScrollPixelsPerSecond = 1;

        /// <summary>
        /// Timer used for text animation (scrolling). Lazily initialized as required.
        /// </summary>
        protected System.Timers.Timer? _textAnimationTimer = null;

        // Lock object to maintain a consistent set of render parameters.
        private object _lockTextRenderState = new();

        // Lock object to prevent i2c disposal during render.
        // "Terminate" must be called for clean termination of text animation.
        private object _lockWrite = new();

        // Flag set to true when no more rendering should take place because the LED matrix is about to be disposed.
        private volatile bool _terminate = false;

        /// <summary>
        /// Constructs SenseHatLedMatrix instance
        /// </summary>
        protected SenseHatLedMatrix()
        {
        }

        /// <summary>
        /// Translates position in the buffer to X, Y coordinates
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Tuple of X and Y coordinates</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int X, int Y) IndexToPosition(int index)
        {
            if (index < 0 || index >= NumberOfPixelsPerRow * NumberOfPixelsPerRow)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (index % NumberOfPixelsPerRow, index / NumberOfPixelsPerRow);
        }

        /// <summary>
        /// Translate X and Y coordinates to position in the buffer
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Position in the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int x, int y)
        {
            if (x < 0 || x >= NumberOfPixelsPerRow)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0 || y >= NumberOfPixelsPerColumn)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            return x + y * NumberOfPixelsPerRow;
        }

        /// <summary>
        /// Write colors to the device
        /// </summary>
        /// <param name="colors">Array of colors</param>
        [Command]
        public abstract void Write(ReadOnlySpan<Color> colors);

        /// <summary>
        /// Fill LED matrix with a specific color
        /// </summary>
        /// <param name="color">Color to fill the device with</param>
        [Command]
        public abstract void Fill(Color color = default(Color));

        /// <summary>
        /// Sets color on specific position of the LED matrix
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="color">Color to be set in the specified position</param>
        [Command]
        public abstract void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Stop animation effects if active.
        /// </summary>
        public void Terminate()
        {
            lock (_lockTextRenderState)
            {
                _textRenderState = null;
                StopTextAnimationTimer();
            }

            lock (_lockWrite)
            {
                // Prevent further access to the underlying device (i.e. i2c bus)
                _terminate = true;
            }
        }

        /// <inheritdoc/>
        public abstract void Dispose();

        /// <summary>
        /// Renders text on the LED display.
        /// </summary>
        /// <param name="text">Text to render. Set to empty string to stop rendering text.</param>
        [Command]
        public void SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                StopTextAnimationTimer();
                Fill(_textBackgroundColor);
                lock (_lockTextRenderState)
                {
                    _textRenderState = null;
                }

                return;
            }

            if (_pixelFont == null)
            {
                _pixelFont = new SenseHatTextFont();
            }

            var renderMatrix = _pixelFont.RenderText(text);
            SenseHatTextRenderState renderState;
            lock (_lockTextRenderState)
            {
                // Create a new render state containing the render matrix for the new text
                _textRenderState = new SenseHatTextRenderState(renderMatrix, _textColor, _textBackgroundColor, _textRotation);
                renderState = _textRenderState;
            }

            RenderText(renderState);

            StartOrStopTextScrolling();
        }

        /// <summary>
        /// Text color when rendering text.
        /// </summary>
        [Property]
        public Color TextColor
        {
            get
            {
                return _textColor;
            }
            set
            {
                _textColor = value;

                // Apply new state. Lock because render state is nullable and assignment may not be atomic.
                SenseHatTextRenderState? renderState;
                lock (_lockTextRenderState)
                {
                    if (_textRenderState != null)
                    {
                        _textRenderState = _textRenderState.ApplyTextColor(value);
                    }

                    renderState = _textRenderState;
                }

                RenderText(renderState);
            }
        }

        /// <summary>
        /// Text background color when rendering text.
        /// </summary>
        [Property]
        public Color TextBackgroundColor
        {
            get
            {
                return _textBackgroundColor;
            }
            set
            {
                _textBackgroundColor = value;

                // Apply new state. Lock because render state is nullable and assignment may not be atomic.
                SenseHatTextRenderState? renderState;
                lock (_lockTextRenderState)
                {
                    if (_textRenderState != null)
                    {
                        _textRenderState = _textRenderState.ApplyTextBackgroundColor(value);
                    }

                    renderState = _textRenderState;
                }

                RenderText(renderState);
            }
        }

        /// <summary>
        /// Text scroll speed in pixels per second.
        /// </summary>
        [Property]
        public double TextScrollPixelsPerSecond
        {
            get
            {
                return _textScrollPixelsPerSecond;
            }
            set
            {
                _textScrollPixelsPerSecond = value;
                StartOrStopTextScrolling();
            }
        }

        /// <summary>
        /// Text rotation, counterclockwise.
        /// </summary>
        [Property]
        public SenseHatTextRotation TextRotation
        {
            get
            {
                return _textRotation;
            }
            set
            {
                _textRotation = value;

                // Apply new state. Lock because render state is nullable and assignment may not be atomic.
                SenseHatTextRenderState? renderState;
                lock (_lockTextRenderState)
                {
                    if (_textRenderState != null)
                    {
                        _textRenderState = _textRenderState.ApplyTextRotation(value);
                    }

                    renderState = _textRenderState;
                }

                RenderText(renderState);
            }
        }

        private void StartOrStopTextScrolling()
        {
            SenseHatTextRenderState? renderState;
            // Lock because render state is nullable and assignment may not be atomic
            lock (_lockTextRenderState)
            {
                renderState = _textRenderState;
            }

            int millisecondsPerPixel;
            if (renderState == null || renderState.TextRenderMatrix.Text.Length == 0 || _textScrollPixelsPerSecond <= 0)
            {
                millisecondsPerPixel = 0;
            }
            else
            {
                millisecondsPerPixel = (int)(1000 / _textScrollPixelsPerSecond);
            }

            if (millisecondsPerPixel <= 0)
            {
                StopTextAnimationTimer();
            }
            else
            {
                // Calculate the number of milliseconds for one pixel shift
                StartTextAnimationTimer((int)millisecondsPerPixel);
            }
        }

        private void StartTextAnimationTimer(int intervalMs)
        {
            StopTextAnimationTimer();
            _textAnimationTimer = new System.Timers.Timer(intervalMs);
            _textAnimationTimer.Elapsed += TextAnimationTimer_Elapsed;
            _textAnimationTimer.Start();
        }

        private void StopTextAnimationTimer()
        {
            if (_textAnimationTimer != null)
            {
                _textAnimationTimer.Stop();
                _textAnimationTimer.Elapsed -= TextAnimationTimer_Elapsed;
                _textAnimationTimer.Dispose();
                _textAnimationTimer = null;
            }
        }

        private void TextAnimationTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            SenseHatTextRenderState? renderState;
            // Lock because render state is nullable and assignment may not be atomic
            lock (_lockTextRenderState)
            {
                renderState = _textRenderState;
            }

            if (renderState != null)
            {
                var renderMatrix = renderState.TextRenderMatrix;
                if (renderMatrix.Text.Length > 1)
                {
                    renderMatrix.ScrollByOnePixel();
                    RenderText(renderState);
                }
            }
        }

        private void RenderText(SenseHatTextRenderState? textRenderState)
        {
            if (textRenderState == null)
            {
                return;
            }

            // Allocate frame buffer to hold color values for one frame
            var frameBuffer = new Color[NumberOfPixelsPerColumn * NumberOfPixelsPerRow];

            // Render the 8x8 matrix
            for (var x = 0; x < NumberOfPixelsPerColumn; x++)
            {
                for (var y = 0; y < NumberOfPixelsPerRow; y++)
                {
                    int tx = x;
                    int ty = y;
                    switch (textRenderState.TextRotation)
                    {
                        case SenseHatTextRotation.Rotate_0_Degrees:
                            break;
                        case SenseHatTextRotation.Rotate_90_Degrees:
                            tx = y;
                            ty = NumberOfPixelsPerColumn - x - 1;
                            break;
                        case SenseHatTextRotation.Rotate_180_Degrees:
                            tx = NumberOfPixelsPerColumn - x - 1;
                            ty = NumberOfPixelsPerRow - y - 1;
                            break;
                        case SenseHatTextRotation.Rotate_270_Degrees:
                            tx = NumberOfPixelsPerColumn - y - 1;
                            ty = x;
                            break;
                    }

                    var frameIndex = PositionToIndex(tx, ty);

                    if (textRenderState.TextRenderMatrix.IsPixelSet(x, y))
                    {
                        frameBuffer[frameIndex] = textRenderState.TextColor;
                    }
                    else
                    {
                        frameBuffer[frameIndex] = textRenderState.TextBackgroundColor;
                    }
                }
            }

            // Lock write so that SenseHat disposal does not interfere
            lock (_lockWrite)
            {
                if (!_terminate)
                {
                    Write(frameBuffer);
                }
            }
        }
    }
}
