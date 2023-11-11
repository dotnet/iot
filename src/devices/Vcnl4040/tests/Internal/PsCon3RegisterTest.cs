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
    public class PsCon3RegisterTest : RegisterTest
    {
        [Theory]
        // PS_SC_EN
        [InlineData(0b0000_0000, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Disabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        [InlineData(0b0000_0001, PsSunlightCancellationState.Enabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Disabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        // PS_TRIG
        [InlineData(0b0000_0100, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.OneTimeCycle, PsActiveForceMode.Disabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        // PS_AF
        [InlineData(0b0000_1000, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Enabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        // PS_SMART_PERS
        [InlineData(0b0001_0000, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Disabled, PsSmartPersistenceState.Enabled, PsMultiPulse.Pulse1)]
        // PS_SMART_MPS
        [InlineData(0b0010_0000, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Disabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse2)]
        [InlineData(0b0100_0000, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Disabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse4)]
        [InlineData(0b0110_0000, PsSunlightCancellationState.Disabled, PsActiveForceModeTrigger.NoTrigger, PsActiveForceMode.Disabled, PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse8)]
        public void Read(byte registerData, PsSunlightCancellationState sunlightCancellationState, PsActiveForceModeTrigger trigger, PsActiveForceMode forceMode, PsSmartPersistenceState smartPersistenceState, PsMultiPulse multiPulse)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            // low byte
            testDevice.DataToRead.Enqueue(registerData);
            // high byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);

            var reg = new PsConf3Register(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CONF_3_MS, testDevice.DataWritten.Dequeue());
            Assert.Equal(sunlightCancellationState, reg.PsScEn);
            Assert.Equal(trigger, reg.PsTrig);
            Assert.Equal(forceMode, reg.PsAf);
            Assert.Equal(smartPersistenceState, reg.PsSmartPers);
            Assert.Equal(multiPulse, reg.PsMps);
        }

        [Theory]
        [InlineData(PsSunlightCancellationState.Disabled, 0b0000_0000)]
        [InlineData(PsSunlightCancellationState.Enabled, 0b0000_0001)]
        public void Write_PsScEn(PsSunlightCancellationState sunlightCancellationState, byte expectedLowByte)
        {
            const byte mask = 0b0000_0001;

            PropertyWriteTest<PsConf3Register, PsSunlightCancellationState>(initialRegisterLowByte: InitialLowByte,
                                                                            initialRegisterHighByte: UnmodifiedHighByte,
                                                                            testValue: sunlightCancellationState,
                                                                            expectedLowByte: expectedLowByte,
                                                                            expectedHighByte: UnmodifiedHighByte,
                                                                            commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                            registerPropertyName: nameof(PsConf3Register.PsScEn),
                                                                            registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsSunlightCancellationState>(initialRegisterLowByte: InitialLowByteInv,
                                                                            initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                            testValue: sunlightCancellationState,
                                                                            expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                            expectedHighByte: UnmodifiedHighByteInv,
                                                                            commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                            registerPropertyName: nameof(PsConf3Register.PsScEn),
                                                                            registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(PsActiveForceModeTrigger.NoTrigger, 0b0000_0000)]
        [InlineData(PsActiveForceModeTrigger.OneTimeCycle, 0b0000_0100)]
        public void Write_PsTrig(PsActiveForceModeTrigger trigger, byte expectedLowByte)
        {
            const byte mask = 0b0000_0100;

            PropertyWriteTest<PsConf3Register, PsActiveForceModeTrigger>(initialRegisterLowByte: InitialLowByte,
                                                                         initialRegisterHighByte: UnmodifiedHighByte,
                                                                         testValue: trigger,
                                                                         expectedLowByte: expectedLowByte,
                                                                         expectedHighByte: UnmodifiedHighByte,
                                                                         commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                         registerPropertyName: nameof(PsConf3Register.PsTrig),
                                                                         registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsActiveForceModeTrigger>(initialRegisterLowByte: InitialLowByteInv,
                                                                         initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                         testValue: trigger,
                                                                         expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                         expectedHighByte: UnmodifiedHighByteInv,
                                                                         commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                         registerPropertyName: nameof(PsConf3Register.PsTrig),
                                                                         registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(PsActiveForceMode.Disabled, 0b0000_0000)]
        [InlineData(PsActiveForceMode.Enabled, 0b0000_1000)]
        public void Write_PsAf(PsActiveForceMode activeForceMode, byte expectedLowByte)
        {
            const byte mask = 0b0000_1000;

            PropertyWriteTest<PsConf3Register, PsActiveForceMode>(initialRegisterLowByte: InitialLowByte,
                                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                                       testValue: activeForceMode,
                                                                       expectedLowByte: expectedLowByte,
                                                                       expectedHighByte: UnmodifiedHighByte,
                                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                       registerPropertyName: nameof(PsConf3Register.PsAf),
                                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsActiveForceMode>(initialRegisterLowByte: InitialLowByteInv,
                                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                       testValue: activeForceMode,
                                                                       expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                       expectedHighByte: UnmodifiedHighByteInv,
                                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                       registerPropertyName: nameof(PsConf3Register.PsAf),
                                                                       registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData(PsSmartPersistenceState.Disabled, 0b0000_0000)]
        [InlineData(PsSmartPersistenceState.Enabled, 0b0001_0000)]
        public void Write_PsSmartPers(PsSmartPersistenceState smartPersistenceState, byte expectedLowByte)
        {
            const byte mask = 0b0001_0000;
            PropertyWriteTest<PsConf3Register, PsSmartPersistenceState>(initialRegisterLowByte: InitialLowByte,
                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                       testValue: smartPersistenceState,
                                                       expectedLowByte: expectedLowByte,
                                                       expectedHighByte: UnmodifiedHighByte,
                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                       registerPropertyName: nameof(PsConf3Register.PsSmartPers),
                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsSmartPersistenceState>(initialRegisterLowByte: InitialLowByteInv,
                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                       testValue: smartPersistenceState,
                                                       expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                       expectedHighByte: UnmodifiedHighByteInv,
                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                       registerPropertyName: nameof(PsConf3Register.PsSmartPers),
                                                       registerReadsBeforeWriting: true);

        }

        [Theory]
        [InlineData(PsMultiPulse.Pulse1, 0b0000_0000)]
        [InlineData(PsMultiPulse.Pulse2, 0b0010_0000)]
        [InlineData(PsMultiPulse.Pulse4, 0b0100_0000)]
        [InlineData(PsMultiPulse.Pulse8, 0b0110_0000)]
        public void Write_PsMps(PsMultiPulse multiPulse, byte expectedLowByte)
        {
            const byte mask = 0b0110_0000;
            PropertyWriteTest<PsConf3Register, PsMultiPulse>(initialRegisterLowByte: InitialLowByte,
                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                       testValue: multiPulse,
                                                       expectedLowByte: expectedLowByte,
                                                       expectedHighByte: UnmodifiedHighByte,
                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                       registerPropertyName: nameof(PsConf3Register.PsMps),
                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsMultiPulse>(initialRegisterLowByte: InitialLowByteInv,
                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                       testValue: multiPulse,
                                                       expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                       expectedHighByte: UnmodifiedHighByteInv,
                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                       registerPropertyName: nameof(PsConf3Register.PsMps),
                                                       registerReadsBeforeWriting: true);

        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new PsConf3Register(new I2cInterface(new I2cTestDevice()));
            Assert.Equal(PsSunlightCancellationState.Disabled, reg.PsScEn);
            Assert.Equal(PsActiveForceModeTrigger.NoTrigger, reg.PsTrig);
            Assert.Equal(PsSmartPersistenceState.Disabled, reg.PsSmartPers);
            Assert.Equal(PsMultiPulse.Pulse1, reg.PsMps);
        }
    }
}
