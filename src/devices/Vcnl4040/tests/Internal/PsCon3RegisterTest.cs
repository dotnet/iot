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
    public class PsCon3RegisterTest : RegisterTest
    {
        [Theory]
        // PS_SC_EN
        [InlineData(0b0000_0000, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        [InlineData(0b0000_0001, (byte)PsSunlightCancellationState.Enabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        // PS_TRIG
        [InlineData(0b0000_0100, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.OneTimeCycle, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        // PS_AF
        [InlineData(0b0000_1000, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Enabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse1)]
        // PS_SMART_PERS
        [InlineData(0b0001_0000, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Enabled, PsMultiPulse.Pulse1)]
        // PS_SMART_MPS
        [InlineData(0b0010_0000, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse2)]
        [InlineData(0b0100_0000, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse4)]
        [InlineData(0b0110_0000, (byte)PsSunlightCancellationState.Disabled, (byte)PsActiveForceModeTrigger.NoTrigger, (byte)PsActiveForceMode.Disabled, (byte)PsSmartPersistenceState.Disabled, PsMultiPulse.Pulse8)]
        public void Read(byte registerData, byte sunlightCancellationState, byte trigger, byte forceMode, byte smartPersistenceState, PsMultiPulse multiPulse)
        {
            var testDevice = new I2cTestDevice();
            // low byte
            testDevice.DataToRead.Enqueue(registerData);
            // high byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);

            var reg = new PsConf3Register(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CONF_3_MS, testDevice.DataWritten.Dequeue());
            Assert.Equal((PsSunlightCancellationState)sunlightCancellationState, reg.PsScEn);
            Assert.Equal((PsActiveForceModeTrigger)trigger, reg.PsTrig);
            Assert.Equal((PsActiveForceMode)forceMode, reg.PsAf);
            Assert.Equal((PsSmartPersistenceState)smartPersistenceState, reg.PsSmartPers);
            Assert.Equal(multiPulse, reg.PsMps);
        }

        [Theory]
        [InlineData((byte)PsSunlightCancellationState.Disabled, 0b0000_0000)]
        [InlineData((byte)PsSunlightCancellationState.Enabled, 0b0000_0001)]
        public void Write_PsScEn(byte sunlightCancellationState, byte expectedLowByte)
        {
            const byte mask = 0b0000_0001;

            PropertyWriteTest<PsConf3Register, PsSunlightCancellationState>(initialRegisterLowByte: InitialLowByte,
                                                                            initialRegisterHighByte: UnmodifiedHighByte,
                                                                            testValue: (PsSunlightCancellationState)sunlightCancellationState,
                                                                            expectedLowByte: expectedLowByte,
                                                                            expectedHighByte: UnmodifiedHighByte,
                                                                            commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                            registerPropertyName: nameof(PsConf3Register.PsScEn),
                                                                            registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsSunlightCancellationState>(initialRegisterLowByte: InitialLowByteInv,
                                                                            initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                            testValue: (PsSunlightCancellationState)sunlightCancellationState,
                                                                            expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                            expectedHighByte: UnmodifiedHighByteInv,
                                                                            commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                            registerPropertyName: nameof(PsConf3Register.PsScEn),
                                                                            registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData((byte)PsActiveForceModeTrigger.NoTrigger, 0b0000_0000)]
        [InlineData((byte)PsActiveForceModeTrigger.OneTimeCycle, 0b0000_0100)]
        public void Write_PsTrig(byte trigger, byte expectedLowByte)
        {
            const byte mask = 0b0000_0100;

            PropertyWriteTest<PsConf3Register, PsActiveForceModeTrigger>(initialRegisterLowByte: InitialLowByte,
                                                                         initialRegisterHighByte: UnmodifiedHighByte,
                                                                         testValue: (PsActiveForceModeTrigger)trigger,
                                                                         expectedLowByte: expectedLowByte,
                                                                         expectedHighByte: UnmodifiedHighByte,
                                                                         commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                         registerPropertyName: nameof(PsConf3Register.PsTrig),
                                                                         registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsActiveForceModeTrigger>(initialRegisterLowByte: InitialLowByteInv,
                                                                         initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                         testValue: (PsActiveForceModeTrigger)trigger,
                                                                         expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                         expectedHighByte: UnmodifiedHighByteInv,
                                                                         commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                         registerPropertyName: nameof(PsConf3Register.PsTrig),
                                                                         registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData((byte)PsActiveForceMode.Disabled, 0b0000_0000)]
        [InlineData((byte)PsActiveForceMode.Enabled, 0b0000_1000)]
        public void Write_PsAf(byte activeForceMode, byte expectedLowByte)
        {
            const byte mask = 0b0000_1000;

            PropertyWriteTest<PsConf3Register, PsActiveForceMode>(initialRegisterLowByte: InitialLowByte,
                                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                                       testValue: (PsActiveForceMode)activeForceMode,
                                                                       expectedLowByte: expectedLowByte,
                                                                       expectedHighByte: UnmodifiedHighByte,
                                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                       registerPropertyName: nameof(PsConf3Register.PsAf),
                                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsActiveForceMode>(initialRegisterLowByte: InitialLowByteInv,
                                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                                       testValue: (PsActiveForceMode)activeForceMode,
                                                                       expectedLowByte: (byte)(expectedLowByte | ~mask),
                                                                       expectedHighByte: UnmodifiedHighByteInv,
                                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                       registerPropertyName: nameof(PsConf3Register.PsAf),
                                                                       registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData((byte)PsSmartPersistenceState.Disabled, 0b0000_0000)]
        [InlineData((byte)PsSmartPersistenceState.Enabled, 0b0001_0000)]
        public void Write_PsSmartPers(byte smartPersistenceState, byte expectedLowByte)
        {
            const byte mask = 0b0001_0000;
            PropertyWriteTest<PsConf3Register, PsSmartPersistenceState>(initialRegisterLowByte: InitialLowByte,
                                                       initialRegisterHighByte: UnmodifiedHighByte,
                                                       testValue: (PsSmartPersistenceState)smartPersistenceState,
                                                       expectedLowByte: expectedLowByte,
                                                       expectedHighByte: UnmodifiedHighByte,
                                                       commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                       registerPropertyName: nameof(PsConf3Register.PsSmartPers),
                                                       registerReadsBeforeWriting: true);

            PropertyWriteTest<PsConf3Register, PsSmartPersistenceState>(initialRegisterLowByte: InitialLowByteInv,
                                                       initialRegisterHighByte: UnmodifiedHighByteInv,
                                                       testValue: (PsSmartPersistenceState)smartPersistenceState,
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
            var reg = new PsConf3Register(new I2cTestDevice());
            Assert.Equal(PsSunlightCancellationState.Disabled, reg.PsScEn);
            Assert.Equal(PsActiveForceModeTrigger.NoTrigger, reg.PsTrig);
            Assert.Equal(PsSmartPersistenceState.Disabled, reg.PsSmartPers);
            Assert.Equal(PsMultiPulse.Pulse1, reg.PsMps);
        }
    }
}
