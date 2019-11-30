// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// This is a high-level interface to an LCD display. 
    /// It supports automatic wrapping of text, automatic scrolling and code page mappings.
    /// This class is thread safe, however using Write from different threads may lead to unexpected results, since the order is not guaranteed. 
    /// </summary>
    public sealed class LcdConsole : IDisposable
    {
        private ICharacterLcd _lcd;

        /// <summary>
        /// The text currently on the display (required for arbitrary scrolling)
        /// </summary>
        private StringBuilder[] _currentData;
        private LineFeedMode _lineFeedMode;
        private readonly object _lock;
        private readonly bool _shouldDispose;
        private TimeSpan _scrollUpDelay;
        private string _romType;
        private Encoding _characterEncoding;

        /// <summary>
        /// Creates a new instance of the <see cref="LcdConsole"/> class using the specified LCD low-level interface. 
        /// This class automatically configures the low-level interface. Do not use the low-level interface at the same time. 
        /// </summary>
        /// <param name="lcd">The low-level LCD interface.</param>
        /// <param name="romType">Name of character ROM of display. Currently supported types: A00 and A02.</param>
        /// <param name="shouldDispose">If the class should dispose the LCD driver when it is disposed. Defaults to true</param>
        public LcdConsole(ICharacterLcd lcd, string romType, bool shouldDispose = true)
        {
            _lcd = lcd;
            _shouldDispose = shouldDispose;
            _romType = romType;
            _characterEncoding = null;
            Size = lcd.Size;
            _currentData = new StringBuilder[Size.Height];
            CursorLeft = 0;
            CursorTop = 0;
            ScrollUpDelay = TimeSpan.Zero;
            _lock = new object();
            _lcd.UnderlineCursorVisible = false;
            _lcd.BlinkingCursorVisible = false;
            _lcd.DisplayOn = true;
            _lcd.BacklightOn = true;
            ClearStringBuffer();
            _lcd.Clear();
        }

        /// <summary>
        /// Position of the cursor, from left.
        /// Note: May be outside the bounds of the display.
        /// </summary>
        public int CursorLeft
        {
            get;
            private set;
        }

        /// <summary>
        /// Position of the cursor, from top
        /// Note: May be outside the bounds of the display.
        /// </summary>
        public int CursorTop
        {
            get;
            private set;
        }

        /// <summary>
        /// If this is larger than zero, an a wait is introduced each time the display wraps to the next line or scrolls up. Can be used to print long texts to the display, 
        /// but keeping it readable. 
        /// </summary>
        public TimeSpan ScrollUpDelay
        {
            get
            {
                return _scrollUpDelay;
            }

            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Timespan must be positive");
                }
                _scrollUpDelay = value;
            }
        }

        /// <summary>
        /// Enables or disables the backlight
        /// </summary>
        public bool BacklightOn
        {
            get
            {
                lock (_lock)
                {
                    return _lcd.BacklightOn;
                }
            }
            set
            {
                lock (_lock)
                {
                    _lcd.BacklightOn = value;
                }
            }
        }

        /// <summary>
        /// Enables or disables the display
        /// </summary>
        public bool DisplayOn
        {
            get
            {
                lock (_lock)
                {
                    return _lcd.DisplayOn;
                }
            }
            set
            {
                lock (_lock)
                {
                    _lcd.DisplayOn = value;
                }
            }
        }

        /// <summary>
        /// Sets the Line Feed Mode. 
        /// This defines what happens when writting past the end of the line/screen.
        /// </summary>
        public LineFeedMode LineFeedMode
        {
            get
            {
                return _lineFeedMode;
            }
            set
            {
                _lineFeedMode = value;
            }
        }

        /// <summary>
        /// Size of the display
        /// </summary>
        public Size Size
        {
            get;
        }

        private void ClearStringBuffer()
        {
            for (int i = 0; i < Size.Height; i++)
            {
                // The display has now only spaces on it. 
                _currentData[i] = new StringBuilder(new String(' ', Size.Width));
            }
        }

        /// <summary>
        /// Clears the screen and sets the cursor back to the start.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _lcd.Clear();
                ClearStringBuffer();
                SetCursorPosition(0, 0);
            }
        }

        /// <summary>
        /// Moves the cursor to an explicit column and row position.
        /// The position may be outside the bounds of the display. Any subsequent writes will then have no effect, unless <see cref="LineFeedMode"/> allows it or a newline character is written.
        /// </summary>
        /// <param name="left">The column position from left to right starting with 0.</param>
        /// <param name="top">The row position from the top starting with 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">The new position negative.</exception>
        public void SetCursorPosition(int left, int top)
        {
            lock (_lock)
            {
                if (left < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(left));
                }
                if (top < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(top));
                }
                if (left < Size.Width && top < Size.Height)
                {
                    _lcd.SetCursorPosition(left, top);
                }
                CursorLeft = left;
                CursorTop = top;
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
        /// <param name="text">Text to be displayed.</param>
        public void Write(string text)
        {
            text = text.Replace("\r\n", "\n"); // Change to linux format only, so we have to consider only this case further

            List<string> lines = text.Split("\n", StringSplitOptions.None).ToList();
            FindLineWraps(CursorLeft, lines);
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string currentLine = line;

                int remainingChars = Math.Min(currentLine.Length, Size.Width - CursorLeft);
                if (remainingChars > 0)
                {
                    lock (_lock)
                    {
                        WriteCurrentLine(currentLine.Substring(0, remainingChars));
                    }
                }

                if (i != lines.Count - 1)
                {
                    // Insert newline except after the last text segment.
                    NewLine();
                }
            }

        }

        /// <summary>
        /// Asynchronously writes text to the display.
        /// See <see cref="Write(string)"/> for limitations.
        /// Only one write operation (synchronous or asynchronous) should be executed at once, otherwise the execution order 
        /// is undefined. 
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <returns>A task object</returns>
        public Task WriteAsync(string text)
        {
            return Task.Factory.StartNew(() =>
            {
                Write(text);
            });
        }

        /// <summary>
        /// Replaces the text of the given line. 
        /// This will overwrite the text in the given line, filling up with spaces, if needed. 
        /// This will never wrap to the next line, and line feeds in the input string are not allowed. 
        /// </summary>
        /// <param name="lineNumber">0-based index of the line to start</param>
        /// <param name="text">Text to insert. No newlines supported.</param>
        /// <exception cref="ArgumentException">The string contains newlines.</exception>
        public void ReplaceLine(int lineNumber, string text)
        {
            if (text.Contains("\n"))
            {
                // This is to simplify usage. You would normally use this method to replace one line with new text (i.e. replace only the active element, not affecting the menu title)
                throw new ArgumentException("The string may not contain line feeds");
            }
            lock (_lock)
            {
                SetCursorPosition(0, lineNumber);
                if (text.Length > Size.Width)
                {
                    text = text.Substring(0, Size.Width);
                }
                else if (text.Length < Size.Width)
                {
                    text = text + new string(' ', Size.Width - text.Length);
                }

                WriteCurrentLine(text);
            }
        }

        /// <summary>
        /// Find where we need to insert additional newlines
        /// </summary>
        private void FindLineWraps(int cursorPos, List<string> lines)
        {
            if (LineFeedMode == LineFeedMode.Wrap || LineFeedMode == LineFeedMode.WordWrap)
            {
                int roomOnLine = Size.Width - cursorPos;
                roomOnLine = Math.Max(roomOnLine, 0);
                for (int i = 0; i < lines.Count; i++)
                {
                    string remaining = lines[i];
                    if (remaining.Length > roomOnLine)
                    {
                        if (LineFeedMode == LineFeedMode.WordWrap)
                        {
                            // In intelligent mode, try finding spaces backwards
                            // Note: this indexes the element 1 char after the last to be printed in the first iteration, since there might be a space just there
                            int tempRoomOnLine = roomOnLine;
                            while (remaining[tempRoomOnLine] != ' ' && tempRoomOnLine > 0)
                            {
                                tempRoomOnLine--;
                            }

                            // If we didn't find a space, break after the maximum number of chars
                            if (tempRoomOnLine > 0)
                            {
                                roomOnLine = tempRoomOnLine;
                            }
                        }

                        // This line does not fit on the remaining room of this row
                        string firstPart = remaining.Substring(0, roomOnLine);

                        lines[i] = firstPart;
                        // Insert the remaining part as new entry to the list, continue iteration there
                        string remainingPart = remaining.Substring(roomOnLine).TrimStart(' ');
                        lines.Insert(i + 1, remainingPart);

                    }
                    roomOnLine = Size.Width; // From now on, we have the full width available
                }
            }
        }

        /// <summary>
        /// Writes a newline to the display (wrapping to the next line)
        /// </summary>
        public void WriteLine()
        {
            NewLine();
        }

        /// <summary>
        /// Writes the given text to the current position, then wraps to the next line
        /// </summary>
        /// <param name="text">Text to draw.</param>
        public void WriteLine(string text)
        {
            Write(text);
            NewLine();
        }

        /// <summary>
        /// Blinks the display text (and the backlight, if available).
        /// Can be used to get user attention. 
        /// Operation is synchronous.
        /// </summary>
        /// <param name="times">Number of times to blink. The blink rate is 1 Hz</param>
        public void BlinkDisplay(int times)
        {
            for (int i = 0; i < times; i++)
            {
                lock (_lock)
                {
                    _lcd.DisplayOn = false;
                    _lcd.BacklightOn = false;
                }
                Thread.Sleep(500);
                lock (_lock)
                {
                    _lcd.DisplayOn = true;
                    _lcd.BacklightOn = true;
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Blinks the display in the background
        /// </summary>
        /// <param name="times">Number of times to blink</param>
        /// <returns>A task handle</returns>
        public Task BlinkDisplayAsync(int times)
        {
            return Task.Factory.StartNew(() => BlinkDisplay(times));
        }

        private void NewLine()
        {
            // Wait before scrolling, so that the last line may also contain text
            if (_scrollUpDelay > TimeSpan.Zero)
            {
                Thread.Sleep(_scrollUpDelay);
            }
            lock (_lock)
            {
                if (CursorTop < Size.Height - 1)
                {
                    SetCursorPosition(0, CursorTop + 1);
                }
                else if (LineFeedMode == LineFeedMode.WordWrap || LineFeedMode == LineFeedMode.Wrap)
                {
                    ScrollUp();
                    SetCursorPosition(0, Size.Height - 1); // Go to beginning of last line
                }
                else
                {
                    CursorLeft = Size.Width; // We are outside the screen, any further writes will be ignored
                }
            }
        }

        /// <summary>
        /// Refreshes the display from the cache (i.e. after a scroll operation)
        /// Does not change the cursor position
        /// </summary>
        private void RefreshFromBuffer()
        {
            int x = CursorLeft;
            int y = CursorTop;
            for (int i = 0; i < Size.Height; i++)
            {
                _lcd.SetCursorPosition(0, i);
                byte[] buffer = MapChars(_currentData[i].ToString());
                _lcd.Write(buffer);
            }
            SetCursorPosition(x, y);
        }

        /// <summary>
        /// Scrolls the text up by one row, clearing the last row. Does not change the cursor position.
        /// Implementation note: Caller must own the lock. 
        /// </summary>
        private void ScrollUp()
        {
            for (int i = 1; i < Size.Height; i++)
            {
                _currentData[i - 1] = _currentData[i];
            }
            _currentData[Size.Height - 1] = new StringBuilder(new String(' ', Size.Width));
            RefreshFromBuffer();
        }

        /// <summary>
        /// This is expected to be called only with a string length of less or equal the remaining number of characters on the current line. 
        /// Implementation note: Caller must own the lock. 
        /// </summary>
        private void WriteCurrentLine(string line)
        {
            // Replace the existing chars at the given position with the new text
            _currentData[CursorTop].Remove(CursorLeft, line.Length);
            _currentData[CursorTop].Insert(CursorLeft, line);
            byte[] buffer = MapChars(line);
            _lcd.Write(buffer);
            CursorLeft += line.Length;
        }

        /// <summary>
        /// Creates an encoding that can be used for an LCD display. 
        /// Typically, the returned value will be loaded using <see cref="LoadEncoding(LcdCharacterEncoding)"/>.
        /// </summary>
        /// <param name="culture">Required display culture (forwarded to the factory)</param>
        /// <param name="romType">The name of the ROM for which the encoding is to be applied. The default factory supports roms A00 and A02.</param>
        /// <param name="unknownCharacter">The character to print for unknown letters, default: ?</param>
        /// <param name="maxNumberOfCustomCharacters">The maximum number of custom characters supported by the hardware.</param>
        /// <param name="factory">Character encoding factory that delivers the mapping of the Char type to the hardware ROM character codes. May add special characters into 
        /// the character ROM. Default: Null (Use internal factory)</param>
        public static LcdCharacterEncoding CreateEncoding(CultureInfo culture, string romType, char unknownCharacter = '?', int maxNumberOfCustomCharacters = 8, LcdCharacterEncodingFactory factory = null)
        {
            if (factory == null)
            {
                factory = new LcdCharacterEncodingFactory();
            }

            return factory.Create(culture, romType, unknownCharacter, maxNumberOfCustomCharacters);
        }

        /// <summary>
        /// Loads the specified encoding. 
        /// This behaves as <see cref="LoadEncoding(LcdCharacterEncoding)"/> when the argument is of the dynamic type <see cref="LcdCharacterEncoding"/>, otherwise like an encding 
        /// with no special characters.
        /// </summary>
        /// <param name="encoding">The encoding to load.</param>
        /// <returns>See true if the encoding was correctly loaded.</returns>
        public bool LoadEncoding(Encoding encoding)
        {
            LcdCharacterEncoding lcdCharacterEncoding = encoding as LcdCharacterEncoding;
            if (lcdCharacterEncoding != null)
            {
                return LoadEncoding(encoding);
            }
            lock (_lock)
            {
                _characterEncoding = encoding;
            }
            return true;
        }

        /// <summary>
        /// Loads the specified character encoding. This loads any custom characters from the encoding to the display. 
        /// </summary>
        /// <param name="encoding">The encoding to load.</param>
        /// <returns>True if the character encoding was successfully loaded, false if there are not enough custom slots for all the required custom characters.
        /// This may also return false if the encoding factory returned incomplete results, such as a missing custom character for a special diacritic.</returns>
        public bool LoadEncoding(LcdCharacterEncoding encoding)
        {
            bool allCharactersLoaded = encoding.AllCharactersSupported;
            lock (_lock)
            {
                int numberOfCharctersToLoad = Math.Min(encoding.ExtraCharacters.Count, _lcd.NumberOfCustomCharactersSupported);
                if (numberOfCharctersToLoad < encoding.ExtraCharacters.Count)
                {
                    // We can't completelly load that encoding, because there are not enough custom slots. 
                    allCharactersLoaded = false;
                }
                for (byte i = 0; i < numberOfCharctersToLoad; i++)
                {
                    byte[] pixelMap = encoding.ExtraCharacters[i];
                    _lcd.CreateCustomCharacter(i, pixelMap);
                }

                _characterEncoding = encoding;
            }
            return allCharactersLoaded;
        }

        /// <summary>
        /// Resets the character encoding to hardware defaults (using simply the lower byte of a char).
        /// </summary>
        public void ResetEncoding()
        {
            lock (_lock)
            {
                _characterEncoding = null;
            }
        }

        private byte[] MapChars(string line)
        {
            byte[] buffer = new byte[line.Length];
            if (_characterEncoding == null)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    buffer[i] = (byte)line[i];
                }
                return buffer;
            }
            else
            {
                return _characterEncoding.GetBytes(line);
            }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _lcd.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
