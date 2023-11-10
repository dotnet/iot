// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;
using Iot.Device.Vcnl4040.Tests;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    /// <summary>
    /// This is a test against the register specification in the datasheet.
    /// </summary>
    public class AlsLowInterruptThresholdRegisterTest : RegisterTest
    {
        [Theory]
        [InlineData(0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010, 0b0000_0101)]
        public void Read(byte thresholdHighByte, byte thresholdLowByte)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            testDevice.DataToRead.Enqueue(thresholdLowByte);
            testDevice.DataToRead.Enqueue(thresholdHighByte);

            var reg = new AlsLowInterruptThresholdRegister(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.ALS_THDL, testDevice.DataWritten.Dequeue());
            Assert.Equal(thresholdHighByte << 8 | thresholdLowByte, reg.Threshold);
        }

        [Theory]
        [InlineData(0b0101_0101_1010_1010, 0b0101_0101, 0b1010_1010)]
        [InlineData(0b1010_1010_0000_0101, 0b1010_1010, 0b0000_0101)]
        public void Write_AlsSd(int threshold, byte expectedThresholdHighByte, byte expectedThresholdLowByte)
        {
            // expect 3 bytes to be written: 1x command code for actual write, 2x data for write
            PropertyWriteTest<AlsLowInterruptThresholdRegister, int>(0x00,
                                                                     0x00,
                                                                     threshold,
                                                                     expectedThresholdLowByte,
                                                                     expectedThresholdHighByte,
                                                                     (byte)CommandCode.ALS_THDL,
                                                                     nameof(AlsLowInterruptThresholdRegister.Threshold),
                                                                     3,
                                                                     false);
        }
    }
}
