// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Common.Definitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        [Fact]
        public void GetEmitterConfiguration()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            EmitterConfiguration referenceConfiguration = new(PsLedCurrent.I180mA, PsDuty.Duty320, PsMultiPulse.Pulse4);

            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, (byte)referenceConfiguration.Current);
            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)referenceConfiguration.DutyRatio);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)referenceConfiguration.MultiPulses);

            EmitterConfiguration configuration = vcnl4040.ProximitySensor.GetEmitterConfiguration();
            Assert.Equal(referenceConfiguration.Current, configuration.Current);
            Assert.Equal(referenceConfiguration.DutyRatio, configuration.DutyRatio);
            Assert.Equal(referenceConfiguration.MultiPulses, configuration.MultiPulses);

            referenceConfiguration = new(PsLedCurrent.I120mA, PsDuty.Duty160, PsMultiPulse.Pulse2);

            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, (byte)referenceConfiguration.Current);
            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)referenceConfiguration.DutyRatio);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)referenceConfiguration.MultiPulses);

            EmitterConfiguration updatedConfiguration = vcnl4040.ProximitySensor.GetEmitterConfiguration();
            Assert.Equal(referenceConfiguration.Current, updatedConfiguration.Current);
            Assert.Equal(referenceConfiguration.DutyRatio, updatedConfiguration.DutyRatio);
            Assert.Equal(referenceConfiguration.MultiPulses, updatedConfiguration.MultiPulses);
        }

        [Fact]
        public void ConfigureEmitter()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            EmitterConfiguration initialConfiguration = new(Current: PsLedCurrent.I180mA,
                                                            DutyRatio: PsDuty.Duty160,
                                                            MultiPulses: PsMultiPulse.Pulse4);
            vcnl4040.ProximitySensor.ConfigureEmitter(initialConfiguration);

            Assert.Equal((byte)initialConfiguration.Current, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsLedI);
            Assert.Equal((byte)initialConfiguration.DutyRatio, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsDuty);
            Assert.Equal((byte)initialConfiguration.MultiPulses, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsMps);

            EmitterConfiguration reconfiguration = new(Current: PsLedCurrent.I160mA,
                                                       DutyRatio: PsDuty.Duty320,
                                                       MultiPulses: PsMultiPulse.Pulse2);
            vcnl4040.ProximitySensor.ConfigureEmitter(reconfiguration);

            Assert.Equal((byte)reconfiguration.Current, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsLedI);
            Assert.Equal((byte)reconfiguration.DutyRatio, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsDuty);
            Assert.Equal((byte)reconfiguration.MultiPulses, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsMps);
        }

        [Fact]
        public void GetReceiverConfiguration()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)PsIntegrationTime.Time3_0);
            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, (byte)BitMasks.PsHd);
            _testDevice.SetData(CommandCode.PS_CANC, 12345);
            // while channel disabled (bit = 1)
            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, (byte)BitMasks.WhiteEn);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)PsSunlightCancellationState.Enabled);

            ReceiverConfiguration configuration = vcnl4040.ProximitySensor.GetReceiverConfiguration();
            Assert.Equal(PsIntegrationTime.Time3_0, configuration.IntegrationTime);
            Assert.True(configuration.ExtendedOutputRange);
            Assert.Equal(12345, configuration.CancellationLevel);
            Assert.False(configuration.WhiteChannelEnabled);
            Assert.True(configuration.SunlightCancellationEnabled);

            // Output range is 12-bit (default, 0)
            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, 0);
            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)PsIntegrationTime.Time2_0);
            _testDevice.SetData(CommandCode.PS_CANC, 54321);
            // white channel enabled (bit = 0)
            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, 0);
            // sunlight cancellation disabled
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, 0);

            ReceiverConfiguration updatedConfiguration = vcnl4040.ProximitySensor.GetReceiverConfiguration();
            Assert.Equal(PsIntegrationTime.Time2_0, updatedConfiguration.IntegrationTime);
            Assert.False(updatedConfiguration.ExtendedOutputRange);
            Assert.Equal(54321, updatedConfiguration.CancellationLevel);
            Assert.True(updatedConfiguration.WhiteChannelEnabled);
            Assert.False(updatedConfiguration.SunlightCancellationEnabled);
        }

        [Fact]
        public void ConfigureReceiver()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ReceiverConfiguration initialConfiguration = new(IntegrationTime: PsIntegrationTime.Time2_0,
                                                             ExtendedOutputRange: true,
                                                             CancellationLevel: 12345,
                                                             WhiteChannelEnabled: true,
                                                             SunlightCancellationEnabled: false);

            vcnl4040.ProximitySensor.ConfigureReceiver(initialConfiguration);

            Assert.Equal((byte)PsIntegrationTime.Time2_0, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsIt);
            Assert.Equal((byte)BitMasks.PsHd, _testDevice.GetMsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsHd);
            Assert.Equal(12345, _testDevice.GetData(CommandCode.PS_CANC));
            Assert.Equal(0, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.WhiteEn);
            Assert.Equal(0, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsScEn);

            ReceiverConfiguration reConfiguration = new(IntegrationTime: PsIntegrationTime.Time2_5,
                                                        ExtendedOutputRange: false,
                                                        CancellationLevel: 54321,
                                                        WhiteChannelEnabled: false,
                                                        SunlightCancellationEnabled: true);

            vcnl4040.ProximitySensor.ConfigureReceiver(reConfiguration);

            Assert.Equal((byte)PsIntegrationTime.Time2_5, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsIt);
            Assert.Equal(0, _testDevice.GetMsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsHd);
            Assert.Equal(54321, _testDevice.GetData(CommandCode.PS_CANC));
            Assert.Equal((byte)BitMasks.WhiteEn, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.WhiteEn);
            Assert.Equal((byte)BitMasks.PsScEn, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsScEn);
        }

        [Fact]
        public void ActiveForceMode_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, 0);
            Assert.False(vcnl4040.ProximitySensor.ActiveForceMode);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)BitMasks.PsAf);
            Assert.True(vcnl4040.ProximitySensor.ActiveForceMode);
        }

        [Fact]
        public void ActiveForceMode_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            Assert.Equal(0, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsAf);
            vcnl4040.ProximitySensor.ActiveForceMode = true;
            Assert.Equal((byte)BitMasks.PsAf, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsAf);
            vcnl4040.ProximitySensor.ActiveForceMode = false;
            Assert.Equal(0, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsAf);
        }
    }
}
