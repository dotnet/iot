// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Mcp25xxx.Models;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Models
{
    public class SendingCanMessageTests
    {
        [Theory]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2, 0, 0 })]
        [InlineData(new byte[] { 3, 4 }, new byte[] { 3, 4, 0, 0 })]
        public void Create_Standard_Message_Id(byte[] id, byte[] resultId)
        {
            var message = SendingCanMessage.CreateStandard(id, Array.Empty<byte>());
            Assert.Equal(resultId, message.Id);
        }

        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 1, 2, 3, 4 })]
        public void When_Invalid_Id_Create_Standard_Message_Thrown(byte[] id)
        {
            Assert.Throws<ArgumentException>(() => SendingCanMessage.CreateStandard(id, Array.Empty<byte>()));
        }

        [Theory]
        [InlineData(new byte[0], new byte[0])]
        [InlineData(new byte[] { 1 }, new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2, 3, 4 }, new byte[] { 1, 2, 3, 4 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6 }, new byte[] { 1, 2, 3, 4, 5, 6 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
        public void Create_Standard_Message_With_Data(byte[] data, byte[] resultData)
        {
            var message = SendingCanMessage.CreateStandard(new byte[2], data);
            Assert.Equal(resultData, message.Data);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
        public void When_Invalid_Data_Create_Standard_Message_Thrown(byte[] data)
        {
            Assert.Throws<ArgumentException>(() => SendingCanMessage.CreateStandard(new byte[2], data));
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3, 4 }, new byte[] { 1, 2, 3, 4 })]
        [InlineData(new byte[] { 3, 4, 5, 6 }, new byte[] { 3, 4, 5, 6 })]
        public void Create_Extended_Message_Id(byte[] id, byte[] resultId)
        {
            var message = SendingCanMessage.CreateExtended(id, Array.Empty<byte>());
            Assert.Equal(resultId, message.Id);
        }

        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 })]
        public void When_Invalid_Id_Create_Extended_Message_Thrown(byte[] id)
        {
            Assert.Throws<ArgumentException>(() => SendingCanMessage.CreateExtended(id, Array.Empty<byte>()));
        }

        [Theory]
        [InlineData(new byte[0], new byte[0])]
        [InlineData(new byte[] { 1 }, new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2, 3, 4 }, new byte[] { 1, 2, 3, 4 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6 }, new byte[] { 1, 2, 3, 4, 5, 6 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
        public void Create_Extended_Message_With_Data(byte[] data, byte[] resultData)
        {
            var message = SendingCanMessage.CreateStandard(new byte[2], data);
            Assert.Equal(resultData, message.Data);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
        public void When_Invalid_Data_Create_Extended_Message_Thrown(byte[] data)
        {
            Assert.Throws<ArgumentException>(() => SendingCanMessage.CreateStandard(new byte[2], data));
        }
    }
}
