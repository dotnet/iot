// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests.Internal
{
    /// <summary>
    /// This is a test against the register specification in the datasheet.
    /// </summary>
    public class IdRegisterTest : RegisterTest
    {
        [Theory]
        [InlineData(0x86, 0b0000_0101)]
        [InlineData(0x86, 0b0000_1010)]
        public void Read(byte lowByte, byte highByte)
        {
            var testDevice = new I2cTestDevice();
            testDevice.DataToRead.Enqueue(lowByte);
            testDevice.DataToRead.Enqueue(highByte);

            var reg = new IdRegister(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.ID, testDevice.DataWritten.Dequeue());
            Assert.Equal(highByte << 8 | lowByte, reg.Id);
        }
    }
}
