// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Runtime.CompilerServices;

namespace Iot.Device.Blinkt
{
    /// <summary>
    /// Pimoroni Blinkt! - 8-pixel APA102 LED display
    /// </summary>
    public class Blinkt : IBlinkt
    {
        private const int Data = 23;
        private const int Clock = 24;

        // number of bytes that represent an LED
        private const int PixelWidth = 4;

        /// <summary>
        /// Indicates the number of LEDs on the Blinkt!
        /// </summary>
        public const int NumPixels = 8;

        /// <summary>
        /// Buffer order is Brightness, Green, Blue, Red in LED order.
        /// </summary>
        private readonly byte[] _buffer = new byte[NumPixels * PixelWidth];

        private readonly bool _shouldDispose;

        private GpioController _gpioController;

        /// <summary>
        /// Initializes a new instance of the <see cref="Blinkt"/> class.
        /// </summary>
        public Blinkt(GpioController gpioController = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose;
            _gpioController = gpioController ?? new GpioController(PinNumberingScheme.Logical);
            _gpioController.OpenPin(Data, PinMode.Output);
            _gpioController.OpenPin(Clock, PinMode.Output);

            SetBrightness(0.1);
        }

        /// <summary>
        /// If true, set all LEDs to black and zero brightness on dispose.
        /// </summary>
        public bool ClearOnExit { get; set; } = true;

        /// <summary>
        /// Set the brightness for all the LEDs
        /// </summary>
        /// <param name="brightness">The percentage of brightness desired.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBrightness(double brightness)
        {
            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness should be between 0.0 and 1.0");
            }

            for (var x = 0; x < NumPixels * PixelWidth; x += PixelWidth)
            {
                _buffer[x] = ConvertBrightness(brightness);
            }
        }

        /// <summary>
        /// Set all LEDs to black.
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < _buffer.Length; x++)
            {
                // Respect the brightness
                if (x % PixelWidth != 0)
                {
                    _buffer[x] = 0;
                }
            }
        }

        /// <summary>
        /// Displays the buffer on the Blinkt.
        /// </summary>
        public void Show()
        {
            StartFrame();

            for (var x = 0; x < NumPixels * PixelWidth; x++)
            {
                WriteByte((byte)(0b11100000 | _buffer[x])); // Brightness
                WriteByte(_buffer[++x]); // Blue
                WriteByte(_buffer[++x]); // Green
                WriteByte(_buffer[++x]); // Red
            }

            EndFrame();
        }

        /// <summary>
        /// Sets all LEDs to the desired color and brightness.
        /// </summary>
        /// <param name="red">Red color.</param>
        /// <param name="green">Green color.</param>
        /// <param name="blue">Blue color.</param>
        /// <param name="brightness">Brightness percentage.</param>
        public void SetAll(byte red, byte green, byte blue, double? brightness = null)
        {
            for (var x = 0; x < NumPixels; x++)
            {
                SetPixel(x, red, green, blue, brightness);
            }
        }

        /// <summary>
        /// Set a specific LED to the desired color and brightness.
        /// </summary>
        /// <param name="pixel">LED to set.</param>
        /// <param name="red">Red color.</param>
        /// <param name="green">Green color.</param>
        /// <param name="blue">Blue color.</param>
        /// <param name="brightness">Brightness percentage.</param>
        public void SetPixel(int pixel, byte red, byte green, byte blue, double? brightness = null)
        {
            if (pixel < 0 || pixel >= NumPixels)
            {
                throw new ArgumentOutOfRangeException(nameof(pixel), "Pixel must be between 0 and 7");
            }

            pixel *= PixelWidth;

            if (brightness != null)
            {
                _buffer[pixel] = ConvertBrightness(brightness.Value);
            }

            _buffer[++pixel] = blue;
            _buffer[++pixel] = green;
            _buffer[++pixel] = red;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ConvertBrightness(double brightness) => (byte)((int)(31.0 * brightness) & 0b11111);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartFrame() => SetFrame(32);

        // Emit exactly enough clock pulses to latch the small dark die APA102s which are weird
        // for some reason it takes 36 clocks, the other IC takes just 4 (number of pixels/2)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndFrame() => SetFrame(36);

        private void SetFrame(int clockCycles)
        {
            _gpioController.Write(Data, 0);
            for (var x = 0; x < clockCycles; x++)
            {
                _gpioController.Write(Clock, 1);
                _gpioController.Write(Clock, 0);
            }
        }

        private void WriteByte(byte data)
        {
            for (var x = 0; x < 8; x++)
            {
                _gpioController.Write(Data, data & 0b10000000);
                _gpioController.Write(Clock, 1);
                data <<= 1;
                _gpioController.Write(Clock, 0);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_gpioController == null)
            {
                return;
            }

            if (ClearOnExit)
            {
                Clear();
                Show();
            }

            _gpioController?.ClosePin(Data);
            _gpioController?.ClosePin(Clock);

            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}
