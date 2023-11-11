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
    public class AlsConfRegisterTest : RegisterTest
    {
        [Theory]
        // ALS_SD
        [InlineData(0b0000_0000, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time80ms)]
        [InlineData(0b0000_0001, PowerState.PowerOff, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time80ms)]
        // ALS_INT_EN
        [InlineData(0b0000_0010, PowerState.PowerOn, AlsInterrupt.Enabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time80ms)]
        // ALS_PERS
        [InlineData(0b0000_0100, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence2, AlsIntegrationTime.Time80ms)]
        [InlineData(0b0000_1000, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence4, AlsIntegrationTime.Time80ms)]
        [InlineData(0b0000_1100, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence8, AlsIntegrationTime.Time80ms)]
        // ALS_IT
        [InlineData(0b0100_0000, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time160ms)]
        [InlineData(0b1000_0000, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time320ms)]
        [InlineData(0b1100_0000, PowerState.PowerOn, AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time640ms)]
        public void Read(byte data, PowerState powerState, AlsInterrupt interruptEnabled, AlsInterruptPersistence persistence, AlsIntegrationTime integrationTime)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            // low byte
            testDevice.DataToRead.Enqueue(data);
            // high byte
            testDevice.DataToRead.Enqueue(0x00);

            var reg = new AlsConfRegister(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.ALS_CONF, testDevice.DataWritten.Dequeue());
            Assert.Equal(powerState, reg.AlsSd);
            Assert.Equal(interruptEnabled, reg.AlsIntEn);
            Assert.Equal(persistence, reg.AlsPers);
            Assert.Equal(integrationTime, reg.AlsIt);
        }

        [Theory]
        [InlineData(0b0000_0000, PowerState.PowerOff, 0b0000_0001, 0x55)]
        [InlineData(0b0000_0001, PowerState.PowerOn, 0b0000_0000, 0x55)]
        [InlineData(0b1111_1110, PowerState.PowerOff, 0b1111_1111, 0xaa)]
        [InlineData(0b1111_1111, PowerState.PowerOn, 0b1111_1110, 0xaa)]
        public void Write_AlsSd(byte initialLowByte, PowerState powerState, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<AlsConfRegister, PowerState>(initialLowByte,
                                                           unmodifiedHighByte,
                                                           powerState,
                                                           expectedLowByte,
                                                           unmodifiedHighByte,
                                                           (byte)CommandCode.ALS_CONF,
                                                           nameof(AlsConfRegister.AlsSd),
                                                           5,
                                                           true);
        }

        [Theory]
        [InlineData(0b0000_0000, AlsInterrupt.Enabled, 0b0000_0010, 0x55)]
        [InlineData(0b0000_0010, AlsInterrupt.Disabled, 0b0000_0000, 0x55)]
        [InlineData(0b1111_1101, AlsInterrupt.Enabled, 0b1111_1111, 0xaa)]
        [InlineData(0b1111_1111, AlsInterrupt.Disabled, 0b1111_1101, 0xaa)]
        public void Write_AlsIntEn(byte initialLowByte, AlsInterrupt interruptEnabled, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<AlsConfRegister, AlsInterrupt>(initialLowByte,
                                                             unmodifiedHighByte,
                                                             interruptEnabled,
                                                             expectedLowByte,
                                                             unmodifiedHighByte,
                                                             (byte)CommandCode.ALS_CONF,
                                                             nameof(AlsConfRegister.AlsIntEn),
                                                             5,
                                                             true);
        }

        [Theory]
        [InlineData(0b0000_0000, AlsInterruptPersistence.Persistence1, 0b0000_0000, 0x55)]
        [InlineData(0b0000_0000, AlsInterruptPersistence.Persistence2, 0b0000_0100, 0x55)]
        [InlineData(0b0000_0000, AlsInterruptPersistence.Persistence4, 0b0000_1000, 0x55)]
        [InlineData(0b0000_0000, AlsInterruptPersistence.Persistence8, 0b0000_1100, 0x55)]
        [InlineData(0b1111_1111, AlsInterruptPersistence.Persistence1, 0b1111_0011, 0xaa)]
        [InlineData(0b1111_1111, AlsInterruptPersistence.Persistence2, 0b1111_0111, 0xaa)]
        [InlineData(0b1111_1111, AlsInterruptPersistence.Persistence4, 0b1111_1011, 0xaa)]
        [InlineData(0b1111_1111, AlsInterruptPersistence.Persistence8, 0b1111_1111, 0xaa)]
        public void Write_AlsPers(byte initialLowByte, AlsInterruptPersistence persistence, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<AlsConfRegister, AlsInterruptPersistence>(initialLowByte,
                                                                        unmodifiedHighByte,
                                                                        persistence,
                                                                        expectedLowByte,
                                                                        unmodifiedHighByte,
                                                                        (byte)CommandCode.ALS_CONF,
                                                                        nameof(AlsConfRegister.AlsPers),
                                                                        5,
                                                                        true);
        }

        [Theory]
        [InlineData(0b0000_0000, AlsIntegrationTime.Time80ms, 0b0000_0000, 0x55)]
        [InlineData(0b0000_0000, AlsIntegrationTime.Time160ms, 0b0100_0000, 0x55)]
        [InlineData(0b0000_0000, AlsIntegrationTime.Time320ms, 0b1000_0000, 0x55)]
        [InlineData(0b0000_0000, AlsIntegrationTime.Time640ms, 0b1100_0000, 0x55)]
        [InlineData(0b1100_1111, AlsIntegrationTime.Time80ms, 0b0000_1111, 0xaa)]
        [InlineData(0b1100_1111, AlsIntegrationTime.Time160ms, 0b0100_1111, 0xaa)]
        [InlineData(0b1100_1111, AlsIntegrationTime.Time320ms, 0b1000_1111, 0xaa)]
        [InlineData(0b1100_1111, AlsIntegrationTime.Time640ms, 0b1100_1111, 0xaa)]
        public void Write_AlsIt(byte initialLowByte, AlsIntegrationTime integrationTime, byte expectedLowByte, byte unmodifiedHighByte)
        {
            // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
            PropertyWriteTest<AlsConfRegister, AlsIntegrationTime>(initialLowByte,
                                                                   unmodifiedHighByte,
                                                                   integrationTime,
                                                                   expectedLowByte,
                                                                   unmodifiedHighByte,
                                                                   (byte)CommandCode.ALS_CONF,
                                                                   nameof(AlsConfRegister.AlsIt),
                                                                   5,
                                                                   true);
        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new AlsConfRegister(new I2cInterface(new I2cTestDevice()));
            Assert.Equal(PowerState.PowerOff, reg.AlsSd);
            Assert.Equal(AlsInterrupt.Disabled, reg.AlsIntEn);
            Assert.Equal(AlsInterruptPersistence.Persistence1, reg.AlsPers);
            Assert.Equal(AlsIntegrationTime.Time80ms, reg.AlsIt);
        }
    }
}
