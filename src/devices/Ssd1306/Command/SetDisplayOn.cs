// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetDisplayOn : ICommand
    {
        /// <summary>
        /// This command turns the OLED panel display on. 
        /// </summary>
        public SetDisplayOn()
        {
        }

        public byte Value => 0xAF;

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
