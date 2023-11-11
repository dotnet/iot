// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    /// <summary>
    /// This is a test against the register specification in the datasheet.
    /// </summary>
    public class PsCancellationLevelRegisterTest : RegisterTest
    {
        [Theory]
        [InlineData(0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010, 0b0000_0101)]
        public void Read(byte highByte, byte lowByte)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            testDevice.DataToRead.Enqueue(lowByte);
            testDevice.DataToRead.Enqueue(highByte);

            var reg = new PsCancellationLevelRegister(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CANC, testDevice.DataWritten.Dequeue());
            Assert.Equal(highByte << 8 | lowByte, reg.Level);
        }

        [Theory]
        [InlineData(0b0101_0101_1010_1010, 0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010_0000_0101, 0b1010_1010, 0b0000_0101)]
        public void Write(int level, byte expectedHighByte, byte expectedLowByte)
        {
            PropertyWriteTest<PsCancellationLevelRegister, int>(0x00,
                                                                0x00,
                                                                level,
                                                                expectedLowByte,
                                                                expectedHighByte,
                                                                (byte)CommandCode.PS_CANC,
                                                                nameof(PsCancellationLevelRegister.Level),
                                                                false);
        }
    }
}
