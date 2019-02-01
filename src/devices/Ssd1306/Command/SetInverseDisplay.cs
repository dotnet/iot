// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetInverseDisplay : ICommand
    {
        /// <summary>
        /// This command sets the display to be inverse.  Displays a RAM data of 0 indicates an ON pixel.
        /// </summary>
        public SetInverseDisplay()
        {
        }

        public byte Id => 0xA7;

        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}
