// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetSegmentReMap : ICommand
    {
        public SetSegmentReMap(bool columnAddress0 = true)
        {
            ColumnAddress0 = columnAddress0;
        }

        public byte Value => (byte)(ColumnAddress0 ? 0xA0 : 0xA1);

        public bool ColumnAddress0 { get; set; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
