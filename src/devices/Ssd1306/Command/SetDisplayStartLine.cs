// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetDisplayStartLine : ICommand
    {
        /// <summary>
        /// This command sets the Display Start Line register to determine starting address of display RAM,
        /// by selecting a value from 0 to 63. With value equal to 0, RAM row 0 is mapped to COM0.
        /// With value equal to 1, RAM row 1 is mapped to COM0 and so on.
        /// </summary>
        /// <param name="displayStartLine">Display start line with a range of 0-63.</param>
        public SetDisplayStartLine(byte displayStartLine = 0x00)
        {
            if (displayStartLine > 0x3F)
            {
                throw new ArgumentException("The display start line is invalid.", nameof(displayStartLine));
            }

            DisplayStartLine = displayStartLine;
        }

        public byte Value => (byte)(0x40 + DisplayStartLine);

        /// <summary>
        /// Display start line with a range of 0-63.
        /// </summary>
        public byte DisplayStartLine { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
