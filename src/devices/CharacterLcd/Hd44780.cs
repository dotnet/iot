// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
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
    /// Samsung KS0066U, Aiptek AIP31066, and many more.
    ///
    /// Some compatible chips extend the HD44780 with addtional pins and features. They are still fully compatible. The ST7036 is one example.
    ///
    /// This implementation was drawn from numerous datasheets and libraries such as Adafruit_Python_CharLCD.
    /// </remarks>
    public class Hd44780 : ICharacterLcd, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Command which can be used to clear the display
        /// </summary>
        protected const byte ClearDisplayCommand = 0b_0001;

        /// <summary>
        /// Command which can be used to return (cursor) home
        /// </summary>
        protected const byte ReturnHomeCommand = 0b_0010;

        /// <summary>
        /// Command which can be used to set CG RAM address
        /// </summary>
        protected const byte SetCGRamAddressCommand = 0b_0100_0000;

        /// <summary>
        /// Command which can be used to set DD RAM address
        /// </summary>
        protected const byte SetDDRamAddressCommand = 0b_1000_0000;

        internal DisplayFunction _displayFunction = DisplayFunction.Command;
        internal DisplayControl _displayControl = DisplayControl.Command;
        internal DisplayEntryMode _displayMode = DisplayEntryMode.Command;

        /// <summary>
        /// Offsets of the rows
        /// </summary>
        protected readonly byte[] _rowOffsets;

        /// <summary>
        /// LCD interface used by the device
        /// </summary>
        protected readonly LcdInterface _lcdInterface;

        /// <summary>
        /// Initializes a new HD44780 LCD controller.
        /// </summary>
        /// <param name="size">The logical size of the LCD.</param>
        /// <param name="lcdInterface">The interface to use with the LCD.</param>
        public Hd44780(Size size, LcdInterface lcdInterface)
        {
            Size = size;
            _lcdInterface = lcdInterface;
            AutoShift = false;

            if (_lcdInterface.EightBitMode)
            {
                _displayFunction |= DisplayFunction.EightBit;
            }

            Initialize(size.Height);
            _rowOffsets = InitializeRowOffsets(size.Height);
        }

        /// <summary>
        /// Logical size, in characters, of the LCD.
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// Returns the number of custom characters for this display.
        /// A custom character is one that can be user-defined and assigned to a slot using <see cref="CreateCustomCharacter(byte, byte[])"/>
        /// </summary>
        public virtual int NumberOfCustomCharactersSupported => 8;

        /// <summary>
        /// Initializes the display by setting the specified columns and lines.
        /// </summary>
        private void Initialize(int rows)
        {
            // While the chip supports 5x10 pixel characters for one line displays they
            // don't seem to be generally available. Supporting 5x10 would require extra
            // support for CreateCustomCharacter
            if (GetTwoLineMode(rows))
            {
                _displayFunction |= DisplayFunction.TwoLine;
            }

            _displayControl |= DisplayControl.DisplayOn;
            _displayMode |= DisplayEntryMode.Increment;

            ReadOnlySpan<byte> commands = stackalloc byte[]
            {
                // Function must be set first to ensure that we always have the basic
                // instruction set selected. (See PCF2119x datasheet Function_set note
                // for one documented example of where this is necessary.)
                (byte)_displayFunction,
                (byte)_displayControl,
                (byte)_displayMode,
                ClearDisplayCommand
            };

            SendCommands(commands);
        }

        /// <summary>
        /// Enable/disable the backlight. (Will always return false if no backlight pin was provided.)
        /// </summary>
        public virtual bool BacklightOn
        {
            get => _lcdInterface.BacklightOn;
            set => _lcdInterface.BacklightOn = value;
        }

        /// <summary>
        /// Sends byte to the device
        /// </summary>
        /// <param name="value">Byte to be sent to the device</param>
        protected void SendData(byte value) => _lcdInterface.SendData(value);

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Byte representing the command to be sent</param>
        protected void SendCommand(byte command) => _lcdInterface.SendCommand(command);

        /// <summary>
        /// Sends data to the device
        /// </summary>
        /// <param name="values">Data to be send to the device</param>
        protected void SendData(ReadOnlySpan<byte> values) => _lcdInterface.SendData(values);

        /// <summary>
        /// Send commands to the device
        /// </summary>
        /// <param name="commands">Each byte represents command being sent to the device</param>
        protected void SendCommands(ReadOnlySpan<byte> commands) => _lcdInterface.SendCommands(commands);

        /// <summary>
        /// Determines if the device should use two line mode
        /// </summary>
        /// <param name="rows">Number of rows on the device</param>
        /// <returns>True if device should use two line mode</returns>
        protected virtual bool GetTwoLineMode(int rows) => rows > 1;

        /// <summary>
        /// Initializes row offsets
        /// </summary>
        /// <param name="rows">Rows to be initialized</param>
        /// <returns>Array of offsets</returns>
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
            //   Third row:  20 - 39  [0x14 - 0x27]  (Continues first row)
            //   Fourth row: 84 - 103 [0x54 - 0x67]  (Continues second row)
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
        /// Wait for the device to not be busy.
        /// </summary>
        /// <param name="microseconds">Time to wait if checking busy state isn't possible/practical.</param>
        protected void WaitForNotBusy(int microseconds)
        {
            _lcdInterface.WaitForNotBusy(microseconds);
        }

        /// <summary>
        /// Clears the LCD, returning the cursor to home and unshifting if shifted.
        /// Will also set to Increment.
        /// </summary>
        public void Clear()
        {
            SendCommand(ClearDisplayCommand);

            // The HD44780 spec doesn't call out how long this takes. Home is documented as
            // taking 1.52ms, and as this does more work (sets all memory to the space character)
            // we do a longer wait. On the PCF2119x it is described as taking 165 clock cycles which
            // would be 660μs on the "typical" clock.
            WaitForNotBusy(2000);
        }

        /// <summary>
        /// Moves the cursor to the first line and first column, unshifting if shifted.
        /// </summary>
        public void Home()
        {
            SendCommand(ReturnHomeCommand);

            // The return home command is documented as taking 1.52ms with the standard 270KHz clock.
            // SendCommand already waits for 37μs,
            WaitForNotBusy(1520);
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
            {
                throw new ArgumentOutOfRangeException(nameof(top));
            }

            // Throw if we're given a negative left value or the calculated address would be
            // larger than the max "good" address. Addressing is covered in detail in
            // InitializeRowOffsets above.
            int newAddress = left + _rowOffsets[top];
            if (left < 0 || (rows == 1 && newAddress >= 80) || (rows > 1 && newAddress >= 104))
            {
                throw new ArgumentOutOfRangeException(nameof(left));
            }

            SendCommand((byte)(SetDDRamAddressCommand | newAddress));
        }

        /// <summary>
        /// Enable/disable the display.
        /// </summary>
        public bool DisplayOn
        {
            get => (_displayControl & DisplayControl.DisplayOn) > 0;
            set => SendCommand((byte)(value ? _displayControl |= DisplayControl.DisplayOn
                : _displayControl &= ~DisplayControl.DisplayOn));
        }

        /// <summary>
        /// Enable/disable the underline cursor.
        /// </summary>
        public bool UnderlineCursorVisible
        {
            get => (_displayControl & DisplayControl.CursorOn) > 0;
            set => SendCommand((byte)(value ? _displayControl |= DisplayControl.CursorOn
                : _displayControl &= ~DisplayControl.CursorOn));
        }

        /// <summary>
        /// Enable/disable the blinking cursor.
        /// </summary>
        public bool BlinkingCursorVisible
        {
            get => (_displayControl & DisplayControl.BlinkOn) > 0;
            set => SendCommand((byte)(value ? _displayControl |= DisplayControl.BlinkOn
                : _displayControl &= ~DisplayControl.BlinkOn));
        }

        /// <summary>
        /// When enabled the display will shift rather than the cursor.
        /// </summary>
        public bool AutoShift
        {
            get => (_displayMode & DisplayEntryMode.DisplayShift) > 0;
            set => SendCommand((byte)(value ? _displayMode |= DisplayEntryMode.DisplayShift
                : _displayMode &= ~DisplayEntryMode.DisplayShift));
        }

        /// <summary>
        /// Gets/sets whether the cursor location increments (true) or decrements (false).
        /// </summary>
        public bool Increment
        {
            get => (_displayMode & DisplayEntryMode.Increment) > 0;
            set => SendCommand((byte)(value ? _displayMode |= DisplayEntryMode.Increment
                : _displayMode &= ~DisplayEntryMode.Increment));
        }

        /// <summary>
        /// Move the display left one position.
        /// </summary>
        public void ShiftDisplayLeft() => SendCommand((byte)(DisplayShift.Command | DisplayShift.Display));

        /// <summary>
        /// Move the display right one position.
        /// </summary>
        public void ShiftDisplayRight() => SendCommand((byte)(DisplayShift.Command | DisplayShift.Display | DisplayShift.Right));

        /// <summary>
        /// Move the cursor left one position.
        /// </summary>
        public void ShiftCursorLeft() => SendCommand((byte)(DisplayShift.Command | DisplayShift.Display));

        /// <summary>
        /// Move the cursor right one position.
        /// </summary>
        public void ShiftCursorRight() => SendCommand((byte)(DisplayShift.Command | DisplayShift.Display | DisplayShift.Right));

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
            if (characterMap == null)
            {
                throw new ArgumentNullException(nameof(characterMap));
            }

            CreateCustomCharacter(location, characterMap.AsSpan());
        }

        /// <summary>
        /// Fill one of the 8 CGRAM locations (character codes 0 - 7) with custom characters.
        /// </summary>
        /// <param name="location">Should be between 0 and 7</param>
        /// <param name="characterMap">Provide an array of 8 bytes containing the pattern</param>
        public void CreateCustomCharacter(byte location, ReadOnlySpan<byte> characterMap)
        {
            if (location >= NumberOfCustomCharactersSupported)
            {
                throw new ArgumentOutOfRangeException(nameof(location));
            }

            if (characterMap.Length != 8)
            {
                throw new ArgumentException("The character map must be exactly 8 bytes long", nameof(characterMap));
            }

            // The character address is set in bits 3-5 of the command byte
            SendCommand((byte)(SetCGRamAddressCommand | (location << 3)));
            SendData(characterMap);
        }

        /// <summary>
        /// Write text to display.
        /// </summary>
        /// <param name="text">Text to be displayed.</param>
        /// <remarks>
        /// There are only 256 characters available. There are chip variants
        /// with different character sets. Characters from space ' ' (32) to
        /// '}' are usually the same with the exception of '\', which is a
        /// yen symbol on some chips '¥'.
        /// </remarks>
        public void Write(string text)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(text.Length);
            for (int i = 0; i < text.Length; ++i)
            {
                buffer[i] = (byte)text[i];
            }

            SendData(new ReadOnlySpan<byte>(buffer, 0, text.Length));
            ArrayPool<byte>.Shared.Return(buffer);
        }

        /// <summary>
        /// Write a raw byte stream to the display.
        /// Used if character translation already took place
        /// </summary>
        /// <param name="text">Text to print</param>
        public void Write(ReadOnlySpan<byte> text)
        {
           SendData(text);
        }

        /// <summary>
        /// Releases unmanaged resources used by Hd44780
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lcdInterface?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }
        }
    }
}
