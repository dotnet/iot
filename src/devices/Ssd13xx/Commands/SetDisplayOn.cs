// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands
{
    /// <summary>
    /// Represents SetDisplayOn command
    /// </summary>
    public class SetDisplayOn : ISharedCommand
    {
        /// <summary>
        /// This command turns the OLED panel display on.
        /// </summary>
        public SetDisplayOn()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xAF;

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}
