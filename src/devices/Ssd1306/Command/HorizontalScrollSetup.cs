// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class HorizontalScrollSetup : ICommand
    {
        public HorizontalScrollSetup(HorizontalScrollType scrollType, PageAddress startPageAddress, FrameFrequencyType frameFrequencyType, PageAddress endPageAddress)
        {
            // TODO: Validate values.  EndPageAddress must be >= StartPageAddress.

            ScrollType = scrollType;
            StartPageAddress = startPageAddress;
            FrameFrequencyType = frameFrequencyType;
            EndPageAddress = endPageAddress;
        }

        public byte Value => (byte)ScrollType;

        public HorizontalScrollType ScrollType { get; }

        public PageAddress StartPageAddress { get; }

        public FrameFrequencyType FrameFrequencyType { get; }

        public PageAddress EndPageAddress { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, 0x00, (byte)StartPageAddress, (byte)FrameFrequencyType, (byte)EndPageAddress, 0x00, 0xFF };
        }

        public enum HorizontalScrollType
        {
            Right = 0x26,
            Left = 0x27
        }
    }
}
