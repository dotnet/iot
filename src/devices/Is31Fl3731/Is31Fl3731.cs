// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-learn.adafruit.com/assets/assets/000/030/994/original/31FL3731.pdf
    // Product: https://www.adafruit.com/product/2946
    // Product: https://www.adafruit.com/product/2974
    public abstract class Is31Fl3731 : IDisposable
    {
        // Register addresses
        // Command register
        // Points pages one to nine
        // table 2 in datasheet
        private const byte COMMAND_REGISTER = 0xFD;
        private const byte FUNCTION_REGISTER = 0x0B;

        // Response register
        // Writes data to pages one to eight
        // table 3 in datasheet
        private const byte LED_REGISTER = 0x0;
        private const int LED_REGISTER_LENGTH = BLINK_REGISTER - LED_REGISTER;
        private const byte BLINK_REGISTER = 0x12;
        private const byte BLINK_REGISTER_LENGTH = PWM_REGISTER - BLINK_REGISTER;
        private const byte PWM_REGISTER = 0x24;
        private const byte PWM_REGISTER_END = 0xB3;
        private const byte PWM_REGISTER_LENGTH = (PWM_REGISTER_END - PWM_REGISTER) + 1;

        // Function register
        // Writes data to page nine
        // table 3 in datasheet
        private const byte CONFIGURATION_REGISTER = 0x0;
        private const byte DISPLAY_REGISTER = 0x5;
        private const byte SHUTDOWN = 0x0A;

        // Values
        private readonly byte[] _disable_all_leds_data = new byte[LED_REGISTER_LENGTH];
        private readonly byte[] _enable_all_leds_data = new byte[LED_REGISTER_LENGTH];

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initialize IS31FL3731 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Is31Fl3731(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _enable_all_leds_data.AsSpan().Fill(0xff);
        }

        /// <summary>
        /// Initialize IS31FL3731 device
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
        /// IS31FL3731 default I2C address
        /// </summary>
        public static readonly int DefaultI2cAddress = 0x74;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 16;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 9;

        /// <summary>
        /// Indexer for updating matrix, with PWM register.
        /// </summary>
        public byte this[int x, int y]
        {
            get => ReadLedPwm(x, y);
            set => WriteLedPwm(x, y, value);
        }

        /// <summary>
        /// Set value for LED.
        /// </summary>
        public void WritePixel(int x, int y, byte brightness, bool enable, bool blink)
        {
            WriteLedBlink(x, y, blink);
            WriteLed(x, y, enable);
            WriteLedPwm(x, y, brightness);
        }

        /// <summary>
        /// Fill all LEDs.
        /// </summary>
        public void Fill(byte brightness = 128, byte page = 0)
        {
            EnableAllLeds(page);
            byte[] data = new byte[PWM_REGISTER_LENGTH];
            data.AsSpan().Fill(brightness);
            Write(page, PWM_REGISTER, data);
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void EnableAllLeds(byte page = 0)
        {
            Write(page, LED_REGISTER, _enable_all_leds_data);
        }

        /// <summary>
        /// Disable all LEDs.
        /// </summary>
        public void DisableAllLeds(byte page = 0)
        {
            Write(page, LED_REGISTER, _disable_all_leds_data);
        }

        /// <summary>
        /// Disable all LEDs.
        /// </summary>
        /// <param name="rate">Set the showdown mode. `true` sets device into shutdown mode. `false` sets device into normal operation.</param>
        public void EnableBlinking(int rate)
        {
            int value = 0;
            if (rate > 0)
            {
                rate /= 270;
                value = rate & 0x07 | 0x08;
            }
            else
            {
                value = 0;
            }

            Write(FUNCTION_REGISTER, DISPLAY_REGISTER, (byte)value);
        }

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
            // set data page
            _i2cDevice.Write(new byte[] { COMMAND_REGISTER, 0 });
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
        public void Shutdown(bool shutdown = true)
        {
            // mode values
            // 0 = shutdown mode
            // 1 = normal operation
            int mode = shutdown ? 0 : 1;
            Write(FUNCTION_REGISTER, SHUTDOWN, (byte)mode);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        /// <summary>
        /// Gets the hardware location for the pixel.
        /// </summary>
        /// <param name="x">Specifies the x value.</param>
        /// <param name="y">Specifies the y value.</param>
        public abstract int GetLedAddress(int x, int y);

        private void Write(byte register, byte address, byte[] value)
        {
            _i2cDevice.Write(new byte[] { COMMAND_REGISTER, register });
            byte[] data = new byte[value.Length + 1];
            data[0] = address;
            value.CopyTo(data.AsSpan(1));
            _i2cDevice.Write(data);
        }

        private byte Read(int address)
        {
            byte[] buffer = new byte[1];
            _i2cDevice.Write(new byte[] { (byte)address });
            return _i2cDevice.ReadByte();
        }

        private void Write(byte register, byte address, byte value)
        {
            _i2cDevice.Write(new byte[] { COMMAND_REGISTER, register });
            _i2cDevice.Write(new byte[] { address, value });
        }

        private byte ReadLedPwm(int x, int y) => Read(GetLedAddress(x, y) + PWM_REGISTER);

        private void WriteLedPwm(int x, int y, byte brightness)
        {
            int address = PWM_REGISTER + GetLedAddress(x, y);
            Write(0, (byte)address, brightness);
        }

        private void WriteLed(int x, int y, bool enable)
        {
            int longAddress = GetLedAddress(x, y);
            int address = longAddress / 8;
            int ledBlock = Read(address);
            int ledRegisterBit = longAddress % 8;
            int mask = 1 >> ledRegisterBit;

            if (enable)
            {
                ledBlock |= mask;
            }
            else
            {
                ledBlock &= ~mask;
            }

            Write(0, (byte)address, (byte)ledBlock);
        }

        private void WriteLedBlink(int x, int y, bool enable)
        {
            int longAddress = GetLedAddress(x, y);
            int address = BLINK_REGISTER + longAddress / 8;
            int ledBlock = Read(address);
            int ledRegisterBit = longAddress % 8;
            int mask = 1 >> ledRegisterBit;

            if (enable)
            {
                ledBlock |= mask;
            }
            else
            {
                ledBlock &= ~mask;
            }

            Write(0, (byte)address, (byte)ledBlock);
        }
    }
}
