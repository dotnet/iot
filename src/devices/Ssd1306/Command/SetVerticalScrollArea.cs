// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetVerticalScrollArea : ICommand
    {
        /// <summary>
        /// This command consists of 3 consecutive bytes to set up the vertical scroll area.
        /// For the continuous vertical scroll function(command 29/2Ah), the number of rows
        /// that in vertical scrolling can be set smaller or equal to the MUX ratio.
        /// </summary>
        /// <param name="topFixedAreaRows">Top fixed area rows with a range of 0-63.</param>
        /// <param name="scrollAreaRows">Scroll area rows with a range of 0-127.</param>
        public SetVerticalScrollArea(byte topFixedAreaRows = 0x00, byte scrollAreaRows = 0x40)
        {
            if (topFixedAreaRows > 0x3F)
            {
                throw new ArgumentException("The top fixed area rows are invalid.", nameof(topFixedAreaRows));
            }

            if (scrollAreaRows > 0x7F)
            {
                throw new ArgumentException("The scroll area rows are invalid.", nameof(scrollAreaRows));
            }

            TopFixedAreaRows = topFixedAreaRows;
            ScrollAreaRows = scrollAreaRows;
        }

        public byte Value => 0xA3;

        /// <summary>
        /// Top fixed area rows with a range of 0-63.
        /// </summary>
        public byte TopFixedAreaRows { get; }

        /// <summary>
        /// Scroll area rows with a range of 0-127.
        /// </summary>
        public byte ScrollAreaRows { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, TopFixedAreaRows, ScrollAreaRows };
        }
    }
}