// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxDlcTests
    {
        [Theory]
        [InlineData(0, Address.RxB0Dlc)]
        [InlineData(1, Address.RxB1Dlc)]
        public void Get_RxBufferNumber_Address(byte rxBufferNumber, Address address)
        {
            Assert.Equal(rxBufferNumber, RxBxDlc.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxDlc(rxBufferNumber, 0b0000_0000, false).Address);
        }

        [Theory]
        [InlineData(0b0000, false, 0b0000_0000)]
        [InlineData(0b1000, false, 0b0000_1000)]
        [InlineData(0b0000, true, 0b0100_0000)]
        public void From_To_Byte(byte dataLengthCode, bool extendedFrameRemoteTransmissionRequest, byte expectedByte)
        {
            var rxBxDlc = new RxBxDlc(0, dataLengthCode, extendedFrameRemoteTransmissionRequest);
            Assert.Equal(dataLengthCode, rxBxDlc.DataLengthCode);
            Assert.Equal(extendedFrameRemoteTransmissionRequest, rxBxDlc.ExtendedFrameRemoteTransmissionRequest);
            Assert.Equal(expectedByte, rxBxDlc.ToByte());
            Assert.Equal(expectedByte, new RxBxDlc(0, expectedByte).ToByte());
        }
    }
}
