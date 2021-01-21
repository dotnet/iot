// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetDisplayStartLine command
    /// </summary>
    public class SetDisplayStartLine : ISsd1327Command
    {
        /// <summary>
        /// This command sets the Display Start Line register to determine starting address of display RAM,
        /// by selecting a value from 0 to 127. With value equal to 0, RAM row 0 is mapped to COM0.
        /// With value equal to 1, RAM row 1 is mapped to COM0 and so on.
        /// </summary>
        /// <param name="displayStartLine">Display start line with a range of 0-63.</param>
        public SetDisplayStartLine(byte displayStartLine = 0x00)
        {
            if (displayStartLine > 0x7F)
            {
                throw new ArgumentOutOfRangeException(nameof(displayStartLine));
            }

            DisplayStartLine = displayStartLine;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA1;

        /// <summary>
        /// Display start line with a range of 0-127.
        /// </summary>
        public byte DisplayStartLine { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, DisplayStartLine };
        }
    }
}
