// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    public class SetUnlockDriver : ISsd1327Command
    {

        /// <summary>
        /// This command sets the display to be normal.
        /// </summary>
        /// <param name="unlock">Represents if driver have to be unlocked.</param>
        public SetUnlockDriver(bool unlock)
        {
            SetUnlock = (byte)(unlock ? 0b_0001_0010 : 0b_0001_0110);
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xFD;

        /// <summary>
        /// The value that represents if driver should be unlocked.
        /// </summary>
        byte SetUnlock { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, SetUnlock };
        }
    }
}
