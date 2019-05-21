// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using static Iot.Device.Mcp25xxx.RxStatusResponse;

namespace Iot.Device.Mcp25xxx.Tests
{
    public class RxStatusResponseTests
    {
        [Theory]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0000)]
        [InlineData(FilterMatchType.RxF1, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0001)]
        [InlineData(FilterMatchType.RxF2, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0010)]
        [InlineData(FilterMatchType.RxF3, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0011)]
        [InlineData(FilterMatchType.RxF4, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0100)]
        [InlineData(FilterMatchType.RxF5, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0101)]
        [InlineData(FilterMatchType.RxF0RolloverToRxB1, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0110)]
        [InlineData(FilterMatchType.RxF1RolloverToRxB1, MessageReceivedType.StandardDataFrame, ReceivedMessageType.NoRxMessage, 0b0000_0111)]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.StandardRemoteFrame, ReceivedMessageType.NoRxMessage, 0b0000_1000)]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.ExtendedDataFrame, ReceivedMessageType.NoRxMessage, 0b0001_0000)]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.ExtendedRemoteFrame, ReceivedMessageType.NoRxMessage, 0b0001_1000)]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.StandardDataFrame, ReceivedMessageType.MessageInRxB0, 0b0100_0000)]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.StandardDataFrame, ReceivedMessageType.MessageInRxB1, 0b1000_0000)]
        [InlineData(FilterMatchType.RxF0, MessageReceivedType.StandardDataFrame, ReceivedMessageType.MessagesInBothBuffers, 0b1100_0000)]
        public void To_Byte(FilterMatchType filterMatch, MessageReceivedType messageTypeReceived, ReceivedMessageType receivedMessage, byte expectedByte)
        {
            Assert.Equal(expectedByte, new RxStatusResponse(filterMatch, messageTypeReceived, receivedMessage).ToByte());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1100_0000)]
        [InlineData(0b0001_1000)]
        [InlineData(0b0000_0111)]
        public void From_To_Byte(byte expectedByte)
        {
            Assert.Equal(expectedByte, new RxStatusResponse(expectedByte).ToByte());
        }
    }
}
