// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

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
            switch (level)
            {
                case DeselectLevel.Vcc0_65:
                case DeselectLevel.Vcc0_77:
                case DeselectLevel.Vcc0_83:
                case DeselectLevel.Vcc1_00:
                    break;
                default:
                    throw new ArgumentException("The Vcomh deselect level is invalid.", nameof(level));
            }

            Level = level;
        }

        public byte Value => 0xDB;

        /// <summary>
        /// Vcomh deselect level.
        /// </summary>
        public DeselectLevel Level { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, (byte)Level };
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
