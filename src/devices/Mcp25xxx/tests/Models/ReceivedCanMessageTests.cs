// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Iot.Device.Mcp25xxx.Models;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Models
{
    public class ReceivedCanMessageTests
    {
        [Theory]
        [InlineData(new byte[] { 0, 1, 2, 3 }, new byte[] { 0, 1, 2, 3 })]
        [InlineData(new byte[] { 4, 3, 2, 1, 0 }, new byte[] { 4, 3, 2, 1 })]
        [InlineData(new byte[] { 10, 11, 12, 13, 14, 15 }, new byte[] { 10, 11, 12, 13 })]
        public void Get_Message_Id(byte[] rawData, byte[] id)
        {
            var message = new ReceivedCanMessage(ReceiveBuffer.RxB0, rawData);
            Assert.Equal(id, message.GetId());
        }

        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2, 3 })]
        public void When_Invalid_RawData_Get_Message_Id_Thrown(byte[] rawData)
        {
            var message = new ReceivedCanMessage(ReceiveBuffer.RxB0, rawData);
            Assert.Throws<InvalidDataException>(() => message.GetId());
        }

        [Theory]
        [InlineData(new byte[] { 0, 1, 2, 3, 0 }, new byte[0])]
        [InlineData(new byte[] { 0, 1, 2, 3, 1, 14 }, new byte[] { 14 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 2, 14, 15 }, new byte[] { 14, 15 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 3, 14, 15, 16 }, new byte[] { 14, 15, 16 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 4, 14, 15, 16, 17 }, new byte[] { 14, 15, 16, 17 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 5, 14, 15, 16, 17, 18 }, new byte[] { 14, 15, 16, 17, 18 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 6, 14, 15, 16, 17, 18, 19 }, new byte[] { 14, 15, 16, 17, 18, 19 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 7, 14, 15, 16, 17, 18, 19, 20 }, new byte[] { 14, 15, 16, 17, 18, 19, 20 })]
        [InlineData(new byte[] { 0, 1, 2, 3, 8, 14, 15, 16, 17, 18, 19, 20, 21 }, new byte[] { 14, 15, 16, 17, 18, 19, 20, 21 })]
        public void Get_Message_Data(byte[] rawData, byte[] id)
        {
            var message = new ReceivedCanMessage(ReceiveBuffer.RxB0, rawData);
            Assert.Equal(id, message.GetData());
        }

        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1 })]
        [InlineData(new byte[] { 2, 3 })]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 1, 2, 3, 4 })]
        public void When_Invalid_RawData_Get_Message_Data_Thrown(byte[] rawData)
        {
            var message = new ReceivedCanMessage(ReceiveBuffer.RxB0, rawData);
            Assert.Throws<InvalidDataException>(() => message.GetData());
        }

    }
}
