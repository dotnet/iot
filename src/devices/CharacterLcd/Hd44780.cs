// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Drawing;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Supports LCD character displays compatible with the HD44780 LCD controller/driver.
    /// Also supports serial interface adapters such as the MCP23008.
    /// </summary>
    /// <remarks>
    /// The Hitatchi HD44780 was released in 1987 and set the standard for LCD controllers. Hitatchi does not make this chipset anymore, but
    /// most character LCD drivers are intended to be fully compatible with this chipset. Some examples: Sunplus SPLC780D, Sitronix ST7066U,
    /// Samsung KS0066U (and many more).
    /// 
    /// Some compatible chips extend the HD44780 with addtional pins and features. They are still fully compatible. The ST7036 is one example.
    /// 
    /// This implementation was drawn from numerous datasheets and libraries such as Adafruit_Python_CharLCD.
    /// </remarks>
    public class Hd44780 : Hd44780Base, IDisposable
    {
        /// <summary>
        /// Register select pin. Low is for writing to the instruction
        /// register and reading the address counter. High is for reading
        /// and writing to the data register.
        /// </summary>
        private readonly int _rsPin;

        /// <summary>
        /// Read/write pin. Low for write, high for read.
        /// </summary>
        private readonly int _rwPin;

        /// <summary>
        /// Enable pin. Pulse high to process a read/write.
        /// </summary>
        private readonly int _enablePin;

        private readonly int _backlight;

        private readonly int[] _dataPins;

        private IGpioController _controller;

        private byte _lastByte;
        private bool _useLastByte;

        private PinValuePair[] _pinBuffer = new PinValuePair[8];

        // We need to add PWM support to make this useful (to drive the VO pin).
        // For now we'll just stash the value and use it to decide the initial
        // backlight state.
        private float _backlightBrightness;

        /// <summary>
        /// Initializes a new HD44780 LCD controller.
        /// </summary>
        /// <param name="registerSelect">The pin that controls the regsiter select.</param>
        /// <param name="enable">The pin that controls the enable switch.</param>
        /// <param name="data">Collection of pins holding the data that will be printed on the screen.</param>
        /// <param name="size">The logical size of the LCD.</param>
        /// <param name="backlight">The optional pin that controls the backlight of the display.</param>
        /// <param name="backlightBrightness">The brightness of the backlight. 0.0 for off, 1.0 for on.</param>
        /// <param name="readWrite">The optional pin that controls the read and write switch.</param>
        /// <param name="controller">The controller to use with the LCD. If not specified, uses the platform default.</param>
        public Hd44780(int registerSelect, int enable, int[] data, Size size, int backlight = -1, float backlightBrightness = 1.0f, int readWrite = -1, IGpioController controller = null)
            : base(size)
        {
            _rwPin = readWrite;
            _rsPin = registerSelect;
            _enablePin = enable;
            _dataPins = data;
            _backlight = backlight;
            _backlightBrightness = backlightBrightness;

            if (data.Length == 8)
            {
                _displayFunction |= DisplayFunction.EightBit;
            }
            else if (data.Length != 4)
            {
                throw new ArgumentException($"The length of the array given to parameter {nameof(data)} must be 4 or 8");
            }

            _controller = controller ?? new GpioController(PinNumberingScheme.Logical);
            Initialize(size.Height);
        }

        public void Dispose()
        {
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }

        /// <summary>
        /// Initializes the bit mode settings.
        /// </summary>
        protected override void InitializeBitMode()
        {
            // Prep the pins
            _controller.OpenPin(_rsPin, PinMode.Output);

            if (_rwPin != -1)
            {
                _controller.OpenPin(_rwPin, PinMode.Output);
            }
            if (_backlight != -1)
            {
                _controller.OpenPin(_backlight, PinMode.Output);
                if (_backlightBrightness > 0)
                {
                    // Turn on the backlight
                    _controller.Write(_backlight, PinValue.High);
                }
            }
            _controller.OpenPin(_enablePin, PinMode.Output);

            for (int i = 0; i < _dataPins.Length; ++i)
            {
                _controller.OpenPin(_dataPins[i], PinMode.Output);
            }

            // The HD44780 self-initializes when power is turned on to the following settings:
            // 
            //  - 8 bit, 1 line, 5x7 font
            //  - Display, cursor, and blink off
            //  - Increment with no shift
            //
            // It is possible that the initialization will fail if the power is not provided
            // within specific tolerances. As such, we'll always perform the software based
            // initialization as described on pages 45/46 of the HD44780 data sheet. We give
            // a little extra time to the required waits.

            if (_dataPins.Length == 8)
            {
                // Init to 8 bit mode
                DelayMicroseconds(50_000, checkBusy: false);
                Send(0b0011_0000);
                DelayMicroseconds(5_000, checkBusy: false);
                Send(0b0011_0000);
                DelayMicroseconds(100, checkBusy: false);
                Send(0b0011_0000);
            }
            else
            {
                // Init to 4 bit mode, setting _rspin to low as we're writing 4 bits directly.
                // (Send writes the whole byte in two 4bit/nybble chunks)
                _controller.Write(_rsPin, PinValue.Low);
                DelayMicroseconds(50_000, checkBusy: false);
                WriteBits(0b0011, 4);
                DelayMicroseconds(5_000, checkBusy: false);
                WriteBits(0b0011, 4);
                DelayMicroseconds(100, checkBusy: false);
                WriteBits(0b0011, 4);
                WriteBits(0b0010, 4);
            }
        }

        /// <summary>
        /// Enable/disable the backlight. (Will always return false if no backlight pin was provided.)
        /// </summary>
        public override bool BacklightOn
        {
            get
            {
                return _backlight != -1 && _controller.Read(_backlight) == PinValue.High;
            }
            set
            {
                if (_backlight != -1)
                    _controller.Write(_backlight, value ? PinValue.High : PinValue.Low);
            }
        }

        /// <summary>
        /// Send a data or command byte to the controller.
        /// </summary>
        /// <param name="data">True to send data, otherwise sends a command.</param>
        protected override void Send(byte value, bool data = false)
        {
            _controller.Write(_rsPin, data ? PinValue.High : PinValue.Low);

            if (_rwPin != -1)
            {
                _controller.Write(_rwPin, PinValue.Low);
            }

            if (_dataPins.Length == 8)
            {
                WriteBits(value, 8);
            }
            else
            {
                WriteBits((byte)(value >> 4), 4);
                WriteBits(value, 4);
            }

            // Most commands need a maximum of 37μs to complete.
            // This is based on a 270kHz clock in the documentation.
            // (See page 25.)
            DelayMicroseconds(37);
        }

        private void WriteBits(byte value, int count)
        {
            int changedCount = 0;
            for (int i = 0; i < count; i++)
            {
                int newBit = (value >> i) & 1;
                if (!_useLastByte)
                {
                    _pinBuffer[changedCount++] = new PinValuePair(_dataPins[i], newBit);
                }
                else
                {
                    // Each bit change takes ~23μs, so only change what we have to
                    // This is particularly impactful when using all 8 data lines.
                    int oldBit = (_lastByte >> i) & 1;
                    if (oldBit != newBit)
                    {
                        _pinBuffer[changedCount++] = new PinValuePair(_dataPins[i], newBit);
                    }
                }
            }

            if (changedCount > 0)
                _controller.Write(new ReadOnlySpan<PinValuePair>(_pinBuffer, 0, changedCount));

            _useLastByte = true;
            _lastByte = value;

            // Enable pin needs to be high for at least 450ns when running on 3V
            // and 230ns on 5V. (PWeh on page 49/52 and Figure 25 on page 58)

            _controller.Write(_enablePin, PinValue.High);
            DelayMicroseconds(1);
            _controller.Write(_enablePin, PinValue.Low);
        }
    }
}
