// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents EntireDisplayOn command
    /// </summary>
    public class EntireDisplayOn : ISsd1306Command
    {
        /// <summary>
        /// This command turns the entire display on or off.
        /// </summary>
        /// <param name="entireDisplay">Resume to RAM content display when FALSE and turns entire dislay on when TRUE.</param>
        public EntireDisplayOn(bool entireDisplay)
        {
            EntireDisplay = entireDisplay;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => (byte)(EntireDisplay ? 0xA5 : 0xA4);

        /// <summary>
        /// Resume to RAM content display when FALSE and turns entire dislay on when TRUE.
        /// </summary>
        public bool EntireDisplay { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}
