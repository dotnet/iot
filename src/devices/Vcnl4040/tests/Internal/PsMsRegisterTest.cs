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
    public class PsMsRegisterTest : RegisterTest
    {
        [Theory]
        // LED_I
        [InlineData(0b0000_0000, PsLedCurrent.I50mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0001, PsLedCurrent.I75mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0010, PsLedCurrent.I100mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0011, PsLedCurrent.I120mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0100, PsLedCurrent.I140mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0101, PsLedCurrent.I160mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0110, PsLedCurrent.I180mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        [InlineData(0b0000_0111, PsLedCurrent.I200mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Enabled)]
        // PS_MS
        [InlineData(0b0100_0000, PsLedCurrent.I50mA, (byte)PsProximityDetectionOutput.LogicOutput, (byte)PsWhiteChannelState.Enabled)]
        // White_EN
        [InlineData(0b1000_0000, PsLedCurrent.I50mA, (byte)PsProximityDetectionOutput.Interrupt, (byte)PsWhiteChannelState.Disabled)]
        public void Read(byte registerData, PsLedCurrent current, byte outputMode, byte whiteChannel)
        {
            var testDevice = new I2cTestDevice();
            // low byte (not relevant)
            testDevice.DataToRead.Enqueue(0x00);
            // high byte
            testDevice.DataToRead.Enqueue(registerData);

            var reg = new PsMsRegister(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.PS_CONF_3_MS, testDevice.DataWritten.Dequeue());
            Assert.Equal(current, reg.LedI);
            Assert.Equal((PsProximityDetectionOutput)outputMode, reg.PsMs);
            Assert.Equal((PsWhiteChannelState)whiteChannel, reg.WhiteEn);
        }

        [Theory]
        [InlineData(PsLedCurrent.I50mA, 0b0000_0000)]
        [InlineData(PsLedCurrent.I75mA, 0b0000_0001)]
        [InlineData(PsLedCurrent.I100mA, 0b0000_0010)]
        [InlineData(PsLedCurrent.I120mA, 0b0000_0011)]
        [InlineData(PsLedCurrent.I140mA, 0b0000_0100)]
        [InlineData(PsLedCurrent.I160mA, 0b0000_0101)]
        [InlineData(PsLedCurrent.I180mA, 0b0000_0110)]
        [InlineData(PsLedCurrent.I200mA, 0b0000_0111)]
        public void Write_LedI(PsLedCurrent current, byte expectedHighByte)
        {
            const byte mask = 0b0000_0111;

            PropertyWriteTest<PsMsRegister, PsLedCurrent>(initialRegisterLowByte: UnmodifiedLowByte,
                                                           initialRegisterHighByte: InitialHighByte,
                                                           testValue: current,
                                                           expectedLowByte: UnmodifiedLowByte,
                                                           expectedHighByte: expectedHighByte,
                                                           commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                           registerPropertyName: nameof(PsMsRegister.LedI),
                                                           registerReadsBeforeWriting: true);

            PropertyWriteTest<PsMsRegister, PsLedCurrent>(initialRegisterLowByte: UnmodifiedLowByteInv,
                                                          initialRegisterHighByte: InitialHighByteInv,
                                                          testValue: current,
                                                          expectedLowByte: UnmodifiedLowByteInv,
                                                          expectedHighByte: (byte)(expectedHighByte | ~mask),
                                                          commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                          registerPropertyName: nameof(PsMsRegister.LedI),
                                                          registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData((byte)PsProximityDetectionOutput.Interrupt, 0b0000_0000)]
        [InlineData((byte)PsProximityDetectionOutput.LogicOutput, 0b0100_0000)]
        public void Write_PsMs(byte mode, byte expectedHighByte)
        {
            const byte mask = 0b0100_0000;

            PropertyWriteTest<PsMsRegister, PsProximityDetectionOutput>(initialRegisterLowByte: UnmodifiedLowByte,
                                                                            initialRegisterHighByte: InitialHighByte,
                                                                            testValue: (PsProximityDetectionOutput)mode,
                                                                            expectedLowByte: UnmodifiedLowByte,
                                                                            expectedHighByte: expectedHighByte,
                                                                            commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                            registerPropertyName: nameof(PsMsRegister.PsMs),
                                                                            registerReadsBeforeWriting: true);

            PropertyWriteTest<PsMsRegister, PsProximityDetectionOutput>(initialRegisterLowByte: UnmodifiedLowByteInv,
                                                                            initialRegisterHighByte: InitialHighByteInv,
                                                                            testValue: (PsProximityDetectionOutput)mode,
                                                                            expectedLowByte: UnmodifiedLowByteInv,
                                                                            expectedHighByte: (byte)(expectedHighByte | ~mask),
                                                                            commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                            registerPropertyName: nameof(PsMsRegister.PsMs),
                                                                            registerReadsBeforeWriting: true);
        }

        [Theory]
        [InlineData((byte)PsWhiteChannelState.Disabled, 0b1000_0000)]
        [InlineData((byte)PsWhiteChannelState.Enabled, 0b0000_0000)]
        public void Write_WhiteEn(byte state, byte expectedHighByte)
        {
            const byte mask = 0b1000_0000;

            PropertyWriteTest<PsMsRegister, PsWhiteChannelState>(initialRegisterLowByte: UnmodifiedLowByte,
                                                                 initialRegisterHighByte: InitialHighByte,
                                                                 testValue: (PsWhiteChannelState)state,
                                                                 expectedLowByte: UnmodifiedLowByte,
                                                                 expectedHighByte: expectedHighByte,
                                                                 commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                 registerPropertyName: nameof(PsMsRegister.WhiteEn),
                                                                 registerReadsBeforeWriting: true);

            PropertyWriteTest<PsMsRegister, PsWhiteChannelState>(initialRegisterLowByte: UnmodifiedLowByteInv,
                                                                 initialRegisterHighByte: InitialHighByteInv,
                                                                 testValue: (PsWhiteChannelState)state,
                                                                 expectedLowByte: UnmodifiedLowByteInv,
                                                                 expectedHighByte: (byte)(expectedHighByte | ~mask),
                                                                 commandCode: (byte)CommandCode.PS_CONF_3_MS,
                                                                 registerPropertyName: nameof(PsMsRegister.WhiteEn),
                                                                 registerReadsBeforeWriting: true);
        }

        [Fact]
        public void CheckRegisterDefaults()
        {
            var reg = new PsMsRegister(new I2cTestDevice());
            Assert.Equal(PsLedCurrent.I50mA, reg.LedI);
            Assert.Equal(PsProximityDetectionOutput.Interrupt, reg.PsMs);
            Assert.Equal(PsWhiteChannelState.Enabled, reg.WhiteEn);
        }
    }
}
