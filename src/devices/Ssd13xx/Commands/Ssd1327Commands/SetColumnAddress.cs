// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetColumnAddress command
    /// </summary>
    public class SetColumnAddress : ISsd1327Command
    {
        /// <summary>
        /// Set column address.
        /// Start from 8th column of driver IC. This is 0th column for OLED.
        /// End at  (8 + 47)th column. Each column has 2 pixels(or segments).
        /// </summary>
        /// <param name="startAddress">Column start address with a range of 8-55.</param>
        /// <param name="endAddress">Column end address with a range of 8-55.</param>
        public SetColumnAddress(byte startAddress = 0x08, byte endAddress = 0x37)
        {
            if (startAddress > 0x37)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (endAddress > 0x37)
            {
                throw new ArgumentOutOfRangeException(nameof(endAddress));
            }

            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0x15;

        /// <summary>
        /// Column start address.
        /// </summary>
        public byte StartAddress { get; set; }

        /// <summary>
        /// Column end address.
        /// </summary>
        public byte EndAddress { get; set; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, StartAddress, EndAddress };
        }
    }
}
