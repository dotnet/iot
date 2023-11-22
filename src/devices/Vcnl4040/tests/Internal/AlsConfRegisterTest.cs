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
    public class AlsConfRegisterTest : RegisterTest
    {
        [Theory]
        // ALS_SD
        [InlineData(0b0000_0000, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time80ms)]
        [InlineData(0b0000_0001, (byte)PowerState.PowerOff, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time80ms)]
        // ALS_INT_EN
        [InlineData(0b0000_0010, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Enabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time80ms)]
        // ALS_PERS
        [InlineData(0b0000_0100, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence2, AlsIntegrationTime.Time80ms)]
        [InlineData(0b0000_1000, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence4, AlsIntegrationTime.Time80ms)]
        [InlineData(0b0000_1100, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence8, AlsIntegrationTime.Time80ms)]
        // ALS_IT
        [InlineData(0b0100_0000, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time160ms)]
        [InlineData(0b1000_0000, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time320ms)]
        [InlineData(0b1100_0000, (byte)PowerState.PowerOn, (byte)AlsInterrupt.Disabled, AlsInterruptPersistence.Persistence1, AlsIntegrationTime.Time640ms)]
        public void Read(byte data, byte powerState, byte interruptEnabled, AlsInterruptPersistence persistence, AlsIntegrationTime integrationTime)
        {
            var testDevice = new I2cTestDevice();
            // low byte
            testDevice.DataToRead.Enqueue(data);
            // high byte
            testDevice.DataToRead.Enqueue(0x00);

            var reg = new AlsConfRegister(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.ALS_CONF, testDevice.DataWritten.Dequeue());
            Assert.Equal((PowerState)powerState, reg.AlsSd);
            Assert.Equal((AlsInterrupt)interruptEnabled, reg.AlsIntEn);
            Assert.Equal(persistence, reg.AlsPers);
            Assert.Equal(integrationTime, reg.AlsIt);
        }

        [Theory]
        [InlineData((byte)PowerState.PowerOff, 0b0000_0001)]
        [InlineData((byte)PowerState.PowerOn, 0b0000_0000)]
        public void Write_AlsSd(byte powerState, byte expectedLowByte)
        {
            const byte mask = 0b0000_0001;

            PropertyWriteTest<AlsConfRegister, PowerState>(initialRegisterLowByte: InitialLowByte,
                                                           initialRegisterHighByte: UnmodifiedHighByte,
                                                           testValue: (PowerState)powerState,
                                                           expectedLowByte: expectedLowByte,
                                                           expectedHighByte: UnmodifiedHighByte,
                                                           commandCode: (byte)CommandCode.ALS_CONF,
                                                           registerPropertyName: nameof(AlsConfRegister.AlsSd),
                                                           registerReadsBeforeWriting: true);

            PropertyWriteTest<AlsConfRegister, PowerState>(initialRegisterLowByte: InitialLowByteInv,
                                                           initialRegisterHighByte: UnmodifiedHighByteInv,
                                                           testValue: (PowerState)powerState,
                                                           expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                           expectedHighByte: UnmodifiedHighByteInv,
                                                           commandCode: (byte)CommandCode.ALS_CONF,
                                                           registerPropertyName: nameof(AlsConfRegister.AlsSd),
                                                           registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData((byte)AlsInterrupt.Enabled, 0b0000_0010)]
        [InlineData((byte)AlsInterrupt.Disabled, 0b0000_0000)]
        public void Write_AlsIntEn(byte interruptEnabled, byte expectedLowByte)
        {
            const byte mask = 0b0000_0010;

            PropertyWriteTest<AlsConfRegister, AlsInterrupt>(initialRegisterLowByte: InitialLowByte,
                                                             initialRegisterHighByte: UnmodifiedHighByte,
                                                             testValue: (AlsInterrupt)interruptEnabled,
                                                             expectedLowByte: expectedLowByte,
                                                             expectedHighByte: UnmodifiedHighByte,
                                                             commandCode: (byte)CommandCode.ALS_CONF,
                                                             registerPropertyName: nameof(AlsConfRegister.AlsIntEn),
                                                             registerReadsBeforeWriting: true);

            PropertyWriteTest<AlsConfRegister, AlsInterrupt>(initialRegisterLowByte: InitialLowByteInv,
                                                             initialRegisterHighByte: UnmodifiedHighByteInv,
                                                             testValue: (AlsInterrupt)interruptEnabled,
                                                             expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                             expectedHighByte: UnmodifiedHighByteInv,
                                                             commandCode: (byte)CommandCode.ALS_CONF,
                                                             registerPropertyName: nameof(AlsConfRegister.AlsIntEn),
                                                             registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(AlsInterruptPersistence.Persistence1, 0b0000_0000)]
        [InlineData(AlsInterruptPersistence.Persistence2, 0b0000_0100)]
        [InlineData(AlsInterruptPersistence.Persistence4, 0b0000_1000)]
        [InlineData(AlsInterruptPersistence.Persistence8, 0b0000_1100)]
        public void Write_AlsPers(AlsInterruptPersistence persistence, byte expectedLowByte)
        {
            const byte mask = 0b0000_1100;

            PropertyWriteTest<AlsConfRegister, AlsInterruptPersistence>(initialRegisterLowByte: InitialLowByte,
                                                                        initialRegisterHighByte: UnmodifiedHighByte,
                                                                        testValue: persistence,
                                                                        expectedLowByte: expectedLowByte,
                                                                        expectedHighByte: UnmodifiedHighByte,
                                                                        commandCode: (byte)CommandCode.ALS_CONF,
                                                                        registerPropertyName: nameof(AlsConfRegister.AlsPers),
                                                                        registerReadsBeforeWriting: true);

            PropertyWriteTest<AlsConfRegister, AlsInterruptPersistence>(initialRegisterLowByte: InitialLowByteInv,
                                                                        initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                        testValue: persistence,
                                                                        expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                        expectedHighByte: UnmodifiedHighByteInv,
                                                                        commandCode: (byte)CommandCode.ALS_CONF,
                                                                        registerPropertyName: nameof(AlsConfRegister.AlsPers),
                                                                        registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, 0b0000_0000)]
        [InlineData(AlsIntegrationTime.Time160ms, 0b0100_0000)]
        [InlineData(AlsIntegrationTime.Time320ms, 0b1000_0000)]
        [InlineData(AlsIntegrationTime.Time640ms, 0b1100_0000)]
        public void Write_AlsIt(AlsIntegrationTime integrationTime, byte expectedLowByte)
        {
            const byte mask = 0b1100_0000;

            PropertyWriteTest<AlsConfRegister, AlsIntegrationTime>(initialRegisterLowByte: InitialLowByte,
                                                                   initialRegisterHighByte: UnmodifiedHighByte,
                                                                   testValue: integrationTime,
                                                                   expectedLowByte: expectedLowByte,
                                                                   expectedHighByte: UnmodifiedHighByte,
                                                                   commandCode: (byte)CommandCode.ALS_CONF,
                                                                   registerPropertyName: nameof(AlsConfRegister.AlsIt),
                                                                   registerReadsBeforeWriting: true);

            PropertyWriteTest<AlsConfRegister, AlsIntegrationTime>(initialRegisterLowByte: InitialLowByteInv,
                                                                   initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                   testValue: integrationTime,
                                                                   expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                   expectedHighByte: UnmodifiedHighByteInv,
                                                                   commandCode: (byte)CommandCode.ALS_CONF,
                                                                   registerPropertyName: nameof(AlsConfRegister.AlsIt),
                                                                   registerReadsBeforeWriting: true);
        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new AlsConfRegister(new I2cTestDevice());
            Assert.Equal(PowerState.PowerOff, reg.AlsSd);
            Assert.Equal(AlsInterrupt.Disabled, reg.AlsIntEn);
            Assert.Equal(AlsInterruptPersistence.Persistence1, reg.AlsPers);
            Assert.Equal(AlsIntegrationTime.Time80ms, reg.AlsIt);
        }
    }
}
