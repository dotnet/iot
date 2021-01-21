// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetComOutputScanDirectionTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetComOutputScanDirection setComOutputScanDirection = new SetComOutputScanDirection();
            byte[] actualBytes = setComOutputScanDirection.GetBytes();
            Assert.Equal(new byte[] { 0xC0 }, actualBytes);
        }

        [Theory]
        [InlineData(false, new byte[] { 0xC8 })]
        [InlineData(true, new byte[] { 0xC0 })]
        public void Get_Bytes(bool normalMode, byte[] expectedBytes)
        {
            SetComOutputScanDirection setComOutputScanDirection = new SetComOutputScanDirection(normalMode);
            byte[] actualBytes = setComOutputScanDirection.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
