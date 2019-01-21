// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class DeactivateScroll : ICommand
    {
        /// <summary>
        /// This command stops the motion of scrolling. After sending 2Eh command to deactivate
        /// the scrolling action, the ram data needs to be rewritten.
        /// </summary>
        public DeactivateScroll()
        {
        }

        public byte Value => 0x2E;

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
