// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxRtsCtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.TxRtsCtrl, new TxRtsCtrl(false, false, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(false, false, false, false, false, false, 0b0000_0000)]
        [InlineData(true, false, false, false, false, false, 0b0000_0001)]
        [InlineData(false, true, false, false, false, false, 0b0000_0010)]
        [InlineData(false, false, true, false, false, false, 0b0000_0100)]
        [InlineData(false, false, false, true, false, false, 0b0000_1000)]
        [InlineData(false, false, false, false, true, false, 0b0001_0000)]
        [InlineData(false, false, false, false, false, true, 0b0010_0000)]
        public void To_Byte(bool b0rtsm, bool b1rtsm, bool b2rtsm, bool b0rts, bool b1rts, bool b2rts, byte expectedByte)
        {
            Assert.Equal(expectedByte, new TxRtsCtrl(b0rtsm, b1rtsm, b2rtsm, b0rts, b1rts, b2rts).ToByte());
        }
    }
}
