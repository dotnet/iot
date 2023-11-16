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
    public class AlsHighInterruptThresholdRegisterTest : RegisterTest
    {
        [Theory]
        [InlineData(0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010, 0b0000_0101)]
        public void Read(byte thresholdHighByte, byte thresholdLowByte)
        {
            var testDevice = new I2cTestDevice();
            testDevice.DataToRead.Enqueue(thresholdLowByte);
            testDevice.DataToRead.Enqueue(thresholdHighByte);

            var reg = new AlsHighInterruptThresholdRegister(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.ALS_THDH, testDevice.DataWritten.Dequeue());
            Assert.Equal(thresholdHighByte << 8 | thresholdLowByte, reg.Level);
        }

        [Theory]
        [InlineData(0b0101_0101_1010_1010, 0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010_0000_0101, 0b1010_1010, 0b0000_0101)]
        public void Write(ushort threshold, byte expectedThresholdHighByte, byte expectedThresholdLowByte)
        {
            PropertyWriteTest<AlsHighInterruptThresholdRegister, ushort>(initialRegisterLowByte: 0x00,
                                                                         initialRegisterHighByte: 0x00,
                                                                         testValue: threshold,
                                                                         expectedLowByte: expectedThresholdLowByte,
                                                                         expectedHighByte: expectedThresholdHighByte,
                                                                         commandCode: (byte)CommandCode.ALS_THDH,
                                                                         registerPropertyName: nameof(AlsHighInterruptThresholdRegister.Level),
                                                                         registerReadsBeforeWriting: false);
        }
    }
}
