// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetMultiplexRatio : ICommand
    {
        public SetMultiplexRatio(byte multiplexRatio = 0x63)
        {
            // TODO: Validate values.   Ranges from 15 to 63.

            MultiplexRatio = multiplexRatio;
        }

        public byte Value => 0xA8;

        public byte MultiplexRatio { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, MultiplexRatio };
        }
    }
}
