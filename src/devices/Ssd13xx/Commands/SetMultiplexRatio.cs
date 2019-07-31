// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands
{
    public class SetMultiplexRatio : ISharedCommand
    {
        /// <summary>
        /// This command switches the default 63 multiplex mode to any multiplex ratio, ranging from 15 to 127.
        /// The output pads COM0-COM63 will be switched to the corresponding COM signal.
        /// </summary>
        /// <param name="multiplexRatio">Multiplex ratio with a range of 15-127.</param>
        public SetMultiplexRatio(byte multiplexRatio = 63)
        {
            if (!Ssd13xx.InRange(multiplexRatio, 0x0F, 0x7F))
            {
                throw new ArgumentException("The multiplex ratio is invalid.", nameof(multiplexRatio));
            }

            MultiplexRatio = multiplexRatio;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA8;

        /// <summary>
        /// Multiplex ratio with a range of 15-63.
        /// </summary>
        public byte MultiplexRatio { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, MultiplexRatio };
        }
    }
}
