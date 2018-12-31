// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class DeactivateScroll : ICommand
    {
        public byte Value => 0x2E;

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
