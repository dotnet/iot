// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetPageStartAddressForPageAddressingModeTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetPageStartAddressForPageAddressingMode setPageStartAddressForPageAddressingMode = new SetPageStartAddressForPageAddressingMode();
            byte[] actualBytes = setPageStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(new byte[] { 0xB0 }, actualBytes);
        }

        [Theory]
        [InlineData(PageAddress.Page0, new byte[] { 0xB0 })]
        [InlineData(PageAddress.Page1, new byte[] { 0xB1 })]
        [InlineData(PageAddress.Page2, new byte[] { 0xB2 })]
        [InlineData(PageAddress.Page3, new byte[] { 0xB3 })]
        [InlineData(PageAddress.Page4, new byte[] { 0xB4 })]
        [InlineData(PageAddress.Page5, new byte[] { 0xB5 })]
        [InlineData(PageAddress.Page6, new byte[] { 0xB6 })]
        [InlineData(PageAddress.Page7, new byte[] { 0xB7 })]
        public void Get_Bytes(PageAddress startAddress, byte[] expectedBytes)
        {
            SetPageStartAddressForPageAddressingMode setPageStartAddressForPageAddressingMode = new SetPageStartAddressForPageAddressingMode(startAddress);
            byte[] actualBytes = setPageStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
