// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;
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

            EmitterConfiguration referenceConfiguration = new(PsLedCurrent.I180mA, PsDuty.Duty320, PsIntegrationTime.Time3_0, PsMultiPulse.Pulse4);

            _psMsRegister.LedI = referenceConfiguration.Current;
            _psConf1Register.PsDuty = referenceConfiguration.DutyRatio;
            _psConf1Register.PsIt = referenceConfiguration.IntegrationTime;
            _psConf3Register.PsMps = referenceConfiguration.MultiPulses;
            WriteRegisters();

            EmitterConfiguration configuration = vcnl4040.ProximitySensor.GetEmitterConfiguration();
            Assert.Equal(referenceConfiguration.Current, configuration.Current);
            Assert.Equal(referenceConfiguration.DutyRatio, configuration.DutyRatio);
            Assert.Equal(referenceConfiguration.IntegrationTime, configuration.IntegrationTime);
            Assert.Equal(referenceConfiguration.MultiPulses, configuration.MultiPulses);

            referenceConfiguration = new(PsLedCurrent.I120mA, PsDuty.Duty160, PsIntegrationTime.Time2_0, PsMultiPulse.Pulse2);

            _psMsRegister.LedI = referenceConfiguration.Current;
            _psConf1Register.PsDuty = referenceConfiguration.DutyRatio;
            _psConf1Register.PsIt = referenceConfiguration.IntegrationTime;
            _psConf3Register.PsMps = referenceConfiguration.MultiPulses;
            WriteRegisters();

            EmitterConfiguration updatedConfiguration = vcnl4040.ProximitySensor.GetEmitterConfiguration();
            Assert.Equal(referenceConfiguration.Current, updatedConfiguration.Current);
            Assert.Equal(referenceConfiguration.DutyRatio, updatedConfiguration.DutyRatio);
            Assert.Equal(referenceConfiguration.IntegrationTime, updatedConfiguration.IntegrationTime);
            Assert.Equal(referenceConfiguration.MultiPulses, updatedConfiguration.MultiPulses);
        }

        [Fact]
        public void ConfigureEmitter()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            EmitterConfiguration initialConfiguration = new(Current: PsLedCurrent.I180mA,
                                                            DutyRatio: PsDuty.Duty160,
                                                            IntegrationTime: PsIntegrationTime.Time2_0,
                                                            MultiPulses: PsMultiPulse.Pulse4);
            vcnl4040.ProximitySensor.ConfigureEmitter(initialConfiguration);

            ReadBackRegisters();
            Assert.Equal(initialConfiguration.Current, _psMsRegister.LedI);
            Assert.Equal(initialConfiguration.DutyRatio, _psConf1Register.PsDuty);
            Assert.Equal(initialConfiguration.IntegrationTime, _psConf1Register.PsIt);
            Assert.Equal(initialConfiguration.MultiPulses, _psConf3Register.PsMps);

            EmitterConfiguration reconfiguration = new(Current: PsLedCurrent.I160mA,
                                                       DutyRatio: PsDuty.Duty320,
                                                       IntegrationTime: PsIntegrationTime.Time2_5,
                                                       MultiPulses: PsMultiPulse.Pulse2);
            vcnl4040.ProximitySensor.ConfigureEmitter(reconfiguration);

            ReadBackRegisters();
            Assert.Equal(reconfiguration.Current, _psMsRegister.LedI);
            Assert.Equal(reconfiguration.DutyRatio, _psConf1Register.PsDuty);
            Assert.Equal(reconfiguration.IntegrationTime, _psConf1Register.PsIt);
            Assert.Equal(reconfiguration.MultiPulses, _psConf3Register.PsMps);
        }

        [Fact]
        public void GetReceiverConfiguration()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _psConf1Register.PsIt = PsIntegrationTime.Time3_0;
            _psConf2Register.PsHd = PsOutputRange.Bits16;
            _psCancellationLevelRegister.Level = 12345;
            _psMsRegister.WhiteEn = PsWhiteChannelState.Disabled;
            _psConf3Register.PsScEn = PsSunlightCancellationState.Enabled;
            WriteRegisters();

            ReceiverConfiguration configuration = vcnl4040.ProximitySensor.GetReceiverConfiguration();
            Assert.True(configuration.ExtendedOutputRange);
            Assert.Equal(12345, configuration.CancellationLevel);
            Assert.False(configuration.WhiteChannelEnabled);
            Assert.True(configuration.SunlightCancellationEnabled);

            _psConf1Register.PsIt = PsIntegrationTime.Time2_0;
            _psConf2Register.PsHd = PsOutputRange.Bits12;
            _psCancellationLevelRegister.Level = 54321;
            _psMsRegister.WhiteEn = PsWhiteChannelState.Enabled;
            _psConf3Register.PsScEn = PsSunlightCancellationState.Disabled;
            WriteRegisters();

            ReceiverConfiguration updatedConfiguration = vcnl4040.ProximitySensor.GetReceiverConfiguration();
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

            ReceiverConfiguration initialConfiguration = new(ExtendedOutputRange: true,
                                                             CancellationLevel: 12345,
                                                             WhiteChannelEnabled: true,
                                                             SunlightCancellationEnabled: false);

            vcnl4040.ProximitySensor.ConfigureReceiver(initialConfiguration);

            ReadBackRegisters();
            Assert.Equal(PsOutputRange.Bits16, _psConf2Register.PsHd);
            Assert.Equal(12345, _psCancellationLevelRegister.Level);
            Assert.Equal(PsWhiteChannelState.Enabled, _psMsRegister.WhiteEn);
            Assert.Equal(PsSunlightCancellationState.Disabled, _psConf3Register.PsScEn);

            ReceiverConfiguration reConfiguration = new(ExtendedOutputRange: false,
                                                        CancellationLevel: 54321,
                                                        WhiteChannelEnabled: false,
                                                        SunlightCancellationEnabled: true);

            vcnl4040.ProximitySensor.ConfigureReceiver(reConfiguration);

            ReadBackRegisters();
            Assert.Equal(PsOutputRange.Bits12, _psConf2Register.PsHd);
            Assert.Equal(54321, _psCancellationLevelRegister.Level);
            Assert.Equal(PsWhiteChannelState.Disabled, _psMsRegister.WhiteEn);
            Assert.Equal(PsSunlightCancellationState.Enabled, _psConf3Register.PsScEn);
        }

        [Fact]
        public void ActiveForceMode_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _psConf3Register.PsAf = PsActiveForceMode.Disabled;
            WriteRegisters();

            Assert.False(vcnl4040.ProximitySensor.ActiveForceMode);

            _psConf3Register.PsAf = PsActiveForceMode.Enabled;
            WriteRegisters();

            Assert.True(vcnl4040.ProximitySensor.ActiveForceMode);
        }

        [Fact]
        public void ActiveForceMode_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ReadBackRegisters();
            Assert.Equal(PsActiveForceMode.Disabled, _psConf3Register.PsAf);

            vcnl4040.ProximitySensor.ActiveForceMode = true;

            ReadBackRegisters();
            Assert.Equal(PsActiveForceMode.Enabled, _psConf3Register.PsAf);

            vcnl4040.ProximitySensor.ActiveForceMode = false;

            ReadBackRegisters();
            Assert.Equal(PsActiveForceMode.Disabled, _psConf3Register.PsAf);
        }
    }
}
