// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    public class SetVerticalScrollArea : ISsd1306Command
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
                throw new ArgumentOutOfRangeException(nameof(topFixedAreaRows));
            }

            if (scrollAreaRows > 0x7F)
            {
                throw new ArgumentOutOfRangeException(nameof(scrollAreaRows));
            }

            TopFixedAreaRows = topFixedAreaRows;
            ScrollAreaRows = scrollAreaRows;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA3;

        /// <summary>
        /// Top fixed area rows with a range of 0-63.
        /// </summary>
        public byte TopFixedAreaRows { get; }

        /// <summary>
        /// Scroll area rows with a range of 0-127.
        /// </summary>
        public byte ScrollAreaRows { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, TopFixedAreaRows, ScrollAreaRows };
        }
    }
}