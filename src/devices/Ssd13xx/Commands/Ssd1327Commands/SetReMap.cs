// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    public class SetReMap : ISsd1327Command
    {
        /// <summary>
        /// Re-map setting in Graphic Display Data RAM(GDDRAM) 
        /// </summary>
        public SetReMap(
            bool columnAddressRemap = false,
            bool nibbleRemap = true,
            bool verticalMode = true,
            bool COMRemap = false,
            bool COMSplitOddEven = true)
        {
            config = 0b_0000_0000;
            if (columnAddressRemap) {
                config |= 0b_0000_0001;
            }
            
            if (nibbleRemap) {
                config |= 0b_0000_0010;
            }

            if (verticalMode) {
                config |= 0b_0000_0100;
            }

            if (COMRemap) {
                config |= 0b_0001_0000;
            }

            if (COMSplitOddEven) {
                config |= 0b_0100_0000;
            }
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA0;

        // <summary>
        /// ReMap config.
        /// </summary>
        public byte config { get; set; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, config };
        }
    }
}
