// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.I2c;
using System.Drawing;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Supports Grove - LCD RGB Backlight (16x2 LCD character display with RGB backlight).
    /// </summary>
    /// <remarks>
    /// This implementation was drawn from numerous libraries such as Grove_LCD_RGB_Backlight.
    /// </remarks>
    public class LcdRgb1602 : Hd44780Base, IDisposable
    {
        private I2cDevice _lcdDevice;
        private I2cDevice _rgbDevice;

        private Color _currentColor;
        private bool _backlightOn = true;

        /// <summary>
        /// Initializes a new HD44780 LCD controller.
        /// </summary>
        /// <param name="lcdDevice">The I2C device to control LCD display.</param>
        /// <param name="rgbDevice">The I2C device to control RGB backlight.</param>
        public LcdRgb1602(I2cDevice lcdDevice, I2cDevice rgbDevice)
            : base(new Size(16, 2))
        {
            _lcdDevice = lcdDevice;
            _rgbDevice = rgbDevice;

            Initialize(Size.Height);
            InitRgb();
        }

        public void Dispose()
        {
            if (_lcdDevice != null)
            {
                _lcdDevice.Dispose();
                _lcdDevice = null;
            }

            if (_rgbDevice != null)
            {
                _rgbDevice.Dispose();
                _rgbDevice = null;
            }
        }

        /// <summary>
        /// Enable/disable the backlight.
        /// </summary>
        public override bool BacklightOn
        {
            get { return _backlightOn; }

            set
            {
                _backlightOn = value;

                if (_backlightOn)
                {
                    ForceSetBacklightColor(_currentColor);
                }
                else
                {
                    ForceSetBacklightColor(Color.Black);
                }
            }
        }

        /// <summary>
        /// Sets register for RGB backlight.
        /// </summary>
        /// <param name="addr">The register address.</param>
        /// <param name="value">The register value.</param>
        private void SetRgbRegister(RgbRegisters addr, byte value)
        {
            Span<byte> dataToSend = stackalloc byte[2] { (byte)addr, value };
            _rgbDevice.Write(dataToSend);
        }

        /// <summary>
        /// Sets the backlight color without any checks.
        /// </summary>
        /// <param name="color">The color to set.</param>
        private void ForceSetBacklightColor(Color color)
        {
            SetRgbRegister(RgbRegisters.REG_RED, color.R);
            SetRgbRegister(RgbRegisters.REG_GREEN, color.G);
            SetRgbRegister(RgbRegisters.REG_BLUE, color.B);
        }

        /// <summary>
        /// Initializes RGB device.
        /// </summary>
        private void InitRgb()
        {
            // backlight init
            SetRgbRegister(RgbRegisters.REG_MODE1, 0);

            // set LEDs controllable by both PWM and GRPPWM registers
            SetRgbRegister(RgbRegisters.REG_LEDOUT, 0xFF);

            // set MODE2 values
            // 0010 0000 -> 0x20  (DMBLNK to 1, ie blinky mode)
            SetRgbRegister(RgbRegisters.REG_MODE2, 0x20);

            SetBacklightColor(Color.White);
        }

        /// <summary>
        /// Send a data or command byte to the controller.
        /// </summary>
        /// <param name="data">True to send data, otherwise sends a command.</param>
        protected override void Send(byte value, bool data = false)
        {
            Span<byte> dataToSend = stackalloc byte[2] { data ? (byte)0x40 : (byte)0x80, (byte)value };
            _lcdDevice.Write(dataToSend);
        }

        /// <summary>
        /// Initializes the bit mode settings.
        /// </summary>
        protected override void InitializeBitMode()
        {
            // Init to 8 bit mode
            DelayHelper.DelayMilliseconds(50, allowThreadYield: true);
            Send(0b0011_0000);
            DelayHelper.DelayMilliseconds(5, allowThreadYield: true);
            Send(0b0011_0000);
            DelayHelper.DelayMicroseconds(100, allowThreadYield: true);
            Send(0b0011_0000);
        }

        /// <summary>
        /// Sets the backlight color.
        /// The action will be ignored in case of the backlight is disabled.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public void SetBacklightColor(Color color)
        {
            if (!BacklightOn)
            {
                return;
            }

            ForceSetBacklightColor(color);
            _currentColor = color;
        }
    }
}
