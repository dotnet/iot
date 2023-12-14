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
    public class PsConf1RegisterTest : RegisterTest
    {
        [Theory]
        // PS_SD
        [InlineData(0b0000_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_0001, (byte)PowerState.PowerOff, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        // PS_IT
        [InlineData(0b0000_0010, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_5, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_0100, (byte)PowerState.PowerOn, PsIntegrationTime.Time2_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_0110, (byte)PowerState.PowerOn, PsIntegrationTime.Time2_5, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1000, (byte)PowerState.PowerOn, PsIntegrationTime.Time3_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1010, (byte)PowerState.PowerOn, PsIntegrationTime.Time3_5, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1100, (byte)PowerState.PowerOn, PsIntegrationTime.Time4_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1110, (byte)PowerState.PowerOn, PsIntegrationTime.Time8_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        // PS_PERS
        [InlineData(0b0001_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence2, PsDuty.Duty40)]
        [InlineData(0b0010_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence3, PsDuty.Duty40)]
        [InlineData(0b0011_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence4, PsDuty.Duty40)]
        // PS_DUTY
        [InlineData(0b0100_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty80)]
        [InlineData(0b1000_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty160)]
        [InlineData(0b1100_0000, (byte)PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty320)]
        public void Read(byte regsiterData, byte powerState, PsIntegrationTime integrationTime, PsInterruptPersistence persistence, PsDuty duty)
        {
            var testDevice = new I2cTestDevice();
            // low byte
            testDevice.DataToRead.Enqueue(regsiterData);
            // high byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);

            var reg = new PsConf1Register(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CONF_1_2, testDevice.DataWritten.Dequeue());
            Assert.Equal((PowerState)powerState, reg.PsSd);
            Assert.Equal(integrationTime, reg.PsIt);
            Assert.Equal(persistence, reg.PsPers);
            Assert.Equal(duty, reg.PsDuty);
        }

        [Theory]
        [InlineData((byte)PowerState.PowerOff, 0b0000_0001)]
        [InlineData((byte)PowerState.PowerOn, 0b0000_0000)]
        public void Write_PsSd(byte powerState, byte expectedLowByte)
        {
            const byte mask = 0b0000_0001;

            PropertyWriteTest<PsConf1Register, PowerState>(initialRegisterLowByte: InitialLowByte,
                                                           initialRegisterHighByte: UnmodifiedHighByte,
                                                           testValue: (PowerState)powerState,
                                                           expectedLowByte: expectedLowByte,
                                                           expectedHighByte: UnmodifiedHighByte,
                                                           commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                           registerPropertyName: nameof(PsConf1Register.PsSd),
                                                           registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf1Register, PowerState>(initialRegisterLowByte: InitialLowByteInv,
                                                           initialRegisterHighByte: UnmodifiedHighByteInv,
                                                           testValue: (PowerState)powerState,
                                                           expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                           expectedHighByte: UnmodifiedHighByteInv,
                                                           commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                           registerPropertyName: nameof(PsConf1Register.PsSd),
                                                           registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(PsIntegrationTime.Time1_0, 0b0000_0000)]
        [InlineData(PsIntegrationTime.Time1_5, 0b0000_0010)]
        [InlineData(PsIntegrationTime.Time2_0, 0b0000_0100)]
        [InlineData(PsIntegrationTime.Time2_5, 0b0000_0110)]
        [InlineData(PsIntegrationTime.Time3_0, 0b0000_1000)]
        [InlineData(PsIntegrationTime.Time3_5, 0b0000_1010)]
        [InlineData(PsIntegrationTime.Time4_0, 0b0000_1100)]
        [InlineData(PsIntegrationTime.Time8_0, 0b0000_1110)]
        public void Write_PsIt(PsIntegrationTime integrationTime, byte expectedLowByte)
        {
            const byte mask = 0b0000_1110;

            PropertyWriteTest<PsConf1Register, PsIntegrationTime>(initialRegisterLowByte: InitialLowByte,
                                                                  initialRegisterHighByte: UnmodifiedHighByte,
                                                                  testValue: integrationTime,
                                                                  expectedLowByte: expectedLowByte,
                                                                  expectedHighByte: UnmodifiedHighByte,
                                                                  commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                                  registerPropertyName: nameof(PsConf1Register.PsIt),
                                                                  registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf1Register, PsIntegrationTime>(initialRegisterLowByte: InitialLowByteInv,
                                                                  initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                  testValue: integrationTime,
                                                                  expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                  expectedHighByte: UnmodifiedHighByteInv,
                                                                  commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                                  registerPropertyName: nameof(PsConf1Register.PsIt),
                                                                  registerReadsBeforeWriting: true);

        }

        [Theory]
        [InlineData(PsInterruptPersistence.Persistence1, 0b0000_0000)]
        [InlineData(PsInterruptPersistence.Persistence2, 0b0001_0000)]
        [InlineData(PsInterruptPersistence.Persistence3, 0b0010_0000)]
        [InlineData(PsInterruptPersistence.Persistence4, 0b0011_0000)]
        public void Write_PsPers(PsInterruptPersistence persistence, byte expectedLowByte)
        {
            const byte mask = 0b0011_0000;

            PropertyWriteTest<PsConf1Register, PsInterruptPersistence>(initialRegisterLowByte: InitialLowByte,
                                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                                       testValue: persistence,
                                                                       expectedLowByte: expectedLowByte,
                                                                       expectedHighByte: UnmodifiedHighByte,
                                                                       commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                                       registerPropertyName: nameof(PsConf1Register.PsPers),
                                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf1Register, PsInterruptPersistence>(initialRegisterLowByte: InitialLowByteInv,
                                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                       testValue: persistence,
                                                                       expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                       expectedHighByte: UnmodifiedHighByteInv,
                                                                       commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                                       registerPropertyName: nameof(PsConf1Register.PsPers),
                                                                       registerReadsBeforeWriting: true);

        }

        [Theory]
        [InlineData(PsDuty.Duty40, 0b0000_0000)]
        [InlineData(PsDuty.Duty80, 0b0100_0000)]
        [InlineData(PsDuty.Duty160, 0b1000_0000)]
        [InlineData(PsDuty.Duty320, 0b1100_0000)]
        public void Write_PsDuty(PsDuty duty, byte expectedLowByte)
        {
            const byte mask = 0b1100_0000;
            PropertyWriteTest<PsConf1Register, PsDuty>(initialRegisterLowByte: InitialLowByte,
                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                       testValue: duty,
                                                       expectedLowByte: expectedLowByte,
                                                       expectedHighByte: UnmodifiedHighByte,
                                                       commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                       registerPropertyName: nameof(PsConf1Register.PsDuty),
                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf1Register, PsDuty>(initialRegisterLowByte: InitialLowByteInv,
                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                       testValue: duty,
                                                       expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                       expectedHighByte: UnmodifiedHighByteInv,
                                                       commandCode: (byte)CommandCode.PS_CONF_1_2,
                                                       registerPropertyName: nameof(PsConf1Register.PsDuty),
                                                       registerReadsBeforeWriting: true);

        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new PsConf1Register(new I2cTestDevice());
            Assert.Equal(PowerState.PowerOff, reg.PsSd);
            Assert.Equal(PsIntegrationTime.Time1_0, reg.PsIt);
            Assert.Equal(PsInterruptPersistence.Persistence1, reg.PsPers);
            Assert.Equal(PsDuty.Duty40, reg.PsDuty);
        }
    }
}
