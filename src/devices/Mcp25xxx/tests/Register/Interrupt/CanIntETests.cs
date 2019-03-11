// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.Interrupt
{
    public class CanIntETests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanIntE, new CanIntE(false, false, false, false, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(false, false, false, false, false, false, false, false, 0b0000_0000)]
        [InlineData(true, false, false, false, false, false, false, false, 0b0000_0001)]
        [InlineData(false, true, false, false, false, false, false, false, 0b0000_0010)]
        [InlineData(false, false, true, false, false, false, false, false, 0b0000_0100)]
        [InlineData(false, false, false, true, false, false, false, false, 0b0000_1000)]
        [InlineData(false, false, false, false, true, false, false, false, 0b0001_0000)]
        [InlineData(false, false, false, false, false, true, false, false, 0b0010_0000)]
        [InlineData(false, false, false, false, false, false, true, false, 0b0100_0000)]
        [InlineData(false, false, false, false, false, false, false, true, 0b1000_0000)]
        public void From_To_Byte(bool rx0ie, bool rx1ie, bool tx0ie, bool tx1ie, bool tx2ie, bool errie, bool wakie, bool merre, byte expectedByte)
        {
            var canIntE = new CanIntE(expectedByte);
            Assert.Equal(rx0ie, canIntE.Rx0Ie);
            Assert.Equal(rx1ie, canIntE.Rx1Ie);
            Assert.Equal(tx0ie, canIntE.Tx0Ie);
            Assert.Equal(tx1ie, canIntE.Tx1Ie);
            Assert.Equal(tx2ie, canIntE.Tx2Ie);
            Assert.Equal(errie, canIntE.ErrIe);
            Assert.Equal(wakie, canIntE.WakIe);
            Assert.Equal(merre, canIntE.Merre);

            Assert.Equal(expectedByte, new CanIntE(rx0ie, rx1ie, tx0ie, tx1ie, tx2ie, errie, wakie, merre).ToByte());
        }
    }
}
