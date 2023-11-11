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
    public class PsCon1RegisterTest : RegisterTest
    {
        [Theory]
        // PS_SD
        [InlineData(0b0000_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_0001, PowerState.PowerOff, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        // PS_IT
        [InlineData(0b0000_0010, PowerState.PowerOn, PsIntegrationTime.Time1_5, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_0100, PowerState.PowerOn, PsIntegrationTime.Time2_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_0110, PowerState.PowerOn, PsIntegrationTime.Time2_5, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1000, PowerState.PowerOn, PsIntegrationTime.Time3_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1010, PowerState.PowerOn, PsIntegrationTime.Time3_5, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1100, PowerState.PowerOn, PsIntegrationTime.Time4_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        [InlineData(0b0000_1110, PowerState.PowerOn, PsIntegrationTime.Time8_0, PsInterruptPersistence.Persistence1, PsDuty.Duty40)]
        // PS_PERS
        [InlineData(0b0001_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence2, PsDuty.Duty40)]
        [InlineData(0b0010_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence3, PsDuty.Duty40)]
        [InlineData(0b0011_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence4, PsDuty.Duty40)]
        // PS_DUTY
        [InlineData(0b0100_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty80)]
        [InlineData(0b1000_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty160)]
        [InlineData(0b1100_0000, PowerState.PowerOn, PsIntegrationTime.Time1_0, PsInterruptPersistence.Persistence1, PsDuty.Duty320)]
        public void Read(byte data, PowerState powerState, PsIntegrationTime integrationTime, PsInterruptPersistence persistence, PsDuty duty)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            // low byte
            testDevice.DataToRead.Enqueue(data);
            // high byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);

            var reg = new PsConf1Register(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CONF_1_2, testDevice.DataWritten.Dequeue());
            Assert.Equal(powerState, reg.PsSd);
            Assert.Equal(integrationTime, reg.PsIt);
            Assert.Equal(persistence, reg.PsPers);
            Assert.Equal(duty, reg.PsDuty);
        }

        [Theory]
        [InlineData(PowerState.PowerOff, 0b0000_0001)]
        [InlineData(PowerState.PowerOn, 0b0000_0000)]
        public void Write_PsSd(PowerState powerState, byte expectedLowByte)
        {
            const byte mask = 0b0000_0001;
            byte initialLowByte = 0x00;
            byte unmodifiedHighByte = 0x55;

            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf1Register, PowerState>(initialLowByte,
                                                           unmodifiedHighByte,
                                                           powerState,
                                                           expectedLowByte,
                                                           unmodifiedHighByte,
                                                           (byte)CommandCode.PS_CONF_1_2,
                                                           nameof(PsConf1Register.PsSd),
                                                           5,
                                                           true);

            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf1Register, PowerState>((byte)~initialLowByte,
                                                           (byte)~unmodifiedHighByte,
                                                           powerState,
                                                           (byte)(expectedLowByte | ~mask),
                                                           (byte)~unmodifiedHighByte,
                                                           (byte)CommandCode.PS_CONF_1_2,
                                                           nameof(PsConf1Register.PsSd),
                                                           5,
                                                           true);
        }

        [Theory]
        [InlineData(0b0000_0000, PsIntegrationTime.Time1_0, 0b0000_0000, 0x55)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time1_5, 0b0000_0010, 0xaa)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time2_0, 0b0000_0100, 0x55)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time2_5, 0b0000_0110, 0xaa)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time3_0, 0b0000_1000, 0x55)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time3_5, 0b0000_1010, 0xaa)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time4_0, 0b0000_1100, 0x55)]
        [InlineData(0b0000_0000, PsIntegrationTime.Time8_0, 0b0000_1110, 0xaa)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time1_0, 0b1111_0001, 0x55)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time1_5, 0b1111_0011, 0xaa)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time2_0, 0b1111_0101, 0x55)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time2_5, 0b1111_0111, 0xaa)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time3_0, 0b1111_1001, 0x55)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time3_5, 0b1111_1011, 0xaa)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time4_0, 0b1111_1101, 0x55)]
        [InlineData(0b1111_1111, PsIntegrationTime.Time8_0, 0b1111_1111, 0xaa)]
        public void Write_PsIt(byte initialLowByte, PsIntegrationTime integrationTime, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf1Register, PsIntegrationTime>(initialLowByte,
                                                                  unmodifiedHighByte,
                                                                  integrationTime,
                                                                  expectedLowByte,
                                                                  unmodifiedHighByte,
                                                                  (byte)CommandCode.PS_CONF_1_2,
                                                                  nameof(PsConf1Register.PsIt),
                                                                  5,
                                                                  true);
        }

        [Theory]
        [InlineData(0b0000_0000, PsInterruptPersistence.Persistence1, 0b0000_0000, 0x55)]
        [InlineData(0b0000_0000, PsInterruptPersistence.Persistence2, 0b0001_0000, 0xaa)]
        [InlineData(0b0000_0000, PsInterruptPersistence.Persistence3, 0b0010_0000, 0x55)]
        [InlineData(0b0000_0000, PsInterruptPersistence.Persistence4, 0b0011_0000, 0xaa)]
        [InlineData(0b1111_1111, PsInterruptPersistence.Persistence1, 0b1100_1111, 0x55)]
        [InlineData(0b1111_1111, PsInterruptPersistence.Persistence2, 0b1101_1111, 0xaa)]
        [InlineData(0b1111_1111, PsInterruptPersistence.Persistence3, 0b1110_1111, 0x55)]
        [InlineData(0b1111_1111, PsInterruptPersistence.Persistence4, 0b1111_1111, 0xaa)]
        public void Write_PsPers(byte initialLowByte, PsInterruptPersistence persistence, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf1Register, PsInterruptPersistence>(initialLowByte,
                                                                       unmodifiedHighByte,
                                                                       persistence,
                                                                       expectedLowByte,
                                                                       unmodifiedHighByte,
                                                                       (byte)CommandCode.PS_CONF_1_2,
                                                                       nameof(PsConf1Register.PsPers),
                                                                       5,
                                                                       true);
        }

        [Theory]
        [InlineData(0b0000_0000, PsDuty.Duty40, 0b0000_0000, 0x55)]
        [InlineData(0b0000_0000, PsDuty.Duty80, 0b0100_0000, 0xaa)]
        [InlineData(0b0000_0000, PsDuty.Duty160, 0b1000_0000, 0x55)]
        [InlineData(0b0000_0000, PsDuty.Duty320, 0b1100_0000, 0xaa)]
        [InlineData(0b1111_1111, PsDuty.Duty40, 0b0011_1111, 0x55)]
        [InlineData(0b1111_1111, PsDuty.Duty80, 0b0111_1111, 0xaa)]
        [InlineData(0b1111_1111, PsDuty.Duty160, 0b1011_1111, 0x55)]
        [InlineData(0b1111_1111, PsDuty.Duty320, 0b1111_1111, 0xaa)]
        public void Write_PsDuty(byte initialLowByte, PsDuty duty, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<PsConf1Register, PsDuty>(initialLowByte,
                                                       unmodifiedHighByte,
                                                       duty,
                                                       expectedLowByte,
                                                       unmodifiedHighByte,
                                                       (byte)CommandCode.PS_CONF_1_2,
                                                       nameof(PsConf1Register.PsDuty),
                                                       5,
                                                       true);
        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new PsConf1Register(new I2cInterface(new I2cTestDevice()));
            Assert.Equal(PowerState.PowerOff, reg.PsSd);
            Assert.Equal(PsIntegrationTime.Time1_0, reg.PsIt);
            Assert.Equal(PsInterruptPersistence.Persistence1, reg.PsPers);
            Assert.Equal(PsDuty.Duty40, reg.PsDuty);
        }
    }
}
