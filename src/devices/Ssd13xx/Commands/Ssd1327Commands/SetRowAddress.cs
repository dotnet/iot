// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetRowAddress command
    /// </summary>
    public class SetRowAddress : ISsd1327Command
    {
        /// <summary>
        /// Set row address
        /// </summary>
        /// <param name="startAddress">Column start address with a range of 0-95.</param>
        /// <param name="endAddress">Column end address with a range of 0-95.</param>
        public SetRowAddress(byte startAddress = 0x00, byte endAddress = 0x5f)
        {
            if (startAddress > 0x5f)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (endAddress > 0x5f)
            {
                throw new ArgumentOutOfRangeException(nameof(endAddress));
            }

            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0x75;

        /// <summary>
        /// Row start address.
        /// </summary>
        public byte StartAddress { get; set; }

        /// <summary>
        /// Row end address.
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
