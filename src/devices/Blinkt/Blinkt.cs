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
        private const int DAT = 23;
        private const int CLK = 24;
        private const int BRIGHTNESS = 7;

        /// <summary>
        /// The number of pixels in the Blinkt strip
        /// </summary>
        public const int NUMBER_OF_PIXELS = 8;

        private readonly Color[] _pixels = new Color[NUMBER_OF_PIXELS];
        private readonly int _sleepTime = 0;
        private readonly GpioController _gpio;

        private bool _gpioSetup = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Blinkt"/> class.
        /// </summary>
        public Blinkt()
        {
            for (int i = 0; i < NUMBER_OF_PIXELS; i++)
            {
                _pixels[i] = Color.Empty;
            }

            _gpio = new GpioController();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Clear();
            Show();

            _gpio.Dispose();
        }

        /// <summary>
        /// Sets the brightness of all the pixels
        /// </summary>
        /// <param name="brightness">The brightess for the pixels</param>
        public void SetBrightness(byte brightness)
        {
            for (var i = 0; i < NUMBER_OF_PIXELS; i++)
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
            if (!_gpioSetup)
            {
                _gpio.OpenPin(DAT, PinMode.Output);
                _gpio.OpenPin(CLK, PinMode.Output);
                _gpioSetup = true;
            }

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
            for (var i = 0; i < NUMBER_OF_PIXELS; i++)
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
            if (pixel < 0 || pixel >= NUMBER_OF_PIXELS)
            {
                throw new ArgumentOutOfRangeException(nameof(pixel), $"Pixel must be between 0 and {NUMBER_OF_PIXELS - 1}");
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
            if (pixel < 0 || pixel >= NUMBER_OF_PIXELS)
            {
                throw new ArgumentOutOfRangeException(nameof(pixel), $"Pixel must be between 0 and {NUMBER_OF_PIXELS - 1}");
            }

            _pixels[pixel] = color;
        }

        private void WriteByte(int b)
        {
            for (var i = 0; i < 8; i++)
            {
                _gpio.Write(DAT, (b & 0b10000000) != 0);
                _gpio.Write(CLK, PinValue.High);
                Thread.Sleep(_sleepTime);
                b <<= 1;
                _gpio.Write(CLK, PinValue.Low);
                Thread.Sleep(_sleepTime);
            }
        }

        private void Eof()
        {
            _gpio.Write(DAT, PinValue.Low);
            for (var i = 0; i < 36; i++)
            {
                _gpio.Write(CLK, PinValue.High);
                Thread.Sleep(_sleepTime);
                _gpio.Write(CLK, PinValue.Low);
                Thread.Sleep(_sleepTime);
            }
        }

        private void Sof()
        {
            _gpio.Write(DAT, PinValue.Low);
            for (var i = 0; i < 32; i++)
            {
                _gpio.Write(CLK, PinValue.High);
                Thread.Sleep(_sleepTime);
                _gpio.Write(CLK, PinValue.Low);
                Thread.Sleep(_sleepTime);
            }
        }
    }
}
