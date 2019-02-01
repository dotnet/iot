// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class ActivateScroll : ICommand
    {
        /// <summary>
        /// This command starts the motion of scrolling and should only be issued
        /// after the scroll setup parameters have been defined by the scrolling
        /// setup commands :26h/27h/29h/2Ah. The setting in the last scrolling setup
        /// command overwrites the setting in the previous scrolling setup commands.
        /// </summary>
        public ActivateScroll()
        {
        }

        public byte Id => 0x2F;

        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}
