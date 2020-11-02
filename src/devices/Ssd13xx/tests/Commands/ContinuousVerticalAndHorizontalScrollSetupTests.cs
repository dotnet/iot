// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;
using static Iot.Device.Ssd13xx.Commands.Ssd1306Commands.ContinuousVerticalAndHorizontalScrollSetup;

namespace Iot.Device.Ssd13xx.Tests
{
    public class ContinuousVerticalAndHorizontalScrollSetupTests
    {
        [Theory]
        // VerticalHorizontalScrollType
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Left, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00 })]
        // StartPageAddress
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page1, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x01, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page2, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x02, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page3, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x03, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page4, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x04, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page5, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x05, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page6, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x06, 0x00, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page7, FrameFrequencyType.Frames5, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x07, 0x00, 0x00, 0x00 })]
        // FrameFrequencyType
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames64, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x01, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames128, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x02, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames256, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x03, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames3, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x04, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames4, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x05, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames25, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x06, 0x00, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames2, PageAddress.Page0, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x07, 0x00, 0x00 })]
        // EndPageAddress
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page1, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x01, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page2, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x02, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page3, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x03, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page4, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x04, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page5, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x05, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page6, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x06, 0x00 })]
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page7, 0x00, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x07, 0x00 })]
        // VerticalScrollingOffset
        [InlineData(VerticalHorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page0, 0x3F, new byte[] { 0x29, 0x00, 0x00, 0x00, 0x00, 0x3F })]
        public void Get_Bytes(
            VerticalHorizontalScrollType scrollType,
            PageAddress startPageAddress,
            FrameFrequencyType frameFrequencyType,
            PageAddress endPageAddress,
            byte verticalScrollingOffset,
            byte[] expectedBytes)
        {
            ContinuousVerticalAndHorizontalScrollSetup continuousVerticalAndHorizontalScrollSetup = new ContinuousVerticalAndHorizontalScrollSetup(
                scrollType,
                startPageAddress,
                frameFrequencyType,
                endPageAddress,
                verticalScrollingOffset);
            byte[] actualBytes = continuousVerticalAndHorizontalScrollSetup.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
