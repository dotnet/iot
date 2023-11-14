// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Common.Definitions;
using Iot.Device.Vcnl4040.Internal;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests.Internal
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
        public void Read(byte registerData, PsInterruptMode interruptMode, PsOutputRange range)
        {
            var testDevice = new I2cTestDevice();
            // low byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);
            // high byte
            testDevice.DataToRead.Enqueue(registerData);

            var reg = new PsConf2Register(testDevice);
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

            PropertyWriteTest<PsConf2Register, PsInterruptMode>(initialRegisterLowByte: UnmodifiedLowByte,
                                                                initialRegisterHighByte: InitialHighByte,
                                                                testValue: interruptMode,
                                                                expectedLowByte: UnmodifiedLowByte,
                                                                expectedHighByte: expectedHighByte,
                                                                commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                                registerPropertyName: nameof(PsConf2Register.PsInt),
                                                                registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf2Register, PsInterruptMode>(initialRegisterLowByte: UnmodifiedLowByteInv,
                                                                initialRegisterHighByte: InitialHighByteInv,
                                                                testValue: interruptMode,
                                                                expectedLowByte: UnmodifiedLowByteInv,
                                                                expectedHighByte: (byte)(expectedHighByte | ~mask),
                                                                commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                                registerPropertyName: nameof(PsConf2Register.PsInt),
                                                                registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(PsOutputRange.Bits12, 0b0000_0000)]
        [InlineData(PsOutputRange.Bits16, 0b0000_1000)]
        public void Write_PsIt(PsOutputRange range, byte expectedHighByte)
        {
            const byte mask = 0b0000_1000;

            PropertyWriteTest<PsConf2Register, PsOutputRange>(initialRegisterLowByte: UnmodifiedLowByte,
                                                              initialRegisterHighByte: InitialHighByte,
                                                              testValue: range,
                                                              expectedLowByte: UnmodifiedLowByte,
                                                              expectedHighByte: expectedHighByte,
                                                              commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                              registerPropertyName: nameof(PsConf2Register.PsHd),
                                                              registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf2Register, PsOutputRange>(initialRegisterLowByte: UnmodifiedLowByteInv,
                                                              initialRegisterHighByte: InitialHighByteInv,
                                                              testValue: range,
                                                              expectedLowByte: UnmodifiedLowByteInv,
                                                              expectedHighByte: (byte)(expectedHighByte | ~mask),
                                                              commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                              registerPropertyName: nameof(PsConf2Register.PsHd),
                                                              registerReadsBeforeWriting: true);
        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new PsConf2Register(new I2cTestDevice());
            Assert.Equal(PsInterruptMode.Disabled, reg.PsInt);
            Assert.Equal(PsOutputRange.Bits12, reg.PsHd);
        }
    }
}
