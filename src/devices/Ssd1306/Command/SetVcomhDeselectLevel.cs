// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetVcomhDeselectLevel : ICommand
    {
        public SetVcomhDeselectLevel(DeselectLevel level = DeselectLevel.Vcc0_77)
        {
            // TODO: Validate value.
            // 000b 00h ~0.65 x VCC
            // 010b 20h ~0.77 x VCC(RESET)
            // 011b 30h ~0.83 x VCC

            Level = level;
        }

        public byte Value => 0xDB;

        public DeselectLevel Level { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, (byte)Level };
        }

        public enum DeselectLevel
        {
            Vcc0_65 = 0x00,
            Vcc0_77 = 0x20,
            Vcc0_83 = 0x30
        }
    }
}
