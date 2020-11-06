using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Extension methods for ICharacterLcd
    /// </summary>
    public static class CharacterLcdExtensions
    {
        /// <summary>
        /// Creates a custom character for standard displays with 8-pixel-per-row characters. See <see cref="ICharacterLcd.CreateCustomCharacter"/> for details.
        /// </summary>
        /// <param name="self">Instance of ICharacterLcd. This method can be called as extension method on this instance</param>
        /// <param name="location">Index of the character to create in the hardware character table</param>
        /// <param name="b0">First row data (standard displays only use the lower 5 bits of each row)</param>
        /// <param name="b1">Second row data</param>
        /// <param name="b2">Third row data</param>
        /// <param name="b3">Fourth row data</param>
        /// <param name="b4">Fifth row data</param>
        /// <param name="b5">Sixth row data</param>
        /// <param name="b6">Seventh row data</param>
        /// <param name="b7">Eight row data</param>
        public static void CreateCustomCharacter(this ICharacterLcd self, byte location, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
        {
            byte[] array = { b0, b1, b2, b3, b4, b5, b6, b7 };
            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(array);
            self.CreateCustomCharacter(location, bytes);
        }
    }
}
