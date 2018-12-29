// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;
using static Iot.Device.Ssd1306.Command.HorizontalScrollSetup;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class HorizontalScrollSetupTests
    {
        [Theory]
        // HorizontalScrollType
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Left, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x27, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF })]
        // StartPageAddress
        [InlineData(HorizontalScrollType.Right, PageAddress.Page1, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page2, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x02, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page3, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x03, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page4, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x04, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page5, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x05, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page6, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x06, 0x00, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page7, FrameFrequencyType.Frames5, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x07, 0x00, 0x00, 0x00, 0xFF })]
        // FrameFrequencyType
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames64, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x01, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames128, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x02, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames256, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x03, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames3, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x04, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames4, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x05, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames25, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x06, 0x00, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames2, PageAddress.Page0, new byte[] { 0x26, 0x00, 0x00, 0x07, 0x00, 0x00, 0xFF })]
        // EndPageAddress
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page1, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x01, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page2, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x02, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page3, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x03, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page4, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x04, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page5, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x05, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page6, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x06, 0x00, 0xFF })]
        [InlineData(HorizontalScrollType.Right, PageAddress.Page0, FrameFrequencyType.Frames5, PageAddress.Page7, new byte[] { 0x26, 0x00, 0x00, 0x00, 0x07, 0x00, 0xFF })]
        public void Get_Bytes(HorizontalScrollType scrollType, PageAddress startPageAddress, FrameFrequencyType frameFrequencyType, PageAddress endPageAddress, byte[] expectedBytes)
        {
            HorizontalScrollSetup horizontalScrollSetup = new HorizontalScrollSetup(
                scrollType,
                startPageAddress,
                frameFrequencyType,
                endPageAddress);
            byte[] actualBytes = horizontalScrollSetup.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}