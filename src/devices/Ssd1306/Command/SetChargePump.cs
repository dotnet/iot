// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetChargePump : ICommand
    {
        public SetChargePump(bool enableChargePump = false)
        {
            EnableChargePump = enableChargePump;
        }

        public byte Value => 0x8D;

        public bool EnableChargePump { get; }

        public byte[] GetBytes()
        {
            byte enableChargePump = 0x10;

            if (EnableChargePump)
            {
                enableChargePump = 0x14;
            }

            return new byte[] { Value, enableChargePump };
        }
    }
}
