// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetSegmentReMapTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetSegmentReMap setSegmentReMap = new SetSegmentReMap();
            byte[] actualBytes = setSegmentReMap.GetBytes();
            Assert.Equal(new byte[] { 0xA0 }, actualBytes);
        }

        [Theory]
        [InlineData(false, new byte[] { 0xA0 })]
        [InlineData(true, new byte[] { 0xA1 })]
        public void Get_Bytes(bool columnAddress127, byte[] expectedBytes)
        {
            SetSegmentReMap setSegmentReMap = new SetSegmentReMap(columnAddress127);
            byte[] actualBytes = setSegmentReMap.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
