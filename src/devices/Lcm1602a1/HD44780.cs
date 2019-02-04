// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

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
    public class Hd44780 : IDisposable
    {
        private const byte ClearDisplayCommand = 0b0001;
        private const byte ReturnHomeCommand   = 0b0010;

        private const byte SetCGRamAddressCommand = 0b0100_0000;
        private const byte SetDDRamAddressCommand = 0b1000_0000;

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

        private DisplayFunction _displayFunction = DisplayFunction.Command;
        private DisplayControl _displayControl = DisplayControl.Command;
        private DisplayEntryMode _displayMode = DisplayEntryMode.Command;

        private readonly byte[] _rowOffsets;
        private Stopwatch _stopwatch = new Stopwatch();
        private byte _lastByte;
        private bool _useLastByte;

        // We need to add PWM support to make this useful (to drive the VO pin).
        // For now we'll just stash the value and use it to decide the initial
        // backlight state.
        private float _backlightBrightness;

        /// <summary>
        /// Logical size, in characters, of the LCD.
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// The command wait time multiplier for the LCD.
        /// </summary>
        /// <remarks>
        /// Timings are based off the original HD44780 specification, which dates back
        /// to the late 1980s. Modern LCDs can often respond faster. In addition we spend
        /// time in other code between pulsing in new data, which gives quite a bit of
        /// headroom over the by-the-book waits.
        /// 
        /// This is more useful if you have a slow GpioAdapter as time spent in the
        /// adapter may eat into the need to wait as long for a command to complete.
        /// 
        /// There is a busy signal that can be checked that could make this moot, but
        /// currently we are unable to check the signal fast enough to make gains (or
        /// even equal) going off hard timings. The busy signal also requires having a
        /// r/w pin attached. And lastly, as already stated, it takes time to set up
        /// for another byte of data to be pulsed in.
        /// </remarks>
        public double TimingMultiplier { get; set; } = 1.0;

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
        {
            _rwPin = readWrite;
            _rsPin = registerSelect;
            _enablePin = enable;
            _dataPins = data;
            _backlight = backlight;
            _backlightBrightness = backlightBrightness;

            Size = size;

            _rowOffsets = InitializeRowOffsets(size.Height);

            if (data.Length == 8)
            {
                _displayFunction |= DisplayFunction.EightBit;
            }
            else if (data.Length != 4)
            {
                throw new ArgumentException($"The length of the array given to parameter {nameof(data)} must be 4 or 8");
            }

            _controller = controller ?? new GpioControllerAdapter(new GpioController(PinNumberingScheme.Logical));
            Initialize(size.Height);
        }

        protected virtual byte[] InitializeRowOffsets(int rows)
        {
            // In one-line mode DDRAM addresses go from 0 - 79 [0x00 - 0x4F]
            //
            // In two-line mode DDRAM addresses are laid out as follows:
            //
            //   First row:  0 - 39   [0x00 - 0x27]
            //   Second row: 64 - 103 [0x40 - 0x67]
            //
            // (The address gap presumably is to allow all second row addresses to be
            // identifiable with one bit? Not sure what the value of that is.)
            //
            // The chipset doesn't natively support more than two rows. For tested
            // four row displays the two rows are split as follows:
            //
            //   First row:  0 - 19   [0x00 - 0x13]
            //   Second row: 64 - 83  [0x40 - 0x53]
            //   Third row:  20 - 39  [0x14 - 0x27]
            //   Fourth row: 84 - 103 [0x54 - 0x67]

            byte[] rowOffsets;

            switch (rows)
            {
                case 1:
                    rowOffsets = new byte[1];
                    break;
                case 2:
                    rowOffsets = new byte[] { 0, 64 };
                    break;
                case 4:
                    rowOffsets = new byte[] { 0, 64, 20, 84 };
                    break;
                default:
                    // We don't support other rows, users can derive for odd cases.
                    // (Three row LCDs exist, but aren't common.)
                    throw new ArgumentOutOfRangeException(nameof(rows));
            }

            return rowOffsets;
        }

        protected virtual bool SetTwoLineMode(int rows) => rows > 1;

        public void Dispose()
        {
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }

        /// <summary>
        /// Initializes the display by setting the specified columns and lines.
        /// </summary>
        private void Initialize(int rows)
        {
            // While the chip supports 5x10 pixel characters for one line displays they
            // don't seem to be generally available. Supporting 5x10 would require extra
            // support for CreateCustomCharacter

            if (SetTwoLineMode(rows))
                _displayFunction |= DisplayFunction.TwoLine;

            _displayControl |= DisplayControl.DisplayOn;
            _displayMode |= DisplayEntryMode.Increment;

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

            // The busy flag cannot be checked until this point.

            Send((byte)_displayFunction);
            Send((byte)_displayControl);
            Send((byte)_displayMode);
            Clear();
        }

        /// <summary>
        /// Clears the LCD, returning the cursor to home and unshifting if shifted.
        /// Will also set to Increment.
        /// </summary>
        public void Clear()
        {
            Send(ClearDisplayCommand);
            DelayMicroseconds(3000);
        }

        /// <summary>
        /// Moves the cursor to the first line and first column.
        /// </summary>
        public void Home()
        {
            Send(ReturnHomeCommand);
            DelayMicroseconds(3000);
        }

        /// <summary>
        /// Moves the cursor to an explicit column and row position.
        /// </summary>
        /// <param name="left">The column position from left to right starting with 0.</param>
        /// <param name="top">The row position from the top starting with 0.</param>
        public void SetCursorPosition(int left, int top)
        {
            int rows = _rowOffsets.Length;
            if (top < 0 || top >= rows)
                throw new ArgumentOutOfRangeException(nameof(top));

            int newAddress = left + _rowOffsets[top];
            if (left < 0 || (rows == 1 && newAddress >= 80) || (rows > 1 && newAddress >= 104))
                throw new ArgumentOutOfRangeException(nameof(left));

            Send((byte)(SetDDRamAddressCommand | newAddress));
        }

        /// <summary>
        /// Enable/disable the display.
        /// </summary>
        public bool DisplayOn
        {
            get => (_displayControl & DisplayControl.DisplayOn) > 0;
            set => Send((byte)(value ? _displayControl |= DisplayControl.DisplayOn
                : _displayControl &= ~DisplayControl.DisplayOn));
        }

        /// <summary>
        /// Enable/disable the backlight. (Will always return false if no backlight pin was provided.)
        /// </summary>
        public bool BacklightOn
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
        /// Enable/disable the underline cursor.
        /// </summary>
        public bool UnderlineCursorVisible
        {
            get => (_displayControl & DisplayControl.CursorOn) > 0;
            set => Send((byte)(value ? _displayControl |= DisplayControl.CursorOn
                : _displayControl &= ~DisplayControl.CursorOn));
        }

        /// <summary>
        /// Enable/disable the blinking cursor.
        /// </summary>
        public bool BlinkingCursorVisible
        {
            get => (_displayControl & DisplayControl.BlinkOn) > 0;
            set => Send((byte)(value ? _displayControl |= DisplayControl.BlinkOn
                : _displayControl &= ~DisplayControl.BlinkOn));
        }

        /// <summary>
        /// When enabled the display will shift rather than the cursor.
        /// </summary>
        public bool AutoShift
        {
            get => (_displayMode & DisplayEntryMode.DisplayShift) > 0;
            set => Send((byte)(value ? _displayMode |= DisplayEntryMode.DisplayShift
                : _displayMode &= ~DisplayEntryMode.DisplayShift));
        }

        /// <summary>
        /// Gets/sets whether the cursor location increments (true) or decrements (false).
        /// </summary>
        public bool Increment
        {
            get => (_displayMode & DisplayEntryMode.Increment) > 0;
            set => Send((byte)(value ? _displayMode |= DisplayEntryMode.Increment
                : _displayMode &= ~DisplayEntryMode.Increment));
        }

        /// <summary>
        /// Move the display left one position.
        /// </summary>
        public void ShiftDisplayLeft() => Send((byte)(DisplayShift.Command | DisplayShift.Display));

        /// <summary>
        /// Move the display right one position.
        /// </summary>
        public void ShiftDisplayRight() => Send((byte)(DisplayShift.Command | DisplayShift.Display | DisplayShift.Right));

        /// <summary>
        /// Move the cursor left one position.
        /// </summary>
        public void ShiftCursorLeft() => Send((byte)(DisplayShift.Command | DisplayShift.Display));

        /// <summary>
        /// Move the cursor right one position.
        /// </summary>
        public void ShiftCursorRight() => Send((byte)(DisplayShift.Command | DisplayShift.Display | DisplayShift.Right));

        /// <summary>
        /// Fill one of the 8 CGRAM locations (character codes 0 - 7) with custom characters.
        /// </summary>
        /// <remarks>
        /// The custom characters also occupy character codes 8 - 15.
        /// 
        /// You can find help designing characters at https://www.quinapalus.com/hd44780udg.html.
        /// 
        /// The datasheet description for custom characters is very difficult to follow. Here is
        /// a rehash of the technical details that is hopefully easier:
        /// 
        /// Only 6 bits of addresses are available for character ram. That makes for 64 bytes of
        /// available character data. 8 bytes of data are used for each character, which is where
        /// the 8 total custom characters comes from (64/8).
        /// 
        /// Each byte corresponds to a character line. Characters are only 5 bits wide so only
        /// bits 0-4 are used for display. Whatever is in bits 5-7 is just ignored. Store bits
        /// there if it makes you happy, but it won't impact the display. '1' is on, '0' is off.
        /// 
        /// In the built-in characters the 8th byte is usually empty as this is where the underline
        /// cursor will be if enabled. You can put data there if you like, which gives you the full
        /// 5x8 character. The underline cursor just turns on the entire bottom row.
        /// 
        /// 5x10 mode is effectively useless as displays aren't available that utilize it. In 5x10
        /// mode *16* bytes of data are used for each character. That leaves room for only *4*
        /// custom characters. The first character is addressable from code 0, 1, 8, and 9. The
        /// second is 2, 3, 10, 11 and so on...
        /// 
        /// In this mode *11* bytes of data are actually used for the character data, which
        /// effectively gives you a 5x11 character, although typically the last line is blank to
        /// leave room for the underline cursor. Why the modes are referred to as 5x8 and 5x10 as
        /// opposed to 5x7 and 5x10 or 5x8 and 5x11 is a mystery. In an early pre-release data
        /// book 5x7 and 5x10 is used (Advance Copy #AP4 from July 1985). Perhaps it was a
        /// marketing change?
        /// 
        /// As only 11 bytes are used in 5x10 mode, but 16 bytes are reserved, the last 5 bytes
        /// are useless. The datasheet helpfully suggests that you can store your own data there.
        /// The same would be true for bits 5-7 of lines that matter for both 5x8 and 5x10.
        /// </remarks>
        /// <param name="location">Should be between 0 and 7</param>
        /// <param name="characterMap">Provide an array of 8 bytes containing the pattern</param>
        public void CreateCustomCharacter(byte location, params byte[] characterMap)
        {
            if (location > 7)
                throw new ArgumentOutOfRangeException(nameof(location));

            if (characterMap.Length != 8)
                throw new ArgumentException(nameof(characterMap));

            Send((byte)(SetCGRamAddressCommand | (location << 3)));

            for (int i = 0; i < 8; i++)
            {
                Send(characterMap[i], data: true);
            }
        }

        /// <summary>
        /// Write text to display.
        /// </summary>
        /// <remarks>
        /// There are only 256 characters available. There are chip variants
        /// with different character sets. Characters from space ' ' (32) to
        /// '}' are usually the same with the exception of '\', which is a
        /// yen symbol on some chips '¥'.
        /// </remarks>
        /// <param name="value">Text to be displayed.</param>
        public void Write(string value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                Send((byte)value[i], data: true);
            }
        }

        /// <summary>
        /// Send a data or command byte to the controller.
        /// </summary>
        /// <param name="data">True to send data, otherwise sends a command.</param>
        private void Send(byte value, bool data = false)
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
            PinValue ToPinValue(int bit) => (bit == 1) ? PinValue.High : PinValue.Low;

            for (int i = 0; i < count; i++)
            {
                int newBit = (value >> i) & 1;
                if (!_useLastByte)
                {
                    _controller.Write(_dataPins[i], ToPinValue(newBit));
                }
                else
                {
                    // Each bit change takes ~23μs, so only change what we have to
                    // This is particularly impactful when using all 8 data lines.
                    int oldBit = (_lastByte >> i) & 1;
                    if (oldBit != newBit)
                    {
                        _controller.Write(_dataPins[i], ToPinValue(newBit));
                    }
                }
            }

            _useLastByte = true;
            _lastByte = value;

            // Enable pin needs to be high for at least 450ns when running on 3V
            // and 230ns on 5V. (PWeh on page 49/52 and Figure 25 on page 58)

            _controller.Write(_enablePin, PinValue.High);
            DelayMicroseconds(1);
            _controller.Write(_enablePin, PinValue.Low);
        }

        private void DelayMicroseconds(int microseconds, bool checkBusy = true)
        {
            _stopwatch.Restart();

            // While we could check for the busy state it isn't currently practical.
            // Most commands need a maximum of 37μs to complete. Reading the busy flag
            // alone takes ~200μs (on the Pi). Prepping the pins and reading once can take
            // nearly a millisecond, which clearly is not going to be performant.
            // 
            // Leaving the flag here to make sure calling code is describing when it
            // cannot actually check the busy state should we be able to get performance
            // to the point where checking would be a net benefit.

            // Note that on a Raspberry Pi 3B+ we average about 1.5μs when delaying for one μs.
            // 
            // SpinWait currently spins to approximately 1μs before it will yield the thread.
            // Thread.Yield() takes around 1.2μs. This gives us a fidelity of microseconds to
            // microseconds + 1.4.
            SpinWait spinWait = new SpinWait();
            long v = (long)(((microseconds * Stopwatch.Frequency) / 1_000_000) * TimingMultiplier);
            while (_stopwatch.ElapsedTicks < v)
            {
                spinWait.SpinOnce();
            }
        }
    }
}
