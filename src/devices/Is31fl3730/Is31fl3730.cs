// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Device.Spi;
using Iot.Device.Multiplexing.Utility;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/microdot-phat
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class Is31fl3730 : IDisposable
    {
        // Function register
        // table 2 in datasheet
        private const byte CONFIGURATION_REGISTER = 0x0;
        private const byte MATRIX_1_REGISTER = 0x1;
        private const byte MATRIX_1_REGISTER_LENGTH = MATRIX_2_REGISTER - MATRIX_1_REGISTER;
        private const byte MATRIX_2_REGISTER = 0xE;
        private const byte MATRIX_2_REGISTER_LENGTH = UPDATE_COLUMN_REGISTER - MATRIX_2_REGISTER;
        private const byte UPDATE_COLUMN_REGISTER = 0xC;
        private const byte LIGHTING_EFFECT_REGISTER = 0xD;
        private const byte PWM_REGISTER = 0x19;
        private const byte RESET_REGISTER = 0xC;
        

        // Configuration register
        // table 3 in datasheet
        private const byte SHUTDOWN = 0x80;
        private const byte DISPLAY_MATRIX_ONE = 0x0;
        private const byte DISPLAY_MATRIX_TWO = 0x8;
        private const byte DISPLAY_MATRIX_BOTH = 0x18;
        private const byte MATRIX_8x8 = 0x0;
        private const byte MATRIX_7x9 = 0x1;
        private const byte MATRIX_6x10 = 0x2;
        private const byte MATRIX_5x11 = 0x3;

       /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Is31fl3730(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            // _enable_all_leds_data.AsSpan().Fill(0xff);

        }

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        /// <param name="width">The width of the LED matrix.</param>
        /// <param name="height">The height of the LED matrix.</param>
        public Is31Fl3731(I2cDevice i2cDevice, int width, int height)
        : this(i2cDevice)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static readonly int DefaultI2cAddress;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 16;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 9;
        
        private int _configurationValue = 0;

        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        public void Initialize()
        {
            // Reset device
            Reset();

            // set display mode (bits d4:d3)
            // 00 = picture mode
            // 01 = auto frame
            // 1x audio frame play mode
            Write(FUNCTION_REGISTER, CONFIGURATION_REGISTER, 0);
            SetDisplayMode(true, false);
            SetMatrixMode(MatrixMode.Size8x8);
        }

        /// <summary>
        /// Indexer for updating matrix, with PWM register.
        /// </summary>
        public int this[int x, int y]
        {
            get => ReadLedPwm(x, y);
            set => WriteLedPwm(x, y, value);
        }

        /// <summary>
        /// Reset device.
        /// </summary>
        public void Reset()
        {
            // Reset device
            Shutdown(true);
            Thread.Sleep(10);
            Shutdown(false);
        }

        /// <summary>
        /// Set the shutdown mode.
        /// </summary>
        /// <param name="shutdown">Set the showdown mode. `true` sets device into shutdown mode. `false` sets device into normal operation.</param>
        public void Shutdown(bool shutdown)
        {
            // mode values
            // 0 = normal operation
            // 1 = shutdown mode
            if (shutdown)
            {
                _configurationValue |= SHUTDOWN;
            }
            else
            {
                _configurationValue &= ~SHUTDOWN;
            }

            Write(CONFIGURATION_REGISTER, _configurationValue);
        }

        private void SetDisplayMode(bool matrixOne, bool matrixTwo)
        {
            if (matrixOne && matrixTwo)
            {
                _configurationValue |= DISPLAY_MATRIX_BOTH;
            }
            else if (matrixOne)
            {
                _configurationValue |= DISPLAY_MATRIX_ONE;
            }
            else if (matrixTwo)
            {
                _configurationValue |= DISPLAY_MATRIX_TWO;
            }
        }

        private void SetMatrixMode(MatrixMode mode)
        {
            if (mode is MatrixMode.Size5x11)
            {
                _configurationValue |= MATRIX_5x11;
            }
            else if (mode is MatrixMode.Size6x10)
            {
                _configurationValue |= MATRIX_6x10;
            }
            else if (mode is MatrixMode.Size7x9)
            {
                _configurationValue |= MATRIX_7x9;
            }
            else if (mode is MatrixMode.Size8x8)
            {
                _configurationValue |= MATRIX_8x8;
            }
        }

        private void Write(byte address, byte[] value)
        {
            byte[] data = new byte[value.Length + 1];
            data[0] = address;
            value.CopyTo(data.AsSpan(1));
            _i2cDevice.Write(data);
        }
    }
}
