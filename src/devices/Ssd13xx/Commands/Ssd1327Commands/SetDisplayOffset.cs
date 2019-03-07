// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    public class SetDisplayOffset : ISsd1327Command
    {
        /// <summary>
        /// This command specifies the mapping of the display start line to one of COM0-COM127
        /// (assuming that COM0 is the display start line then the display start line register is equal to 0).
        /// </summary>
        /// <param name="displayOffset">Display offset with a range of 0-127.</param>
        public SetDisplayOffset(byte displayOffset = 0x00)
        {
            if (displayOffset > 0x5F)
            {
                throw new ArgumentException("The display offset is invalid.", nameof(displayOffset));
            }

            DisplayOffset = displayOffset;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA2;

        /// <summary>
        /// Display offset with a range of 0-127.
        /// </summary>
        public byte DisplayOffset { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, DisplayOffset };
        }
    }
}
