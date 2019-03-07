// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    public class SetUnlockDriver : ISsd1327Command
    {
        const byte Unlock = 0b_0001_0010;

        /// <summary>
        /// This command sets the display to be normal.
        /// </summary>
        public SetUnlockDriver()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xFD;

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, Unlock };
        }
    }
}
