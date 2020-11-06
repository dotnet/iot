// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents SetVcomhDeselectLevel command
    /// </summary>
    public class SetVcomhDeselectLevel : ISsd1306Command
    {
        /// <summary>
        /// This command adjusts the VCOMH regulator output.
        /// </summary>
        /// <param name="level">Vcomh deselect level.</param>
        public SetVcomhDeselectLevel(DeselectLevel level = DeselectLevel.Vcc0_77)
        {
            Level = level;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xDB;

        /// <summary>
        /// Vcomh deselect level.
        /// </summary>
        public DeselectLevel Level { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, (byte)Level };
        }

        /// <summary>
        /// Deselect level
        /// </summary>
        public enum DeselectLevel
        {
            /// <summary>
            /// ~0.65 x Vcc.
            /// </summary>
            Vcc0_65 = 0x00,

            /// <summary>
            /// ~0.77 x Vcc.  Default value after reset.
            /// </summary>
            Vcc0_77 = 0x20,

            /// <summary>
            /// ~0.83 x Vcc.
            /// </summary>
            Vcc0_83 = 0x30,

            /// <summary>
            /// ~1.00 x Vcc.
            /// </summary>
            Vcc1_00 = 0x40 // Not on option in datasheet, but was noted to give maximum brightness.
        }
    }
}
