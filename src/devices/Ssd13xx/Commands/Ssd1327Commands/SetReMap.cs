// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetReMap command
    /// </summary>
    public class SetReMap : ISsd1327Command
    {
        /// <summary>
        /// Re-map setting in Graphic Display Data RAM(GDDRAM)
        /// </summary>
        public SetReMap(
            bool columnAddressRemap = false,
            bool nibbleRemap = true,
            bool verticalMode = true,
            bool comRemap = false,
            bool comSplitOddEven = true)
        {
            Config = 0b_0000_0000;
            if (columnAddressRemap)
            {
                Config |= 0b_0000_0001;
            }

            if (nibbleRemap)
            {
                Config |= 0b_0000_0010;
            }

            if (verticalMode)
            {
                Config |= 0b_0000_0100;
            }

            if (comRemap)
            {
                Config |= 0b_0001_0000;
            }

            if (comSplitOddEven)
            {
                Config |= 0b_0100_0000;
            }
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA0;

        /// <summary>
        /// ReMap Config.
        /// </summary>
        public byte Config { get; set; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, Config };
        }
    }
}
