// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetVcomhDeselectLevel : ICommand
    {
        /// <summary>
        /// This command adjusts the VCOMH regulator output. 
        /// </summary>
        /// <param name="level">Vcomh deselect level.</param>
        public SetVcomhDeselectLevel(DeselectLevel level = DeselectLevel.Vcc0_77)
        {
            Level = level;
        }

        public byte Id => 0xDB;

        /// <summary>
        /// Vcomh deselect level.
        /// </summary>
        public DeselectLevel Level { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Id, (byte)Level };
        }

        public enum DeselectLevel
        {
            Vcc0_65 = 0x00,
            Vcc0_77 = 0x20,
            Vcc0_83 = 0x30,
            Vcc1_00 = 0x40 // Not on option in datasheet, but was noted to give maximum brightness.
        }
    }
}
