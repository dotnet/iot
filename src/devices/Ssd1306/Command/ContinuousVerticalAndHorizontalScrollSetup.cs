// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class ContinuousVerticalAndHorizontalScrollSetup : ICommand
    {
        public ContinuousVerticalAndHorizontalScrollSetup(
            VerticalHorizontalScrollType scrollType,
            PageAddress startPageAddress,
            FrameFrequencyType frameFrequencyType,
            PageAddress endPageAddress,
            byte verticalScrollingOffset)
        {
            // TODO: Validate values.  EndPageAddress must be >= StartPageAddress. VerticalHorizontalScrollType 1 - 63

            ScrollType = scrollType;
            StartPageAddress = startPageAddress;
            FrameFrequencyType = frameFrequencyType;
            EndPageAddress = endPageAddress;
            VerticalScrollingOffset = verticalScrollingOffset;
        }

        public byte Value => (byte)ScrollType;

        public VerticalHorizontalScrollType ScrollType { get; }

        public PageAddress StartPageAddress { get; }

        public FrameFrequencyType FrameFrequencyType { get; }

        public PageAddress EndPageAddress { get; }

        public byte VerticalScrollingOffset { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, 0x00, (byte)StartPageAddress, (byte)FrameFrequencyType, (byte)EndPageAddress, VerticalScrollingOffset };
        }

        public enum VerticalHorizontalScrollType
        {
            Right = 0x29,
            Left = 0x2A
        }
    }
}
