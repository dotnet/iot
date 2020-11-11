// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;

#pragma warning disable SA1011

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Factory for creating Encodings that support different cultures on different LCD Displays.
    /// </summary>
    public class LcdCharacterEncodingFactory
    {
        // Default maps for the HD44780 controller
        private static readonly Dictionary<char, byte> DefaultA00Map;
        private static readonly Dictionary<char, byte> DefaultA02Map;
        // This map is used on SPLC780 controllers, it seems. They're otherwise compatible to the HD44780.
        private static readonly Dictionary<char, byte> DefaultSplC780Map;
        private static readonly Dictionary<char, byte> DefaultCustomMap;

        static LcdCharacterEncodingFactory()
        {
            // Default map which is used for unknown ROMs
            DefaultCustomMap = new Dictionary<char, byte>();
            // The character map A00 contains the most used european letters, some greek math symbols plus japanese letters.
            // Compare with the HD44780 specification sheet, page 17
            DefaultA00Map = new Dictionary<char, byte>()
            {
                // Now the japanese characters in the A00 rom map.
                // They use the same order than described in https://de.wikipedia.org/wiki/Japanische_Schrift#F%C3%BCnfzig-Laute-Tafel Table "Katakana", so the mapping sounds reasonable.
                // Small letters
                { 'ヽ', 0b1010_0100 },
                { '・', 0b1010_0101 },
                { 'ァ', 0b1010_0111 },
                { 'ィ', 0b1010_1000 },
                { 'ゥ', 0b1010_1001 },
                { 'ェ', 0b1010_1010 },
                { 'ォ', 0b1010_1011 },
                { 'ャ', 0b1010_1100 },
                { 'ュ', 0b1010_1101 },
                { 'ョ', 0b1010_1110 },
                { 'ヮ', 0b1010_1111 }, // Not sure on this one
                // Normal letters
                { 'ー', 0b1011_0000 },
                { 'ア', 0b1011_0001 },
                { 'イ', 0b1011_0010 },
                { 'ウ', 0b1011_0011 },
                { 'エ', 0b1011_0100 },
                { 'オ', 0b1011_0101 },
                { 'カ', 0b1011_0110 },
                { 'キ', 0b1011_0111 },
                { 'ク', 0b1011_1000 },
                { 'ケ', 0b1011_1001 },
                { 'コ', 0b1011_1010 },
                { 'サ', 0b1011_1011 },
                { 'シ', 0b1011_1100 },
                { 'ス', 0b1011_1101 },
                { 'セ', 0b1011_1110 },
                { 'ソ', 0b1011_1111 },
                { 'タ', 0b1100_0000 },
                { 'チ', 0b1100_0001 },
                { 'ツ', 0b1100_0010 },
                { 'テ', 0b1100_0011 },
                { 'ト', 0b1100_0100 },
                { 'ナ', 0b1100_0101 },
                { 'ニ', 0b1100_0110 },
                { 'ヌ', 0b1100_0111 },
                { 'ネ', 0b1100_1000 },
                { 'ノ', 0b1100_1001 },
                { 'ハ', 0b1100_1010 },
                { 'ヒ', 0b1100_1011 },
                { 'フ', 0b1100_1100 },
                { 'ヘ', 0b1100_1101 },
                { 'ホ', 0b1100_1110 },
                { 'マ', 0b1100_1111 },
                { 'ミ', 0b1101_0000 },
                { 'ム', 0b1101_0001 },
                { 'メ', 0b1101_0010 },
                { 'モ', 0b1101_0011 },
                { 'ヤ', 0b1101_0100 },
                { 'ユ', 0b1101_0101 },
                { 'ヨ', 0b1101_0110 },
                { 'ラ', 0b1101_0111 },
                { 'リ', 0b1101_1000 },
                { 'ル', 0b1101_1001 },
                { 'レ', 0b1101_1010 },
                { 'ロ', 0b1101_1011 },
                { 'ワ', 0b1101_1100 },
                { 'ン', 0b1101_1101 }, // Characters ヰ and ヱ seem not to exist, they are not used in current japanese and can be replaced by イ and エ
                { 'ヰ', 0b1011_0010 },
                { 'ヱ', 0b1011_0100 },
                { 'ヲ', 0b1010_0110 }, // This one is out of sequence
                { '゛', 0b1101_1110 },
                { '゜', 0b1101_1111 },
                // break in character type
                { '¥', 92 },
                { '{', 123 },
                { '|', 124 },
                { '}', 125 },
                { '\u2192', 126 }, // right arrow
                { '\u2190', 127 }, // left arrow
                // Note: Several letters may point to the same character code
                { '–', 0b1011_0000 },
                { 'α', 224 },
                { 'ä', 225 },
                { 'β', 226 },
                { 'ε', 227 },
                { 'μ', 228 },
                { 'δ', 229 },
                { 'ρ', 230 },
                // Character 231 looks like a small q, but what should it be?
                { '√', 232 },
                // What is the match for chars 233, 234 and 235?
                { '¢', 236 },
                { 'ñ', 238 },
                { 'ö', 239 },
                // 240 and 241 look like p and q again. What are they?
                { 'θ', 242 },
                { '∞', 243 },
                { 'Ω', 244 },
                { 'Ω', 244 },
                { 'ü', 245 },
                { '∑', 246 },
                { 'π', 247 },
                // Some unrecognized characters here as well
                { '÷', 253 },
                { '×', (byte)'x' },
                { '█', 255 },
                { '°', 0b1101_1111 },
            };

            // Character map A02 contains about all characters used in western european languages, a few greek math symbols and some symbols.
            // Compare with the HD44780 specification sheet, page 18.
            // Note especially that this character map really uses the 8 pixel height of the characters, while the A00 map leaves the lowest
            // pixel row usually empty for the cursor. The added space at the top of the character helps by better supporting diacritical symbols.
            DefaultA02Map = new Dictionary<char, byte>()
            {
                // This map contains wide arrow characters. They could be helpful for menus, but not sure where to map them.
                { '{', 123 },
                { '|', 124 },
                { '}', 125 },
                { '~', 126 },
                { '→', 0b0001_1010 }, // right arrow
                { '←', 0b0001_1011 }, // left arrow
                { '↑', 0b0001_1000 },
                { '↓', 0b0001_1001 },
                { '↲', 0b0001_0111 },
                { '≤', 0b0001_1100 },
                { '≥', 0b0001_1101 },
                { '°', 0b1011_0000 },
                // Cyrillic script, capital letters (russian, slawic languages)
                { 'А', (byte)'A' },
                { 'Б', 0b1000_0000 },
                { 'В', (byte)'B' },
                { 'Г', 0b1001_0010 },
                { 'Д', 0b1000_0001 },
                { 'Е', (byte)'E' },
                { 'Ж', 0b1000_0010 },
                { 'З', 0b1000_0011 },
                { 'И', 0b1000_0100 },
                { 'Й', 0b1000_0101 },
                { 'К', (byte)'K' },
                { 'Л', 0b1000_0110 },
                { 'М', (byte)'M' },
                { 'Н', (byte)'H' },
                { 'О', (byte)'O' },
                { 'П', 0b1000_0111 },
                { 'Р', (byte)'P' },
                { 'С', (byte)'C' },
                { 'Т', (byte)'T' },
                { 'У', 0b1000_1000 },
                { 'Ф', 0b1111_1000 },
                { 'Х', (byte)'X' },
                { 'Ц', 0b1000_1001 },
                { 'Ч', 0b1000_1010 },
                { 'Ш', 0b1000_1011 },
                { 'Щ', 0b1000_1100 },
                { 'Ъ', 0b1000_1101 },
                { 'Ы', 0b1000_1110 },
                { 'Ь', (byte)'b' },
                { 'Э', 0b1000_1111 },
                { 'Ю', 0b1010_1100 },
                { 'Я', 0b1010_1101 },
                { 'μ', 0b1011_0101 },
                { '¡', 0b1010_0001 },
                { '¿', 0b1011_1111 },
                // Cyrillic script, special letters
                { 'Ё', 0b1100_1011 },
                // Not available in ROM: ЂЃЄ
                { 'Ѕ', (byte)'S' },
                { 'І', (byte)'I' },
                { 'Ї', 0b1100_1111 },
                { 'Ј', (byte)'J' },
                // Not available in ROM: ЉЊЋЌЏ
                { 'Ў', 0b1101_1101 },
                { '–', 0b0010_1101 },
                { 'α', 0b1001_0000 },
                { 'ε', 0b1001_1110 },
                { 'δ', 0b1001_1011 },
                { 'σ', 0b1001_0101 },
                // 240 and 241 look like p and q again. What are they?
                { 'θ', 0b1001_1001 },
                { 'Ω', 0b1001_1010 },
                { 'Ω', 0b1001_1010 },
                { '∑', 0b1001_0100 },
                { 'π', 0b1001_0011 },
                // West european diacritics (german, spanish, french, scandinavian languages)
                { 'À', 0b1100_0000 },
                { 'Á', 0b1100_0001 },
                { 'Â', 0b1100_0010 },
                { 'Ã', 0b1100_0011 },
                { 'Å', 0b1100_0100 },
                { 'Æ', 0b1100_0101 },
                { 'Ç', 0b1100_0111 },
                { 'È', 0b1100_1000 },
                { 'É', 0b1100_1001 },
                { 'Ê', 0b1100_1010 },
                { 'Ë', 0b1100_1011 },
                { 'Ì', 0b1100_1100 },
                { 'Í', 0b1100_1101 },
                { 'Î', 0b1100_1110 },
                { 'Ï', 0b1100_1111 },
                { 'Đ', 0b1101_0000 },
                { 'Ñ', 0b1101_0001 },
                { 'Ò', 0b1101_0010 },
                { 'Ó', 0b1101_0011 },
                { 'Ô', 0b1101_0100 },
                { 'Õ', 0b1101_0101 },
                { 'Ö', 0b1101_0110 },
                { '×', 0b1101_0111 },
                { 'Ø', 0b1101_1000 },
                { 'Ù', 0b1101_1001 },
                { 'Ú', 0b1101_1010 },
                { 'Û', 0b1101_1011 },
                { 'Ü', 0b1101_1100 },
                { 'Ý', 0b1101_1101 },
                { 'Þ', 0b1101_1110 },
                { 'ß', 0b1101_1111 },
                // break in characters
                { 'à', 0b1110_0000 },
                { 'á', 0b1110_0001 },
                { 'â', 0b1110_0010 },
                { 'ã', 0b1110_0011 },
                { 'å', 0b1110_0100 },
                { 'æ', 0b1110_0101 },
                { 'ç', 0b1110_0111 },
                { 'è', 0b1110_1000 },
                { 'é', 0b1110_1001 },
                { 'ê', 0b1110_1010 },
                { 'ë', 0b1110_1011 },
                { 'ì', 0b1110_1100 },
                { 'í', 0b1110_1101 },
                { 'î', 0b1110_1110 },
                { 'ï', 0b1110_1111 },
                // break in characters
                { 'đ', 0b1111_0000 },
                { 'ð', 0b1111_0000 },
                { 'ñ', 0b1111_0001 },
                { 'ò', 0b1111_0010 },
                { 'ó', 0b1111_0011 },
                { 'ô', 0b1111_0100 },
                { 'ö', 0b1111_0101 },
                { '÷', 0b1111_0111 },
                { 'ø', 0b1111_1000 },
                { 'ù', 0b1111_1001 },
                { 'ú', 0b1111_1010 },
                { 'û', 0b1111_1011 },
                { 'ü', 0b1111_1100 },
                { 'ý', 0b1111_1101 },
                { 'þ', 0b1111_1110 },
                { 'ÿ', 0b1111_1111 },
            };

            // This map for instance can be found here: https://www.microchip.com/forums/m977852.aspx
            DefaultSplC780Map = new Dictionary<char, byte>()
            {
                // Map for the SplC780
                { '{', 123 },
                { '|', 124 },
                { '}', 125 },
                { '~', 0126 },
                { 'Ç', 0128 },
                { 'ü', 0129 },
                { 'é', 0130 },
                { 'â', 0131 },
                { 'å', 0131 },
                { 'ä', 0132 },
                { 'à', 0133 },
                { 'ả', 0134 },
                { 'ç', 0135 },
                { 'ê', 0136 },
                { 'ë', 0137 },
                { 'è', 0138 },
                { 'ï', 0139 },
                { 'î', 0140 },
                { 'ì', 0141 },
                { 'Ä', 0142 },
                { 'Å', 0143 },
                { 'φ', 0xCD },
                // Complete the set of capital greek letters for those that look like latin letters (note that these are not identity assignments)
                { 'Α', (byte)'A' },
                { 'Β', (byte)'B' },
                { 'Ε', (byte)'E' },
                { 'Ζ', (byte)'Z' },
                { 'Η', (byte)'H' },
                { 'Ι', (byte)'I' },
                { 'Κ', (byte)'K' },
                { 'Μ', (byte)'M' },
                { 'Ν', (byte)'N' },
                { 'Ο', (byte)'O' },
                { 'Ρ', (byte)'P' },
                { 'Τ', (byte)'T' },
                { 'Υ', (byte)'Y' },
                { 'Χ', (byte)'X' },
            };

            // Inserts ASCII characters ' ' to 'z', which are common to most known character sets
            for (char c = ' '; c <= 'z'; c++)
            {
                DefaultCustomMap.Add(c, (byte)c);
                DefaultA00Map.Add(c, (byte)c);
                DefaultA02Map.Add(c, (byte)c);
                DefaultSplC780Map.Add(c, (byte)c);
            }

            DefaultA00Map.Remove('\\'); // Instead of the backspace, the Yen letter is in the map, but we can use char 164 instead
            DefaultA00Map.Add('\\', 164);

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

            // A bit easier like this...
            string toAdd = "ÉæÆôöòûùÿÖÜñÑ  ¿"
                           + "áíóú₡£¥₧ ¡ÃãÕõØø"
                           + " ¨° ´½¼×÷≤≥«»≠√ "
                           + "              ®©" // Not much useful in this row
                           + "™†§¶Γ ΘΛΞΠΣ ΦΨΩα"
                           + "βγδεζηθικλμνξπρσ"
                           + "τυχψω"; // greek letter small phi may apparently be represented by its capital letter

            byte startIndex = 144;
            foreach (var c in toAdd)
            {
                if (c != ' ')
                {
                    DefaultSplC780Map.Add(c, startIndex);
                }

                startIndex++;
            }
        }

        /// <summary>
        /// Creates the character mapping optimized for the given culture.
        /// Checks whether the characters required for a given culture are available in the installed character map and tries
        /// to add them as user-defined characters, if possible.
        /// </summary>
        /// <param name="culture">Culture for which support is required</param>
        /// <param name="romName">ROM type of attached chip. Supported values: "A00", "A02", "SplC780"</param>
        /// <param name="unknownLetter">Letter that is printed when an unknown character is encountered. This letter must be part of the
        /// default rom set</param>
        /// <param name="maxNumberOfCustomCharacters">Maximum number of custom characters supported on the hardware. Should be 8 for Hd44780-controlled displays.</param>
        /// <returns>The newly created encoding. Whether the encoding can be loaded to a certain display will be decided later.</returns>
        /// <exception cref="ArgumentException">The character specified as unknownLetter must be part of the mapping.</exception>
        public LcdCharacterEncoding Create(CultureInfo culture, string romName, char unknownLetter, int maxNumberOfCustomCharacters)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            // Need to copy the static map, we must not update that
            Dictionary<char, byte> newMap = romName switch
            {
                "A00" => new (DefaultA00Map),
                "A02" => new (DefaultA02Map),
                "SplC780" => new (DefaultSplC780Map),
                _ => new (DefaultCustomMap),
            };

            List<byte[]> extraCharacters = new ();
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
            foreach (var c in specialLetters)
            {
                if (!characterMapping.ContainsKey(c))
                {
                    // This letter doesn't exist, insert it
                    if (charPos < maxNumberOfCustomCharacters)
                    {
                        var pixelMap = CreateLetter(c, romName);
                        if (pixelMap is object)
                        {
                            extraCharacters.Add(pixelMap);
                            characterMapping.Add(c, charPos);
                            charPos++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
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
        protected virtual byte[]? CreateLetter(char character, string romName) => romName switch
        {
            "A00" => CreateLetterA00(character),
            "A02" => CreateLetterA02(character),
            // The font looks identical, so we can use the same lookup table
            "SplC780" => CreateLetterA00(character),
            _ => null,
        };

        /// <summary>
        /// Creates the given letter from a pixel map for Rom Type A00 (7-pixel high letters, bottom row empty)
        /// </summary>
        /// <param name="character">Character to create</param>
        /// <returns>An 8-Byte array of the pixel map for the created letter.</returns>
        /// <remarks>
        /// Currently requires the characters to be hardcoded here. Would be nice if we could generate the pixel maps from an existing font, such as Consolas
        /// </remarks>
        protected virtual byte[]? CreateLetterA00(char character) => character switch
        {
            '€' => CreateCustomCharacter(
                    0b_00111,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_00111,
                    0b_00000),
            '£' => CreateCustomCharacter(
                    0b_00110,
                    0b_01001,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_01000,
                    0b_11111,
                    0b_00000),
            'Ä' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            'Ö' => CreateCustomCharacter(
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'Ü' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ß' => CreateCustomCharacter(
                    0b_00000,
                    0b_00110,
                    0b_01001,
                    0b_01110,
                    0b_01001,
                    0b_01001,
                    0b_10110,
                    0b_00000),
            'Å' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            // Same as above, cannot really distinguish them in the 7-pixel font (but they would not normally be used by the same languages)
            'Â' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            'æ' => CreateCustomCharacter(
                    0b_00000,
                    0b_00000,
                    0b_11010,
                    0b_00101,
                    0b_01111,
                    0b_10100,
                    0b_01011,
                    0b_00000),
            'Æ' => CreateCustomCharacter(
                    0b_00111,
                    0b_00100,
                    0b_01100,
                    0b_10111,
                    0b_11100,
                    0b_10100,
                    0b_10111,
                    0b_00000),
            'ø' => CreateCustomCharacter(
                    0b_00000,
                    0b_00000,
                    0b_01110,
                    0b_10011,
                    0b_10101,
                    0b_11001,
                    0b_01110,
                    0b_00000),
            'Ø' => CreateCustomCharacter(
                    0b_01110,
                    0b_10011,
                    0b_10011,
                    0b_10101,
                    0b_10101,
                    0b_11001,
                    0b_01110,
                    0b_00000),
            'à' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000),
            'á' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000),
            'â' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000),
            'ä' => CreateCustomCharacter(
                     0b_01010,
                     0b_00000,
                     0b_01110,
                     0b_00001,
                     0b_01111,
                     0b_10001,
                     0b_01111,
                     0b_00000),
            'å' => CreateCustomCharacter(
                     0b_00100,
                     0b_01010,
                     0b_01110,
                     0b_00001,
                     0b_01111,
                     0b_10001,
                     0b_01111,
                     0b_00000),
            'ç' => CreateCustomCharacter(
                     0b_00000,
                     0b_00000,
                     0b_01110,
                     0b_10000,
                     0b_10000,
                     0b_01111,
                     0b_00010,
                     0b_00110),
            'é' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'è' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'ê' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'ë' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'ï' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'ì' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'í' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'î' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'ñ' => CreateCustomCharacter(
                    0b_01010,
                    0b_00101,
                    0b_10000,
                    0b_11110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            'ö' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ô' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ò' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ó' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ú' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            'ù' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            'û' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            'ü' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            '¡' => CreateCustomCharacter(
                    0b_00100,
                    0b_00000,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00000),
            '¿' => CreateCustomCharacter(
                    0b_00100,
                    0b_00000,
                    0b_00100,
                    0b_01000,
                    0b_10000,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            _ => throw new Exception("Character encoding not supported"),
        };

        /// <summary>
        /// Creates the given letter from a pixel map for Rom Type A02 (7 or 8 Pixel high letters, bottom row filled)
        /// </summary>
        /// <param name="character">Character to create</param>
        /// <returns>An 8-Byte array of the pixel map for the created letter.</returns>
        /// <remarks>
        /// Currently requires the characters to be hardcoded here. Would be nice if we could generate the pixel maps from an existing font, such as Consolas
        /// </remarks>
        // TODO: Create letters for A02 map, but that one is a lot better equipped for european languages, so nothing to do for the currently supported languages
        protected virtual byte[]? CreateLetterA02(char character) => null;

        /// <summary>
        /// Combines a set of bytes into a pixel map
        /// </summary>
        /// <example>
        /// Use as follows to create letter 'ü':
        /// <code>
        /// CreateCustomCharacter(
        ///            0b_01010,
        ///            0b_00000,
        ///            0b_10001,
        ///            0b_10001,
        ///            0b_10001,
        ///            0b_10011,
        ///            0b_01101,
        ///            0b_00000)
        /// </code>
        /// </example>
        protected byte[] CreateCustomCharacter(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7) =>
            new byte[] { byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7 };
    }
}
