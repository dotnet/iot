// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetComOutputScanDirection : ICommand
    {
        public SetComOutputScanDirection(bool normalMode = true)
        {
            NormalMode = normalMode;
        }

        public byte Value => (byte)(NormalMode ? 0xC0 : 0xC8);

        public bool NormalMode { get; set; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
