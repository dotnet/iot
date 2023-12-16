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
    // Port of: https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/adafruit_is31fl3731/__init__.py
    public abstract class Is31fl3731
    {
        // Register dimensions
        private const int PWM_REGISTER_END = 0xB3;
        private static readonly int _ledRegisterLength = FrameRegister.Blink - FrameRegister.Led;
        private readonly byte[] _disable_all_leds_data = new byte[_ledRegisterLength];
        private readonly byte[] _enable_all_leds_data = new byte[_ledRegisterLength];
        private readonly int _pwmRegisterLength = (PWM_REGISTER_END - FrameRegister.Pwm) + 1;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initialize IS31FL3731 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Is31fl3731(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _enable_all_leds_data.AsSpan().Fill(0xff);
        }

        /// <summary>
        /// Initialize IS31FL3731 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        /// <param name="width">The width of the LED matrix.</param>
        /// <param name="height">The height of the LED matrix.</param>
        public Is31fl3731(I2cDevice i2cDevice, int width, int height)
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
            Write(CommandRegister.Function, FunctionRegister.Configuration, 0);
            // set data page
            _i2cDevice.Write(stackalloc byte[] { CommandRegister.Command, 0 });
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
        /// Set value for LED.
        /// </summary>
        public void WritePixel(int x, int y, int brightness, bool enable, bool blink)
        {
            WriteLedBlink(x, y, blink);
            WriteLed(x, y, enable);
            WriteLedPwm(x, y, brightness);
        }

        /// <summary>
        /// Set value for all LEDs.
        /// </summary>
        public void WriteAllPixels(int[,] brightness)
        {
            Span<byte> data = stackalloc byte[_pwmRegisterLength];

            int maxX = brightness.GetLength(0);
            int maxY = brightness.GetLength(1);

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    int address = GetLedAddress(x, y);
                    data[address] = (byte)brightness[x, y];
                }
            }

            WriteLedPwm(data);
        }

        /// <summary>
        /// Fill all LEDs.
        /// </summary>
        public void Fill(byte brightness = 128, byte page = 0)
        {
            EnableAllLeds(page);
            Span<byte> data = stackalloc byte[_pwmRegisterLength];
            data.Fill(brightness);
            Write(page, FrameRegister.Pwm, data);
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void EnableAllLeds(byte page = 0)
        {
            Write(page, FrameRegister.Led, _enable_all_leds_data);
        }

        /// <summary>
        /// Disable all LEDs.
        /// </summary>
        public void DisableAllLeds(byte page = 0)
        {
            Write(page, FrameRegister.Led, _disable_all_leds_data);
        }

        /// <summary>
        /// Enable blinking at given rate.
        /// </summary>
        /// <param name="rate">Set the blinking rate.</param>
        public void SetBlinkingRate(int rate)
        {
            // See datasheet for Blink Period Time formula
            // That's where the following values are coming from
            // Copied from:
            // https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/53c796f393145452c504b39e8bcf083d3d4f5bf8/adafruit_is31fl3731/__init__.py#L269-L270
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

            Write(CommandRegister.Function, FunctionRegister.DisplayOption, (byte)value);
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
            Write(CommandRegister.Function, FunctionRegister.Shutdown, (byte)mode);
        }

        /// <summary>
        /// Gets the hardware location for the pixel.
        /// </summary>
        /// <param name="x">Specifies the x value.</param>
        /// <param name="y">Specifies the y value.</param>
        public abstract int GetLedAddress(int x, int y);

        private void Write(byte register, byte address, byte value)
        {
            _i2cDevice.Write(stackalloc byte[] { CommandRegister.Command, register });
            _i2cDevice.Write(stackalloc byte[] { address, value });
        }

        private void Write(byte register, byte address, ReadOnlySpan<byte> value)
        {
            _i2cDevice.Write(stackalloc byte[] { CommandRegister.Command, register });
            Span<byte> data = stackalloc byte[value.Length + 1];
            data[0] = address;
            value.CopyTo(data.Slice(1));
            _i2cDevice.Write(data);
        }

        private byte Read(int address)
        {
            Span<byte> buffer = stackalloc byte[1];
            _i2cDevice.Write(stackalloc byte[] { (byte)address });
            return _i2cDevice.ReadByte();
        }

        private byte ReadLedPwm(int x, int y) => Read(GetLedAddress(x, y) + FrameRegister.Pwm);

        private void WriteLedPwm(int x, int y, int brightness)
        {
            int address = FrameRegister.Pwm + GetLedAddress(x, y);
            Write(0, (byte)address, (byte)brightness);
        }

        private void WriteLedPwm(ReadOnlySpan<byte> value)
        {
            int address = FrameRegister.Pwm + GetLedAddress(0, 0);
            Write(0, (byte)address, value);
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
            int pixelAddress = GetLedAddress(x, y);
            int blinkOffset = pixelAddress / 8;
            int ledBit = pixelAddress % 8;
            int blinkAddress = FrameRegister.Blink + blinkOffset;
            int ledBlock = Read(blinkAddress);
            int mask = 1 << ledBit;

            if (enable)
            {
                ledBlock |= mask;
            }
            else
            {
                ledBlock &= ~mask;
            }

            Write(0, (byte)blinkAddress, (byte)ledBlock);
        }
    }
}
