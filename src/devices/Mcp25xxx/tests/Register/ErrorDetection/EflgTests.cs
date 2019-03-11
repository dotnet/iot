// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.ErrorDetection
{
    public class RecTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Eflg, new Eflg(false, false, false, false, false, false, false, false).Address);
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
        public void From_To_Byte(bool ewarn, bool rxwar, bool txwar, bool rxep, bool txep, bool txbo, bool rx0ovr, bool rx1ovr, byte expectedByte)
        {
            var eflg = new Eflg(expectedByte);
            Assert.Equal(ewarn, eflg.Ewarn);
            Assert.Equal(rxwar, eflg.RxWar);
            Assert.Equal(txwar, eflg.TxWar);
            Assert.Equal(rxep, eflg.RxEp);
            Assert.Equal(txep, eflg.TxEp);
            Assert.Equal(txbo, eflg.TxBo);
            Assert.Equal(rx0ovr, eflg.Rx0Ovr);
            Assert.Equal(rx1ovr, eflg.Rx1Ovr);

            Assert.Equal(expectedByte, new Eflg(ewarn, rxwar, txwar, rxep, txep, txbo, rx0ovr, rx1ovr).ToByte());
        }
    }
}
