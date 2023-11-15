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
    public class WhiteDataRegisterTest : RegisterTest
    {
        [Theory]
        [InlineData(0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010, 0b0101_0101)]
        public void Read(byte highByte, byte lowByte)
        {
            var testDevice = new I2cTestDevice();
            testDevice.DataToRead.Enqueue(lowByte);
            testDevice.DataToRead.Enqueue(highByte);

            var reg = new WhiteDataRegister(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.White_Data, testDevice.DataWritten.Dequeue());
            Assert.Equal(highByte << 8 | lowByte, reg.Data);
        }
    }
}
