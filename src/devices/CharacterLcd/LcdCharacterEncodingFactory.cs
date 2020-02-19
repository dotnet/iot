// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Factory for creating Encodings that support different cultures on different LCD Displays.
    /// </summary>
    public class LcdCharacterEncodingFactory
    {
        private static readonly Dictionary<char, byte> DefaultA00Map;
        private static readonly Dictionary<char, byte> DefaultA02Map;
        private static readonly Dictionary<char, byte> DefaultCustomMap;

        static LcdCharacterEncodingFactory()
        {
            // Default map which is used for unknown ROMs
            DefaultCustomMap = new Dictionary<char, byte>();
            // Inserts ASCII characters ' ' to 'z', which are common to most known character sets
            for (char c = ' '; c <= 'z'; c++)
            {
                DefaultCustomMap.Add(c, (byte)c);
            }

            // The character map A00 contains the most used european letters, some greek math symbols plus japanese letters.
            // Compare with the HD44780 specification sheet, page 17
            DefaultA00Map = new Dictionary<char, byte>();
            // Inserts ASCII characters ' ' to 'z', which are common to most known character sets
            for (char c = ' '; c <= 'z'; c++)
            {
                DefaultA00Map.Add(c, (byte)c);
            }

            DefaultA00Map.Remove('\\'); // Instead of the backspace, the Yen letter is in the map, but we can use char 164 instead
            DefaultA00Map.Add('\\', 164);
            DefaultA00Map.Add('¥', 92);
            DefaultA00Map.Add('{', 123);
            DefaultA00Map.Add('|', 124);
            DefaultA00Map.Add('}', 125);
            DefaultA00Map.Add('\u2192', 126); // right arrow
            DefaultA00Map.Add('\u2190', 127); // left arrow
            // Note: Several letters may point to the same character code
            DefaultA00Map.Add('–', 0b1011_0000);
            DefaultA00Map.Add('α', 224);
            DefaultA00Map.Add('ä', 225);
            DefaultA00Map.Add('β', 226);
            DefaultA00Map.Add('ε', 227);
            DefaultA00Map.Add('μ', 228);
            DefaultA00Map.Add('δ', 229);
            DefaultA00Map.Add('ρ', 230);
            // Character 231 looks like a small q, but what should it be?
            DefaultA00Map.Add('√', 232);
            // What is the match for chars 233, 234 and 235?
            DefaultA00Map.Add('¢', 236);
            DefaultA00Map.Add('ñ', 238);
            DefaultA00Map.Add('ö', 239);

            // 240 and 241 look like p and q again. What are they?
            DefaultA00Map.Add('θ', 242);
            DefaultA00Map.Add('∞', 243);
            DefaultA00Map.Add('Ω', 244);
            DefaultA00Map.Add('Ω', 244);
            DefaultA00Map.Add('ü', 245);
            DefaultA00Map.Add('∑', 246);
            DefaultA00Map.Add('π', 247);
            // Some unrecognized characters here as well
            DefaultA00Map.Add('÷', 253);
            DefaultA00Map.Add('×', (byte)'x');
            DefaultA00Map.Add('█', 255);

            // Now the japanese characters in the A00 rom map.
            // They use the same order than described in https://de.wikipedia.org/wiki/Japanische_Schrift#F%C3%BCnfzig-Laute-Tafel Table "Katakana", so the mapping sounds reasonable.

            // Small letters
            DefaultA00Map.Add('ヽ', 0b1010_0100);
            DefaultA00Map.Add('・', 0b1010_0101);
            DefaultA00Map.Add('ァ', 0b1010_0111);
            DefaultA00Map.Add('ィ', 0b1010_1000);
            DefaultA00Map.Add('ゥ', 0b1010_1001);
            DefaultA00Map.Add('ェ', 0b1010_1010);
            DefaultA00Map.Add('ォ', 0b1010_1011);
            DefaultA00Map.Add('ャ', 0b1010_1100);
            DefaultA00Map.Add('ュ', 0b1010_1101);
            DefaultA00Map.Add('ョ', 0b1010_1110);
            DefaultA00Map.Add('ヮ', 0b1010_1111); // Not sure on this one
            // Normal letters
            DefaultA00Map.Add('ー', 0b1011_0000);
            DefaultA00Map.Add('ア', 0b1011_0001);
            DefaultA00Map.Add('イ', 0b1011_0010);
            DefaultA00Map.Add('ウ', 0b1011_0011);
            DefaultA00Map.Add('エ', 0b1011_0100);
            DefaultA00Map.Add('オ', 0b1011_0101);
            DefaultA00Map.Add('カ', 0b1011_0110);
            DefaultA00Map.Add('キ', 0b1011_0111);
            DefaultA00Map.Add('ク', 0b1011_1000);
            DefaultA00Map.Add('ケ', 0b1011_1001);
            DefaultA00Map.Add('コ', 0b1011_1010);
            DefaultA00Map.Add('サ', 0b1011_1011);
            DefaultA00Map.Add('シ', 0b1011_1100);
            DefaultA00Map.Add('ス', 0b1011_1101);
            DefaultA00Map.Add('セ', 0b1011_1110);
            DefaultA00Map.Add('ソ', 0b1011_1111);
            DefaultA00Map.Add('タ', 0b1100_0000);
            DefaultA00Map.Add('チ', 0b1100_0001);
            DefaultA00Map.Add('ツ', 0b1100_0010);
            DefaultA00Map.Add('テ', 0b1100_0011);
            DefaultA00Map.Add('ト', 0b1100_0100);
            DefaultA00Map.Add('ナ', 0b1100_0101);
            DefaultA00Map.Add('ニ', 0b1100_0110);
            DefaultA00Map.Add('ヌ', 0b1100_0111);
            DefaultA00Map.Add('ネ', 0b1100_1000);
            DefaultA00Map.Add('ノ', 0b1100_1001);
            DefaultA00Map.Add('ハ', 0b1100_1010);
            DefaultA00Map.Add('ヒ', 0b1100_1011);
            DefaultA00Map.Add('フ', 0b1100_1100);
            DefaultA00Map.Add('ヘ', 0b1100_1101);
            DefaultA00Map.Add('ホ', 0b1100_1110);
            DefaultA00Map.Add('マ', 0b1100_1111);
            DefaultA00Map.Add('ミ', 0b1101_0000);
            DefaultA00Map.Add('ム', 0b1101_0001);
            DefaultA00Map.Add('メ', 0b1101_0010);
            DefaultA00Map.Add('モ', 0b1101_0011);
            DefaultA00Map.Add('ヤ', 0b1101_0100);
            DefaultA00Map.Add('ユ', 0b1101_0101);
            DefaultA00Map.Add('ヨ', 0b1101_0110);
            DefaultA00Map.Add('ラ', 0b1101_0111);
            DefaultA00Map.Add('リ', 0b1101_1000);
            DefaultA00Map.Add('ル', 0b1101_1001);
            DefaultA00Map.Add('レ', 0b1101_1010);
            DefaultA00Map.Add('ロ', 0b1101_1011);
            DefaultA00Map.Add('ワ', 0b1101_1100);
            DefaultA00Map.Add('ン', 0b1101_1101); // Characters ヰ and ヱ seem not to exist, they are not used in current japanese and can be replaced by イ and エ
            DefaultA00Map.Add('ヰ', 0b1011_0010);
            DefaultA00Map.Add('ヱ', 0b1011_0100);
            DefaultA00Map.Add('ヲ', 0b1010_0110); // This one is out of sequence
            DefaultA00Map.Add('゛', 0b1101_1110);
            DefaultA00Map.Add('゜', 0b1101_1111);

            DefaultA02Map = new Dictionary<char, byte>();
            // Inserts ASCII characters ' ' to 'z', which are common to most known character sets
            for (char c = ' '; c <= 'z'; c++)
            {
                DefaultA02Map.Add(c, (byte)c);
            }

            // This map contains wide arrow characters. They could be helpful for menus, but not sure where to map them.
            DefaultA02Map.Add('{', 123);
            DefaultA02Map.Add('|', 124);
            DefaultA02Map.Add('}', 125);
            DefaultA02Map.Add('~', 126);
            DefaultA02Map.Add('→', 0b0001_1010); // right arrow
            DefaultA02Map.Add('←', 0b0001_1011); // left arrow
            DefaultA02Map.Add('↑', 0b0001_1000);
            DefaultA02Map.Add('↓', 0b0001_1001);
            DefaultA02Map.Add('↲', 0b0001_0111);
            DefaultA02Map.Add('≤', 0b0001_1100);
            DefaultA02Map.Add('≥', 0b0001_1101);

            // Cyrillic script, capital letters (russian, slawic languages)
            DefaultA02Map.Add('А', (byte)'A');
            DefaultA02Map.Add('Б', 0b1000_0000);
            DefaultA02Map.Add('В', (byte)'B');
            DefaultA02Map.Add('Г', 0b1001_0010);
            DefaultA02Map.Add('Д', 0b1000_0001);
            DefaultA02Map.Add('Е', (byte)'E');
            DefaultA02Map.Add('Ж', 0b1000_0010);
            DefaultA02Map.Add('З', 0b1000_0011);
            DefaultA02Map.Add('И', 0b1000_0100);
            DefaultA02Map.Add('Й', 0b1000_0101);
            DefaultA02Map.Add('К', (byte)'K');
            DefaultA02Map.Add('Л', 0b1000_0110);
            DefaultA02Map.Add('М', (byte)'M');
            DefaultA02Map.Add('Н', (byte)'H');
            DefaultA02Map.Add('О', (byte)'O');
            DefaultA02Map.Add('П', 0b1000_0111);
            DefaultA02Map.Add('Р', (byte)'P');
            DefaultA02Map.Add('С', (byte)'C');
            DefaultA02Map.Add('Т', (byte)'T');
            DefaultA02Map.Add('У', 0b1000_1000);
            DefaultA02Map.Add('Ф', 0b1111_1000);
            DefaultA02Map.Add('Х', (byte)'X');
            DefaultA02Map.Add('Ц', 0b1000_1001);
            DefaultA02Map.Add('Ч', 0b1000_1010);
            DefaultA02Map.Add('Ш', 0b1000_1011);
            DefaultA02Map.Add('Щ', 0b1000_1100);
            DefaultA02Map.Add('Ъ', 0b1000_1101);
            DefaultA02Map.Add('Ы', 0b1000_1110);
            DefaultA02Map.Add('Ь', (byte)'b');
            DefaultA02Map.Add('Э', 0b1000_1111);
            DefaultA02Map.Add('Ю', 0b1010_1100);
            DefaultA02Map.Add('Я', 0b1010_1101);
            DefaultA02Map.Add('μ', 0b1011_0101);
            DefaultA02Map.Add('¡', 0b1010_0001);
            DefaultA02Map.Add('¿', 0b1011_1111);
            // Cyrillic script, special letters
            DefaultA02Map.Add('Ё', 0b1100_1011);
            // Not available in ROM: ЂЃЄ
            DefaultA02Map.Add('Ѕ', (byte)'S');
            DefaultA02Map.Add('І', (byte)'I');
            DefaultA02Map.Add('Ї', 0b1100_1111);
            DefaultA02Map.Add('Ј', (byte)'J');
            // Not available in ROM: ЉЊЋЌЏ
            DefaultA02Map.Add('Ў', 0b1101_1101);

            DefaultA02Map.Add('–', 0b0010_1101);
            DefaultA02Map.Add('α', 0b1001_0000);
            DefaultA02Map.Add('ε', 0b1001_1110);
            DefaultA02Map.Add('δ', 0b1001_1011);
            DefaultA02Map.Add('σ', 0b1001_0101);

            // 240 and 241 look like p and q again. What are they?
            DefaultA02Map.Add('θ', 0b1001_1001);
            DefaultA02Map.Add('Ω', 0b1001_1010);
            DefaultA02Map.Add('Ω', 0b1001_1010);
            DefaultA02Map.Add('∑', 0b1001_0100);
            DefaultA02Map.Add('π', 0b1001_0011);

            string cyrillicLettersSmall = "абвгдежзийклмнопрстуфхцчшщъыьэюяёђѓєѕіїјљњћќўџ";
            string cyrillicLettersCapital = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯЁЂЃЄЅІЇЈЉЊЋЌЎЏ";

            // Map the small cycrillic letters to their capital equivalents
            for (int i = 0; i < cyrillicLettersSmall.Length; i++)
            {
                char small = cyrillicLettersSmall[i];
                char capital = cyrillicLettersCapital[i];
                byte dataPoint;
                if (DefaultA02Map.TryGetValue(capital, out dataPoint))
                {
                    DefaultA02Map.Add(small, dataPoint);
                }
            }

            // West european diacritics (german, spanish, french, scandinavian languages)
            DefaultA02Map.Add('À', 0b1100_0000);
            DefaultA02Map.Add('Á', 0b1100_0001);
            DefaultA02Map.Add('Â', 0b1100_0010);
            DefaultA02Map.Add('Ã', 0b1100_0011);
            DefaultA02Map.Add('Å', 0b1100_0100);
            DefaultA02Map.Add('Æ', 0b1100_0101);
            DefaultA02Map.Add('Ç', 0b1100_0111);
            DefaultA02Map.Add('È', 0b1100_1000);
            DefaultA02Map.Add('É', 0b1100_1001);
            DefaultA02Map.Add('Ê', 0b1100_1010);
            DefaultA02Map.Add('Ë', 0b1100_1011);
            DefaultA02Map.Add('Ì', 0b1100_1100);
            DefaultA02Map.Add('Í', 0b1100_1101);
            DefaultA02Map.Add('Î', 0b1100_1110);
            DefaultA02Map.Add('Ï', 0b1100_1111);
            DefaultA02Map.Add('Đ', 0b1101_0000);
            DefaultA02Map.Add('Ñ', 0b1101_0001);
            DefaultA02Map.Add('Ò', 0b1101_0010);
            DefaultA02Map.Add('Ó', 0b1101_0011);
            DefaultA02Map.Add('Ô', 0b1101_0100);
            DefaultA02Map.Add('Õ', 0b1101_0101);
            DefaultA02Map.Add('Ö', 0b1101_0110);
            DefaultA02Map.Add('×', 0b1101_0111);
            DefaultA02Map.Add('Ø', 0b1101_1000);
            DefaultA02Map.Add('Ù', 0b1101_1001);
            DefaultA02Map.Add('Ú', 0b1101_1010);
            DefaultA02Map.Add('Û', 0b1101_1011);
            DefaultA02Map.Add('Ü', 0b1101_1100);
            DefaultA02Map.Add('Ý', 0b1101_1101);
            DefaultA02Map.Add('Þ', 0b1101_1110);
            DefaultA02Map.Add('ß', 0b1101_1111);

            DefaultA02Map.Add('à', 0b1110_0000);
            DefaultA02Map.Add('á', 0b1110_0001);
            DefaultA02Map.Add('â', 0b1110_0010);
            DefaultA02Map.Add('ã', 0b1110_0011);
            DefaultA02Map.Add('å', 0b1110_0100);
            DefaultA02Map.Add('æ', 0b1110_0101);
            DefaultA02Map.Add('ç', 0b1110_0111);
            DefaultA02Map.Add('è', 0b1110_1000);
            DefaultA02Map.Add('é', 0b1110_1001);
            DefaultA02Map.Add('ê', 0b1110_1010);
            DefaultA02Map.Add('ë', 0b1110_1011);
            DefaultA02Map.Add('ì', 0b1110_1100);
            DefaultA02Map.Add('í', 0b1110_1101);
            DefaultA02Map.Add('î', 0b1110_1110);
            DefaultA02Map.Add('ï', 0b1110_1111);

            DefaultA02Map.Add('đ', 0b1111_0000);
            DefaultA02Map.Add('ð', 0b1111_0000);
            DefaultA02Map.Add('ñ', 0b1111_0001);
            DefaultA02Map.Add('ò', 0b1111_0010);
            DefaultA02Map.Add('ó', 0b1111_0011);
            DefaultA02Map.Add('ô', 0b1111_0100);
            DefaultA02Map.Add('ö', 0b1111_0101);
            DefaultA02Map.Add('÷', 0b1111_0111);
            DefaultA02Map.Add('ø', 0b1111_1000);
            DefaultA02Map.Add('ù', 0b1111_1001);
            DefaultA02Map.Add('ú', 0b1111_1010);
            DefaultA02Map.Add('û', 0b1111_1011);
            DefaultA02Map.Add('ü', 0b1111_1100);
            DefaultA02Map.Add('ý', 0b1111_1101);
            DefaultA02Map.Add('þ', 0b1111_1110);
            DefaultA02Map.Add('ÿ', 0b1111_1111);

        }

        /// <summary>
        /// Creates the character mapping optimized for the given culture.
        /// Checks whether the characters required for a given culture are available in the installed character map and tries
        /// to add them as user-defined characters, if possible.
        /// </summary>
        /// <param name="culture">Culture for which support is required</param>
        /// <param name="romName">ROM type of attached chip. Supported values: "A00" and "A02"</param>
        /// <param name="unknownLetter">Letter that is printed when an unknown character is encountered. This letter must be part of the
        /// default rom set</param>
        /// <param name="maxNumberOfCustomCharacters">Maximum number of custom characters supported on the hardware. Should be 8 for Hd44780-controlled displays.</param>
        /// <returns>The newly created encoding. Whether the encoding can be loaded to a certain display will be decided later.</returns>
        /// <exception cref="ArgumentException">The character specified as unknownLetter must be part of the mapping.</exception>
        public LcdCharacterEncoding Create(CultureInfo culture, string romName, char unknownLetter, int maxNumberOfCustomCharacters)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            Dictionary<char, byte> newMap;
            // Need to copy the static map, we must not update that
            switch (romName)
            {
                case "A00":
                    newMap = new Dictionary<char, byte>(DefaultA00Map);
                    break;
                case "A02":
                    newMap = new Dictionary<char, byte>(DefaultA02Map);
                    break;
                default:
                    newMap = new Dictionary<char, byte>(DefaultCustomMap);
                    break;
            }

            List<byte[]> extraCharacters = new List<byte[]>();
            bool supported = AssignLettersForCurrentCulture(newMap, culture, romName, extraCharacters, maxNumberOfCustomCharacters);

            if (!newMap.ContainsKey(unknownLetter))
            {
                throw new ArgumentException("The replacement letter is not part of the mapping", nameof(unknownLetter));
            }

            var encoding = new LcdCharacterEncoding(culture.Name, romName, newMap, unknownLetter, extraCharacters);
            encoding.AllCharactersSupported = !supported;
            return encoding;
        }

        /// <summary>
        /// Tries to generate letters important in that culture but missing from the current rom set
        /// </summary>
        private bool AssignLettersForCurrentCulture(Dictionary<char, byte> characterMapping, CultureInfo culture, string romName, List<byte[]> extraCharacters, int maxNumberOfCustomCharacters)
        {
            string specialLetters = SpecialLettersForCulture(culture, characterMapping); // Special letters this language group uses, in order of importance

            byte charPos = 0;
            bool retValue = true;
            foreach (var c in specialLetters)
            {
                if (!characterMapping.ContainsKey(c))
                {
                    // This letter doesn't exist, insert it
                    if (charPos < maxNumberOfCustomCharacters)
                    {
                        var pixelMap = CreateLetter(c, romName);
                        if (pixelMap != null)
                        {
                            extraCharacters.Add(pixelMap);
                            characterMapping.Add(c, charPos);
                            charPos++;
                        }
                        else
                        {
                            retValue = false;
                        }
                    }
                    else
                    {
                        retValue = false;
                    }
                }
            }

            return retValue;
        }

        /// <summary>
        /// Returns the set of special characters required for a given culture/language.
        /// This may include diacritics (ä, ö, ø), currency signs (€) or similar chars.
        /// If any of the returned characters are not found in the ROM map, <see cref="CreateLetter"/> is called to obtain the pixel representation of the given character.
        /// A maximum of 8 extra characters can be added to the ones in the ROM.
        /// </summary>
        /// <param name="culture">Culture to support</param>
        /// <param name="characterMapping">The character map, pre-loaded with the characters from the character ROM. This may be extended by explicitly adding direct mappings
        /// where an alternative is allowed (i.e. mapping capital diacritics to normal capital letters É -> E, when there's not enough room to put É into character RAM.</param>
        /// <returns>A string with the set of special characters for a language, i.e. "äöüß€ÄÖÜ" for German</returns>
        protected virtual string SpecialLettersForCulture(CultureInfo culture, Dictionary<char, byte> characterMapping)
        {
            string mainCultureName = culture.Name;
            int idx = mainCultureName.IndexOf('-');
            if (idx > 0)
            {
                mainCultureName = mainCultureName.Substring(0, idx);
            }

            string specialLetters;
            switch (mainCultureName)
            {
                case "en": // English doesn't use any diacritics, so insert some chars that might be helpful anyway
                    specialLetters = "€£";
                    break;
                case "ja": // Japanese
                           // About all the letters. They're there if we use rom map A00, otherwise this will later fail
                    specialLetters = "イロハニホヘトチリヌルヲワカヨタレソツネナラムウヰノオクヤマケフコエテアサキユメミシヱヒモセス";
                    break;
                case "de":
                    specialLetters = "äöüß€ÄÖÜ£ë";
                    break;
                case "fr":
                    specialLetters = "èéêà€çôù";
                    // If the character map doesn't already contain them, we map these diacritical capital letters to their non-diacritical variants.
                    // That's common in french.
                    characterMapping.TryAdd('É', (byte)'E');
                    characterMapping.TryAdd('È', (byte)'E');
                    characterMapping.TryAdd('Ê', (byte)'E');
                    characterMapping.TryAdd('À', (byte)'A');
                    characterMapping.TryAdd('Ç', (byte)'C');
                    break;
                case "no":
                case "da":
                    specialLetters = "æå€øÆÅØ";
                    break;
                case "sv":
                    specialLetters = "åÅöÖüÜ";
                    break;
                case "es":
                    specialLetters = "ñ€¿¡";
                    break;
                case "uk":
                case "ru":
                    specialLetters = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ"; // cyryllic script used for russian (only works on ROM A02, capital letters only)
                    break;
                default:
                    specialLetters = "€£";
                    break;
            }

            return specialLetters;
        }

        /// <summary>
        /// Creates the given letter for the given ROM type.
        /// Overwrite this only if an alternate ROM is used.
        /// </summary>
        protected virtual byte[] CreateLetter(char character, string romName)
        {
            if (romName == "A00")
            {
                return CreateLetterA00(character);
            }

            if (romName == "A02")
            {
                return CreateLetterA02(character);
            }

            return null;
        }

        /// <summary>
        /// Creates the given letter from a pixel map for Rom Type A00 (7-pixel high letters, bottom row empty)
        /// </summary>
        /// <param name="character">Character to create</param>
        /// <returns>An 8-Byte array of the pixel map for the created letter.</returns>
        /// <remarks>
        /// Currently requires the characters to be hardcoded here. Would be nice if we could generate the pixel maps from an existing font, such as Consolas
        /// </remarks>
        protected virtual byte[] CreateLetterA00(char character)
        {
            switch (character)
            {
                case '€':
                    return CreateCustomCharacter(
                    0b_00111,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_00111,
                    0b_00000);
                case '£':
                    return CreateCustomCharacter(
                    0b_00110,
                    0b_01001,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_01000,
                    0b_11111,
                    0b_00000);

                case 'Ä':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000);

                case 'Ö':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000);

                case 'Ü':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000);

                case 'ß':
                    return CreateCustomCharacter(
                    0b_00000,
                    0b_00110,
                    0b_01001,
                    0b_01110,
                    0b_01001,
                    0b_01001,
                    0b_10110,
                    0b_00000);

                case 'Å':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000);

                case 'Â': // Same as above, cannot really distinguish them in the 7-pixel font (but they would not normally be used by the same languages)
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000);

                case 'æ':
                    return CreateCustomCharacter(
                    0b_00000,
                    0b_00000,
                    0b_11010,
                    0b_00101,
                    0b_01111,
                    0b_10100,
                    0b_01011,
                    0b_00000);

                case 'Æ':
                    return CreateCustomCharacter(
                    0b_00111,
                    0b_00100,
                    0b_01100,
                    0b_10111,
                    0b_11100,
                    0b_10100,
                    0b_10111,
                    0b_00000);

                case 'ø':
                    return CreateCustomCharacter(
                    0b_00000,
                    0b_00000,
                    0b_01110,
                    0b_10011,
                    0b_10101,
                    0b_11001,
                    0b_01110,
                    0b_00000);

                case 'Ø':
                    return CreateCustomCharacter(
                    0b_01110,
                    0b_10011,
                    0b_10011,
                    0b_10101,
                    0b_10101,
                    0b_11001,
                    0b_01110,
                    0b_00000);

                case 'à':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000);

                case 'á':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000);

                case 'â':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000);

                case 'ä':
                    return CreateCustomCharacter(
                     0b_01010,
                     0b_00000,
                     0b_01110,
                     0b_00001,
                     0b_01111,
                     0b_10001,
                     0b_01111,
                     0b_00000);

                case 'å':
                    return CreateCustomCharacter(
                     0b_00100,
                     0b_01010,
                     0b_01110,
                     0b_00001,
                     0b_01111,
                     0b_10001,
                     0b_01111,
                     0b_00000);

                case 'ç':
                    return CreateCustomCharacter(
                     0b_00000,
                     0b_00000,
                     0b_01110,
                     0b_10000,
                     0b_10000,
                     0b_01111,
                     0b_00010,
                     0b_00110);

                case 'é':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000);

                case 'è':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000);

                case 'ê':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000);

                case 'ë':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000);

                case 'ï':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000);

                case 'ì':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000);

                case 'í':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000);

                case 'î':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000);

                case 'ñ':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00101,
                    0b_10000,
                    0b_11110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_00000);

                case 'ö':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000);

                case 'ô':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000);

                case 'ò':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000);

                case 'ó':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000);

                case 'ú':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000);

                case 'ù':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000);

                case 'û':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000);

                case 'ü':
                    return CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000);

                case '¡':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00000,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00000);

                case '¿':
                    return CreateCustomCharacter(
                    0b_00100,
                    0b_00000,
                    0b_00100,
                    0b_01000,
                    0b_10000,
                    0b_10001,
                    0b_01110,
                    0b_00000);

            }

            return null;
        }

        /// <summary>
        /// Creates the given letter from a pixel map for Rom Type A02 (7 or 8 Pixel high letters, bottom row filled)
        /// </summary>
        /// <param name="character">Character to create</param>
        /// <returns>An 8-Byte array of the pixel map for the created letter.</returns>
        /// <remarks>
        /// Currently requires the characters to be hardcoded here. Would be nice if we could generate the pixel maps from an existing font, such as Consolas
        /// </remarks>
        protected virtual byte[] CreateLetterA02(char character)
        {
            // TODO: Create letters for A02 map, but that one is a lot better equipped for european languages, so nothing to do for the currently supported languages
            return null;
        }

        /// <summary>
        /// Combines a set of bytes into a pixel map
        /// </summary>
        /// <example>
        /// Use as follows to create letter 'ü':
        /// <code>
        /// case 'ü':
        ///            return CreateCustomCharacter(
        ///            0b_01010,
        ///            0b_00000,
        ///            0b_10001,
        ///            0b_10001,
        ///            0b_10001,
        ///            0b_10011,
        ///            0b_01101,
        ///            0b_00000);
        ///            </code>
        /// </example>
        protected byte[] CreateCustomCharacter(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7)
        {
            return new byte[] { byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7 };
        }
    }
}
