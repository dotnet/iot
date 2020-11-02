// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetChargePumpTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetChargePump setChargePump = new SetChargePump();
            byte[] actualBytes = setChargePump.GetBytes();
            Assert.Equal(new byte[] { 0x8D, 0x10 }, actualBytes);
        }

        [Theory]
        [InlineData(false, new byte[] { 0x8D, 0x10 })]
        [InlineData(true, new byte[] { 0x8D, 0x14 })]
        public void Get_Bytes(bool enableChargePump, byte[] expectedBytes)
        {
            SetChargePump setChargePump = new SetChargePump(enableChargePump);
            byte[] actualBytes = setChargePump.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
