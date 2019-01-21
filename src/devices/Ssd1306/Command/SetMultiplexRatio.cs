// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetMultiplexRatio : ICommand
    {
        /// <summary>
        /// This command switches the default 63 multiplex mode to any multiplex ratio, ranging from 16 to 63.
        /// The output pads COM0-COM63 will be switched to the corresponding COM signal.
        /// </summary>
        /// <param name="multiplexRatio">Multiplex ratio with a range of 15-63.</param>
        public SetMultiplexRatio(byte multiplexRatio = 0x63)
        {
            if (multiplexRatio < 0x0F || multiplexRatio > 0x3F)
            {
                throw new ArgumentException("The multiplex ratio is invalid.", nameof(multiplexRatio));
            }

            MultiplexRatio = multiplexRatio;
        }

        public byte Value => 0xA8;

        /// <summary>
        /// Multiplex ratio with a range of 15-63.
        /// </summary>
        public byte MultiplexRatio { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, MultiplexRatio };
        }
    }
}
