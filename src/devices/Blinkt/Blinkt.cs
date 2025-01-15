// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Drawing;

namespace Iot.Device.Blinkt
{
    /// <summary>
    /// Driver for the Pimoroni Blinkt LED strip for Raspbery Pi
    /// https://shop.pimoroni.com/products/blinkt
    ///
    /// This is a strip of 8 pixels that can be indendently controlled over GPIO.
    ///
    /// Setting the color is a 2-step process:
    /// Set the color using Clear, SetPixel, or SetAll
    /// Call Show to update the physical pixels
    /// </summary>
    public class Blinkt : IDisposable
    {
        private readonly GpioPin _datPin;
        private readonly GpioPin _clkPin;

        private readonly Color[] _pixels = new Color[NumberOfPixels];
        private readonly bool _shouldDispose;
        private GpioController _gpioController;

        /// <summary>
        /// The number of pixels in the Blinkt strip
        /// </summary>
        public const int NumberOfPixels = 8;

        /// <summary>
        /// The sleep time in milliseconds between each bit written to the Blinkt.
        ///
        /// The default value of 0 should be enough for most cases and works on the RPi 4 and 5,
        /// but you can increase this if you experience issues with the Blinkt on faster CPUs.
        /// </summary>
        public int SleepTime { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Blinkt"/> class.
        /// </summary>
        /// <param name="datPin">The GPIO pin number for the data pin. This defaults to 23,
        /// and you should only change this if you connect the blinkt using cables instead of sitting it on the GPIO pins using the provided socket.</param>
        /// <param name="clkPin">The GPIO pin number for the clock pin. This defaults to 24,
        /// and you should only change this if you connect the blinkt using cables instead of sitting it on the GPIO pins using the provided socket.</param>
        /// <param name="gpioController">The GPIO controller to use with the Blinkt. If not provided, a new controller is created.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public Blinkt(int datPin = 23, int clkPin = 24, GpioController? gpioController = null, bool shouldDispose = true)
        {
            for (int i = 0; i < NumberOfPixels; i++)
            {
                _pixels[i] = Color.Empty;
            }

            _shouldDispose = shouldDispose || gpioController is null;
            _gpioController = gpioController ?? new();

            _datPin = _gpioController.OpenPin(datPin, PinMode.Output);
            _clkPin = _gpioController.OpenPin(clkPin, PinMode.Output);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Clear();
            Show();

            // this condition only applies to GPIO devices
            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null!;
            }
        }

        /// <summary>
        /// Sets the brightness of all the pixels
        /// </summary>
        /// <param name="brightness">The brightess for the pixels</param>
        public void SetBrightness(byte brightness)
        {
            for (var i = 0; i < NumberOfPixels; i++)
            {
                _pixels[i] = Color.FromArgb(brightness, _pixels[i]);
            }
        }

        /// <summary>
        /// Clears all the LEDs by turning them off.
        ///
        /// After calling Clear, you must call Show to update the LEDs.
        /// </summary>
        public void Clear()
        {
            SetAll(Color.Empty);
        }

        /// <summary>
        /// Shows the current state of the LEDs by applying the colors and brightness to the LEDs.
        /// </summary>
        public void Show()
        {
            Sof();

            foreach (Color pixel in _pixels)
            {
                int r = pixel.R;
                int g = pixel.G;
                int b = pixel.B;
                int brightness = (int)(31.0 * (pixel.A / 255.0)) & 0b11111;
                WriteByte(0b11100000 | brightness);
                WriteByte(b);
                WriteByte(g);
                WriteByte(r);
            }

            Eof();
        }

        /// <summary>
        /// Sets the color of all the pixels.
        /// This does not update the physical pixel, you must call Show to display the color.
        /// </summary>
        /// <param name="color">The color to set the pixel to</param>
        public void SetAll(Color color)
        {
            for (var i = 0; i < NumberOfPixels; i++)
            {
                SetPixel(i, color);
            }
        }

        /// <summary>
        /// Gets the color that the specified pixel is set to.
        /// This may not reflect the actual color of the LED if Show has not been called.
        /// </summary>
        /// <param name="pixel">The index of the pixel to get</param>
        /// <returns>The color of the pixel </returns>
        public Color GetPixel(int pixel)
        {
            if (pixel < 0 || pixel >= NumberOfPixels)
            {
                throw new ArgumentOutOfRangeException(nameof(pixel), $"Pixel must be between 0 and {NumberOfPixels - 1}");
            }

            return _pixels[pixel];
        }

        /// <summary>
        /// Sets the color of the specified pixel to the specified color.
        /// This does not update the physical pixel, you must call Show to display the color.
        /// </summary>
        /// <param name="pixel">The index of the pixel to update</param>
        /// <param name="color">The color to set on the pixel</param>
        /// <exception cref="ArgumentOutOfRangeException">The value of pixel must be between 0 and 7, otherwise this exception is thrown</exception>
        public void SetPixel(int pixel, Color color)
        {
            if (pixel < 0 || pixel >= NumberOfPixels)
            {
                throw new ArgumentOutOfRangeException(nameof(pixel), $"Pixel must be between 0 and {NumberOfPixels - 1}");
            }

            _pixels[pixel] = color;
        }

        private void WriteByte(int b)
        {
            for (var i = 0; i < 8; i++)
            {
                _datPin.Write((b & 0b10000000) != 0);
                _clkPin.Write(PinValue.High);
                Thread.Sleep(SleepTime);
                b <<= 1;
                _clkPin.Write(PinValue.Low);
                Thread.Sleep(SleepTime);
            }
        }

        private void Eof()
        {
            _datPin.Write(PinValue.Low);
            for (var i = 0; i < 36; i++)
            {
                _clkPin.Write(PinValue.High);
                Thread.Sleep(SleepTime);
                _clkPin.Write(PinValue.Low);
                Thread.Sleep(SleepTime);
            }
        }

        private void Sof()
        {
            _datPin.Write(PinValue.Low);
            for (var i = 0; i < 32; i++)
            {
                _clkPin.Write(PinValue.High);
                Thread.Sleep(SleepTime);
                _clkPin.Write(PinValue.Low);
                Thread.Sleep(SleepTime);
            }
        }
    }
}
