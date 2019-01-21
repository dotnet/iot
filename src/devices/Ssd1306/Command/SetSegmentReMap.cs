// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetSegmentReMap : ICommand
    {
        /// <summary>
        /// This command changes the mapping between the display data column address and the segment driver.
        /// It allows flexibility in OLED module design. This command only affects subsequent data input.
        /// Data already stored in GDDRAM will have no changes.
        /// </summary>
        /// <param name="columnAddress127">Column address 0 is mapped to SEG0 when FALSE.
        /// Column address 127 is mapped to SEG0 when TRUE.</param>
        public SetSegmentReMap(bool columnAddress127 = false)
        {
            ColumnAddress127 = columnAddress127;
        }

        public byte Value => (byte)(ColumnAddress127 ? 0xA1 : 0xA0);

        /// <summary>
        /// Column Address 127.  Column address 127 is mapped to SEG0 when FALSE.
        /// Column address 127 is mapped to SEG0 when TRUE.
        /// </summary>
        public bool ColumnAddress127 { get; set; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
