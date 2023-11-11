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
    public class PsCon2RegisterTest : RegisterTest
    {
        [Theory]
        // PS_INT
        [InlineData(0b0000_0000, PsInterruptMode.Disabled, PsOutputRange.Bits12)]
        [InlineData(0b0000_0001, PsInterruptMode.Close, PsOutputRange.Bits12)]
        [InlineData(0b0000_0010, PsInterruptMode.Away, PsOutputRange.Bits12)]
        [InlineData(0b0000_0011, PsInterruptMode.CloseOrAway, PsOutputRange.Bits12)]
        // PS_HD
        [InlineData(0b0000_1000, PsInterruptMode.Disabled, PsOutputRange.Bits16)]
        public void Read(byte data, PsInterruptMode interruptMode, PsOutputRange range)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            // low byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);
            // high byte
            testDevice.DataToRead.Enqueue(data);

            var reg = new PsConf2Register(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CONF_1_2, testDevice.DataWritten.Dequeue());
            Assert.Equal(interruptMode, reg.PsInt);
            Assert.Equal(range, reg.PsHd);
        }

        [Theory]
        [InlineData(PsInterruptMode.Disabled, 0b0000_0000)]
        [InlineData(PsInterruptMode.Close, 0b0000_0001)]
        [InlineData(PsInterruptMode.Away, 0b0000_0010)]
        [InlineData(PsInterruptMode.CloseOrAway, 0b0000_0011)]
        public void Write_PsInt(PsInterruptMode interruptMode, byte expectedHighByte)
        {
            const byte mask = 0b0000_0011;

            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf2Register, PsInterruptMode>(0x55,
                                                                0b0000_0000,
                                                                interruptMode,
                                                                0x55,
                                                                expectedHighByte,
                                                                (byte)CommandCode.PS_CONF_1_2,
                                                                nameof(PsConf2Register.PsInt),
                                                                5,
                                                                true);

            PropertyWriteTest<PsConf2Register, PsInterruptMode>(0xaa,
                                                                0b1111_1111,
                                                                interruptMode,
                                                                0xaa,
                                                                (byte)(expectedHighByte | ~mask),
                                                                (byte)CommandCode.PS_CONF_1_2,
                                                                nameof(PsConf2Register.PsInt),
                                                                5,
                                                                true);
        }

        [Theory]
        [InlineData(PsOutputRange.Bits12, 0b0000_0000)]
        [InlineData(PsOutputRange.Bits16, 0b0000_1000)]
        public void Write_PsIt(PsOutputRange range, byte expectedHighByte)
        {
            const byte mask = 0b0000_1000;

            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf2Register, PsOutputRange>(0x55,
                                                              0b0000_0000,
                                                              range,
                                                              0x55,
                                                              expectedHighByte,
                                                              (byte)CommandCode.PS_CONF_1_2,
                                                              nameof(PsConf2Register.PsHd),
                                                              5,
                                                              true);

            PropertyWriteTest<PsConf2Register, PsOutputRange>(0xaa,
                                                              0b1111_1111,
                                                              range,
                                                              0xaa,
                                                              (byte)(expectedHighByte | ~mask),
                                                              (byte)CommandCode.PS_CONF_1_2,
                                                              nameof(PsConf2Register.PsHd),
                                                              5,
                                                              true);
        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new PsConf2Register(new I2cInterface(new I2cTestDevice()));
            Assert.Equal(PsInterruptMode.Disabled, reg.PsInt);
            Assert.Equal(PsOutputRange.Bits12, reg.PsHd);
        }
    }
}
