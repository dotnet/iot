// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Graphics;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Displays a value and an unit in a big font on an LCD Display.
    /// Requires a display with at least 20x4 characters
    /// </summary>
    public class LcdValueUnitDisplay
    {
        private const string BigFontMap = "BigFontMap.txt";
        private readonly object _lock;
        private readonly ICharacterLcd _lcd;
        private readonly CultureInfo _culture;
        private readonly Dictionary<char, byte[]> _font;
        private char _currentSeparationChar;
        private LcdCharacterEncoding? _encoding;

        /// <summary>
        /// Creates an instance of <see cref="LcdValueUnitDisplay"/>
        /// </summary>
        /// <param name="lcd">Interface to the display</param>
        /// <param name="culture">User culture</param>
        public LcdValueUnitDisplay(ICharacterLcd lcd, CultureInfo culture)
        {
            _lock = new object();
            _lcd = lcd;
            _culture = culture;
            _font = new Dictionary<char, byte[]>();
            if (lcd.Size.Width < 20 || lcd.Size.Height < 4)
            {
                throw new NotSupportedException("This class can only run on displays with at least 20x4 characters.");
            }

            if (lcd.NumberOfCustomCharactersSupported < 8)
            {
                throw new NotSupportedException("This class can only run on displays with 8 or more custom character slots");
            }
        }

        /// <summary>
        /// Returns the active culture.
        /// </summary>
        public CultureInfo Culture
        {
            get
            {
                return _culture;
            }
        }

        /// <summary>
        /// Initializes the display for use as a big-number display.
        /// Configures the display with some graphic blocks for the display of big numbers.
        /// </summary>
        /// <param name="romName">Name of the character Rom, required to properly print culture-specific characters in the small text display</param>
        /// <param name="factory">Encoding factory or null</param>
        public void InitForRom(string romName, LcdCharacterEncodingFactory? factory = null)
        {
            if (factory == null)
            {
                factory = new LcdCharacterEncodingFactory();
            }

            lock (_lock)
            {
                // Create the default encoding for the current ROM and culture, but leave the custom characters away.
                var encoding = factory.Create(_culture, romName, ' ', 0);
                Dictionary<byte, byte[]> specialGraphicsRequired = new Dictionary<byte, byte[]>();
                CreateSpecialChars(specialGraphicsRequired);
                for (byte i = 0; i < specialGraphicsRequired.Count; i++)
                {
                    _lcd.CreateCustomCharacter(i, specialGraphicsRequired[i]);
                }

                _currentSeparationChar = ' '; // To make sure the next function doesn't just skip the initialization
                LoadSeparationChar(_currentSeparationChar);
                _encoding = encoding;
                _lcd.BlinkingCursorVisible = false;
                _lcd.UnderlineCursorVisible = false;
                _lcd.BacklightOn = true;
                _lcd.DisplayOn = true;
                _lcd.Clear();
                ReadCharacterMap();
            }
        }

        /// <summary>
        /// Stop showing big characters.
        /// This method can be used to signal that the display will be used for different purposes. Before a Display method is used,
        /// <see cref="InitForRom"/> needs to be called again. This clears the display.
        /// </summary>
        public void StopShowing()
        {
            lock (_lock)
            {
                _lcd.Clear();
                _font.Clear();
                _encoding = null;
            }
        }

        private void ReadCharacterMap()
        {
            _font.Clear();
            var assembly = Assembly.GetExecutingAssembly();
            // Get our resource file (independent of default namespace of project)
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("BigFontMap.txt"));

            string mapFile;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    mapFile = reader.ReadToEnd();
                }
            }

            // -1: init
            // 0: char seen, awaiting first line
            // 1.. 3 next line
            int characterStep = -1;
            char currentChar = ' ';
            byte[] currentCharMap = new byte[0];
            int currentCharMapPos = 0;
            // Parse the character map file (syntax see there)
            using (StringReader r = new StringReader(mapFile))
            {
                string? line;
                int lineNo = 0;
                while ((line = r.ReadLine()) != null)
                {
                    lineNo++;
                    if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (characterStep == -1)
                    {
                        line = line.TrimEnd();
                        if (line.Length != 2 || line[1] != ':')
                        {
                            throw new InvalidDataException($"Line {lineNo}: Expected character followed by ':', found {line}");
                        }

                        currentChar = line[0];
                        characterStep = 0;
                        continue;
                    }

                    string[] splits = line.Split(new char[] { ',' }, StringSplitOptions.None);
                    if (characterStep == 0)
                    {
                        currentCharMap = new byte[splits.Length * 4];
                        currentCharMapPos = 0;
                    }

                    for (int i = 0; i < splits.Length; i++)
                    {
                        byte b;
                        if (!byte.TryParse(splits[i], out b))
                        {
                            throw new InvalidDataException($"Line {lineNo}: Expected byte, found {splits[i]}");
                        }

                        if (currentCharMapPos >= currentCharMap.Length)
                        {
                            throw new InvalidDataException($"Line {lineNo}: Character is expected to have {currentCharMap.Length} bytes, but found more");
                        }

                        currentCharMap[currentCharMapPos] = b;
                        currentCharMapPos++;
                    }

                    characterStep++;
                    if (characterStep == 4)
                    {
                        _font.Add(currentChar, currentCharMap);
                        characterStep = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Display the current time
        /// </summary>
        /// <param name="dateTime">Time to display</param>
        /// <param name="format">Time format specifier, default "t" (default short time format with hours and minutes and eventually AM/PM).
        /// Anything after the first space in the formatted string is printed as small text. This will for instance be AM/PM when the format specifier "T" is used,
        /// since only 6 chars (and two separators) fit on the display.</param>
        public void DisplayTime(DateTime dateTime, string format = "t")
        {
            string toDisplay = dateTime.ToString(format, _culture);
            string smallText = string.Empty;
            int spaceIdx = toDisplay.IndexOf(' ');
            if (spaceIdx > 0)
            {
                smallText = toDisplay.Substring(spaceIdx + 1);
                toDisplay = toDisplay.Substring(0, spaceIdx);
            }

            StringBuilder[] lines = CreateLinesFromText(toDisplay, smallText, 0, out _);
            UpdateDisplay(lines);
        }

        /// <summary>
        /// Display the given value/unit pair. The value must be pre-formatted with the required number of digits, ie. "2.01".
        /// The value should only contain one of ".", ":" or ",", or the printed result may be unexpected.
        /// </summary>
        /// <param name="formattedValue">Pre-formatted value to print</param>
        /// <param name="unitText">Unit or name of value. This is printed in normal small font on the bottom right corner of the display. </param>
        public void DisplayValue(string formattedValue, string unitText = "")
        {
            var lines = CreateLinesFromText(formattedValue, unitText, 0, out _);
            UpdateDisplay(lines);
        }

        /// <summary>
        /// Scrolls a text in big font trough the display
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="scrollSpeed">Speed between scroll steps (one step being one display cell width)</param>
        /// <param name="cancellationToken">Token for cancelling the operation</param>
        /// <returns>A task handle</returns>
        public Task DisplayBigTextAsync(string text, TimeSpan scrollSpeed, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                int startPosition = 0;
                int totalColumnsUsed;
                CreateLinesFromText(text, string.Empty, 0, out totalColumnsUsed);
                if (totalColumnsUsed == 0)
                {
                    return;
                }

                while (startPosition < totalColumnsUsed)
                {
                    var displayContent = CreateLinesFromText(text, string.Empty, -startPosition, out _);
                    UpdateDisplay(displayContent);
                    if (WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle }, (int)scrollSpeed.TotalMilliseconds) != WaitHandle.WaitTimeout)
                    {
                        break;
                    }

                    startPosition++;
                }
            });
        }

        private StringBuilder[] CreateLinesFromText(string bigText, string smallText, int startPosition, out int totalColumnsUsed)
        {
            if (_encoding == null)
            {
                throw new InvalidOperationException("Font and encoding not loaded. Use InitForRom first.");
            }

            int xPosition = startPosition; // The current x position during drawing. We draw 4-chars high letters, but with variable width
            int totalColumnsUsedInternal = 0;
            StringBuilder[] ret = new StringBuilder[_lcd.Size.Height];
            // Insert a 2-column char
            void Insert(int columns, params byte[] ci)
            {
                int nextByte = 0;
                for (int row = 0; row < 4; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        // Otherwise, there's no room for this character column
                        int realColumn = xPosition + column;
                        if (realColumn < _lcd.Size.Width && realColumn >= 0)
                        {
                            byte b = ci[nextByte];
                            // We use 9 as space, makes the formatting of the character table a bit more consistent, as everything is 1 digit only
                            if (b == 9)
                            {
                                b = 32;
                            }

                            ret[row][realColumn] = (char)b;
                        }

                        nextByte++;
                    }
                }

                xPosition += columns;
                totalColumnsUsedInternal += columns;
            }

            for (int i = 0; i < _lcd.Size.Height; i++)
            {
                ret[i] = new StringBuilder();
                ret[i].Append(new string(' ', _lcd.Size.Width));
            }

            // Only one of these can be printed simultaneously
            if (bigText.Contains(':'))
            {
                LoadSeparationChar(':');
            }
            else if (bigText.Contains('.'))
            {
                LoadSeparationChar('.');
            }
            else if (bigText.Contains(','))
            {
                LoadSeparationChar(',');
            }

            foreach (var c in bigText)
            {
                if (_font.TryGetValue(c, out byte[]? value))
                {
                    Insert(value.Length / 4, value);
                }
            }

            // Right allign the small text (i.e. an unit)
            // It will eventually overwrite the last row of the rightmost digits, but that presumably is still readable.
            int unitPosition = _lcd.Size.Width - smallText.Length;
            xPosition = Math.Max(unitPosition, 0); // Safety check, if unit is longer than display width
            var encodedSmallText = _encoding.GetBytes(smallText);
            for (int i = 0; i < Math.Min(encodedSmallText.Length, _lcd.Size.Width); i++)
            {
                ret[3][xPosition + i] = (char)encodedSmallText[i];
            }

            totalColumnsUsed = totalColumnsUsedInternal;
            return ret;
        }

        private void UpdateDisplay(StringBuilder[] lines)
        {
            lock (_lock)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    _lcd.SetCursorPosition(0, i);
                    // Will again do a character translation, but that shouldn't hurt, as all characters in the input strings should be printable now.
                    _lcd.Write(lines[i].ToString());
                }
            }
        }

        /// <summary>
        /// Clears the display
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _lcd.Clear();
            }
        }

        private void CreateSpecialChars(Dictionary<byte, byte[]> graphicChars)
        {
            graphicChars.Add(0, new byte[]
            {
                0b00001,
                0b00011,
                0b00011,
                0b00111,
                0b00111,
                0b01111,
                0b01111,
                0b11111
            });
            graphicChars.Add(1, new byte[]
            {
                0b11111,
                0b01111,
                0b01111,
                0b00111,
                0b00111,
                0b00011,
                0b00011,
                0b00001,
            });
            graphicChars.Add(2, new byte[]
            {
                0b10000,
                0b11000,
                0b11000,
                0b11100,
                0b11100,
                0b11110,
                0b11110,
                0b11111,
            });
            graphicChars.Add(3, new byte[]
            {
                0b11111,
                0b11110,
                0b11110,
                0b11100,
                0b11100,
                0b11000,
                0b11000,
                0b10000,
            });
            graphicChars.Add(4, new byte[]
            {
                0b00000,
                0b00000,
                0b00000,
                0b00000,
                0b11111,
                0b11111,
                0b11111,
                0b11111
            });
            graphicChars.Add(5, new byte[]
            {
                0b11111,
                0b11111,
                0b11111,
                0b11111,
                0b00000,
                0b00000,
                0b00000,
                0b00000,
            });
            graphicChars.Add(6, new byte[]
            {
                0b11111,
                0b11111,
                0b11111,
                0b11111,
                0b11111,
                0b11111,
                0b11111,
                0b11111,
            });
        }

        /// <summary>
        /// Character code 7 is always used for the separation char, which is one of ":", "." or ",".
        /// </summary>
        /// <param name="separationChar">Separation character</param>
        private void LoadSeparationChar(char separationChar)
        {
            if (separationChar == _currentSeparationChar)
            {
                return;
            }

            lock (_lock)
            {
                switch (separationChar)
                {
                    case ':':
                        _lcd.CreateCustomCharacter(7,
                            new byte[] { 0b00000, 0b00000, 0b01110, 0b01110, 0b01110, 0b00000, 0b00000, 0b00000 });
                        break;
                    case '.':
                        _lcd.CreateCustomCharacter(7,
                            new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b01110, 0b01110, 0b01110 });
                        break;
                    case ',':
                        _lcd.CreateCustomCharacter(7,
                            new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b01110, 0b01110, 0b01100, 0b01000 });
                        break;
                    default:
                        throw new NotImplementedException("Unknown separation char: " + separationChar);
                }

                _currentSeparationChar = separationChar;
            }
        }

    }
}
