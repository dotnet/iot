// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.Interrupt
{
    public class CanIntFTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanIntF, new CanIntF(false, false, false, false, false, false, false, false).Address);
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
        public void From_To_Byte(bool rx0if, bool rx1if, bool tx0if, bool tx1if, bool tx2if, bool errif, bool wakif, bool merrf, byte expectedByte)
        {
            var canIntF = new CanIntF(expectedByte);
            Assert.Equal(rx0if, canIntF.Rx0If);
            Assert.Equal(rx1if, canIntF.Rx1If);
            Assert.Equal(tx0if, canIntF.Tx0If);
            Assert.Equal(tx1if, canIntF.Tx1If);
            Assert.Equal(tx2if, canIntF.Tx2If);
            Assert.Equal(errif, canIntF.ErrIf);
            Assert.Equal(wakif, canIntF.WakIf);
            Assert.Equal(merrf, canIntF.Merrf);

            Assert.Equal(expectedByte, new CanIntF(rx0if, rx1if, tx0if, tx1if, tx2if, errif, wakif, merrf).ToByte());
        }
    }
}
