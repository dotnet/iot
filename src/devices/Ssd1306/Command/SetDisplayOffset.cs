// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetDisplayOffset : ICommand
    {
        /// <summary>
        /// This command specifies the mapping of the display start line to one of COM0-COM63
        /// (assuming that COM0 is the display start line then the display start line register is equal to 0).
        /// </summary>
        /// <param name="displayOffset">Display offset with a range of 0-63.</param>
        public SetDisplayOffset(byte displayOffset = 0x00)
        {
            if (displayOffset > 0x3F)
            {
                throw new ArgumentException("The display offset is invalid.", nameof(displayOffset));
            }

            DisplayOffset = displayOffset;
        }

        public byte Value => 0xD3;

        /// <summary>
        /// Display offset with a range of 0-63.
        /// </summary>
        public byte DisplayOffset { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, DisplayOffset };
        }
    }
}
