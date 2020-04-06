// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Interface for character LCD Low-Level handler
    /// </summary>
    public interface ICharacterLcd : IDisposable
    {
        /// <summary>
        /// Enable/disable the backlight. (Will always return false if no backlight pin was provided.)
        /// </summary>
        bool BacklightOn { get; set; }

        /// <summary>
        /// Enable/disable the display.
        /// </summary>
        bool DisplayOn { get; set; }

        /// <summary>
        /// Enable/disable the underline cursor.
        /// </summary>
        bool UnderlineCursorVisible { get; set; }

        /// <summary>
        /// Enable/disable the blinking cursor.
        /// </summary>
        bool BlinkingCursorVisible { get; set; }

        /// <summary>
        /// Returns the size of the display.
        /// </summary>
        System.Drawing.Size Size { get; }

        /// <summary>
        /// Returns the number of custom characters for this display.
        /// A custom character is one that can be user-defined and assigned to a slot using <see cref="CreateCustomCharacter"/>
        /// </summary>
        int NumberOfCustomCharactersSupported { get; }

        /// <summary>
        /// Clears the display and moves the cursor to the top left.
        /// </summary>
        void Clear();

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
        /// <param name="location">Should be between 0 and <see cref="NumberOfCustomCharactersSupported"/>.</param>
        /// <param name="characterMap">Provide an array of 8 bytes containing the pattern</param>
        void CreateCustomCharacter(byte location, ReadOnlySpan<byte> characterMap);

        /// <summary>
        /// Moves the cursor to an explicit column and row position.
        /// </summary>
        /// <param name="left">The column position from left to right starting with 0.</param>
        /// <param name="top">The row position from the top starting with 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">The given position is not inside the display.</exception>
        void SetCursorPosition(int left, int top);

        /// <summary>
        /// Write text to the display, without any character translation.
        /// </summary>
        /// <param name="text">Text to be displayed.</param>
        /// <remarks>
        /// There are only 256 characters available. Different chip variants
        /// have different character sets. Characters from space ' ' (32) to
        /// '}' are usually the same with the exception of '\', which is a
        /// yen symbol ('¥') on some chips.
        /// </remarks>
        void Write(string text);

        /// <summary>
        /// Write a raw byte stream to the display.
        /// Used if character translation already took place.
        /// </summary>
        /// <param name="text">Text to print</param>
        void Write(ReadOnlySpan<byte> text);
    }
}
