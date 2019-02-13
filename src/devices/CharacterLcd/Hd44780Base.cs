// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Base class for LCD character displays compatible with the HD44780 LCD controller/driver.
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
    public abstract class Hd44780Base
    {
        private Stopwatch _stopwatch = new Stopwatch();

        protected const byte ClearDisplayCommand = 0b_0001;
        protected const byte ReturnHomeCommand   = 0b_0010;

        protected const byte SetCGRamAddressCommand = 0b_0100_0000;
        protected const byte SetDDRamAddressCommand = 0b_1000_0000;

        internal DisplayFunction _displayFunction = DisplayFunction.Command;
        internal DisplayControl _displayControl = DisplayControl.Command;
        internal DisplayEntryMode _displayMode = DisplayEntryMode.Command;

        protected readonly byte[] _rowOffsets;

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
        /// <param name="size">The logical size of the LCD.</param>
        protected Hd44780Base(Size size)
        {
            Size = size;
            _rowOffsets = InitializeRowOffsets(size.Height);
        }

        protected virtual bool SetTwoLineMode(int rows) => rows > 1;

        /// <summary>
        /// Enable/disable the backlight.
        /// </summary>
        public abstract bool BacklightOn { get; set; }

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

        /// <summary>
        /// Initializes the bit mode settings of specific device.
        /// </summary>
        protected abstract void InitializeBitMode();

        /// <summary>
        /// Send a data or command byte to the controller.
        /// </summary>
        /// <param name="data">True to send data, otherwise sends a command.</param>
        protected abstract void Send(byte value, bool data = false);

        /// <summary>
        /// Initializes the display by setting the specified columns and lines.
        /// </summary>
        protected void Initialize(int rows)
        {
            // While the chip supports 5x10 pixel characters for one line displays they
            // don't seem to be generally available. Supporting 5x10 would require extra
            // support for CreateCustomCharacter

            if (SetTwoLineMode(rows))
                _displayFunction |= DisplayFunction.TwoLine;

            _displayControl |= DisplayControl.DisplayOn;
            _displayMode |= DisplayEntryMode.Increment;

            InitializeBitMode();

            // The busy flag cannot be checked until this point.

            Send((byte)_displayFunction);
            Send((byte)_displayControl);
            Send((byte)_displayMode);
            Clear();
        }

        protected void DelayMicroseconds(int microseconds, bool checkBusy = true)
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
    }
}