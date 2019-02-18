// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetPageAddressTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetPageAddress setPageAddress = new SetPageAddress();
            byte[] actualBytes = setPageAddress.GetBytes();
            Assert.Equal(new byte[] { 0x22, 0x00, 0x07 }, actualBytes);
        }

        [Theory]
        // StartAddress
        [InlineData(PageAddress.Page0, PageAddress.Page0, new byte[] { 0x22, 0x00, 0x00 })]
        [InlineData(PageAddress.Page1, PageAddress.Page0, new byte[] { 0x22, 0x01, 0x00 })]
        [InlineData(PageAddress.Page2, PageAddress.Page0, new byte[] { 0x22, 0x02, 0x00 })]
        [InlineData(PageAddress.Page3, PageAddress.Page0, new byte[] { 0x22, 0x03, 0x00 })]
        [InlineData(PageAddress.Page4, PageAddress.Page0, new byte[] { 0x22, 0x04, 0x00 })]
        [InlineData(PageAddress.Page5, PageAddress.Page0, new byte[] { 0x22, 0x05, 0x00 })]
        [InlineData(PageAddress.Page6, PageAddress.Page0, new byte[] { 0x22, 0x06, 0x00 })]
        [InlineData(PageAddress.Page7, PageAddress.Page0, new byte[] { 0x22, 0x07, 0x00 })]
        // EndAddress
        [InlineData(PageAddress.Page0, PageAddress.Page1, new byte[] { 0x22, 0x00, 0x01 })]
        [InlineData(PageAddress.Page0, PageAddress.Page2, new byte[] { 0x22, 0x00, 0x02 })]
        [InlineData(PageAddress.Page0, PageAddress.Page3, new byte[] { 0x22, 0x00, 0x03 })]
        [InlineData(PageAddress.Page0, PageAddress.Page4, new byte[] { 0x22, 0x00, 0x04 })]
        [InlineData(PageAddress.Page0, PageAddress.Page5, new byte[] { 0x22, 0x00, 0x05 })]
        [InlineData(PageAddress.Page0, PageAddress.Page6, new byte[] { 0x22, 0x00, 0x06 })]
        [InlineData(PageAddress.Page0, PageAddress.Page7, new byte[] { 0x22, 0x00, 0x07 })]
        public void Get_Bytes(PageAddress startAddress, PageAddress endAddress, byte[] expectedBytes)
        {
            SetPageAddress setPageAddress = new SetPageAddress(startAddress, endAddress);
            byte[] actualBytes = setPageAddress.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
