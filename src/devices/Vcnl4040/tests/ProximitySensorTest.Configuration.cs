// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Common.Defnitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        [Fact]
        public void DutyRatio_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)PsDuty.Duty320);
            Assert.Equal(PsDuty.Duty320, vcnl4040.ProximitySensor.DutyRatio);
        }

        [Fact]
        public void DutyRatio_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            vcnl4040.ProximitySensor.DutyRatio = PsDuty.Duty160;
            Assert.Equal((byte)PsDuty.Duty160, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsDuty);
            vcnl4040.ProximitySensor.DutyRatio = PsDuty.Duty320;
            Assert.Equal((byte)PsDuty.Duty320, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsDuty);
        }

        [Fact]
        public void LedCurrent_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, (byte)PsLedCurrent.I180mA);
            Assert.Equal(PsLedCurrent.I180mA, vcnl4040.ProximitySensor.LedCurrent);
        }

        [Fact]
        public void LedCurrent_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            vcnl4040.ProximitySensor.LedCurrent = PsLedCurrent.I180mA;
            Assert.Equal((byte)PsLedCurrent.I180mA, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsLedI);
            vcnl4040.ProximitySensor.LedCurrent = PsLedCurrent.I160mA;
            Assert.Equal((byte)PsLedCurrent.I160mA, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsLedI);
        }

        [Fact]
        public void IntegrationTime_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)PsIntegrationTime.Time3_0);
            Assert.Equal(PsIntegrationTime.Time3_0, vcnl4040.ProximitySensor.IntegrationTime);
        }

        [Fact]
        public void IntegrationTime_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            vcnl4040.ProximitySensor.IntegrationTime = PsIntegrationTime.Time2_0;
            Assert.Equal((byte)PsIntegrationTime.Time2_0, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsIt);
            vcnl4040.ProximitySensor.IntegrationTime = PsIntegrationTime.Time2_5;
            Assert.Equal((byte)PsIntegrationTime.Time2_5, _testDevice.GetLsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsIt);
        }

        [Fact]
        public void ExtendedOutputRange_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            Assert.False(vcnl4040.ProximitySensor.ExtendedOutputRange);
            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, (byte)BitMasks.PsHd);
            Assert.True(vcnl4040.ProximitySensor.ExtendedOutputRange);
        }

        [Fact]
        public void ExtendedOutputRange_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            Assert.Equal(0, _testDevice.GetMsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsHd);
            vcnl4040.ProximitySensor.ExtendedOutputRange = true;
            Assert.Equal((byte)BitMasks.PsHd, _testDevice.GetMsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsHd);
            vcnl4040.ProximitySensor.ExtendedOutputRange = false;
            Assert.Equal(0, _testDevice.GetMsb(CommandCode.PS_CONF_1_2) & (byte)BitMasks.PsHd);
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

        [Fact]
        public void WhiteChannelEnabled_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            // note: 0 = enabled, 1 = disabled
            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, (byte)BitMasks.WhiteEn);
            Assert.False(vcnl4040.ProximitySensor.WhiteChannelEnabled);
            _testDevice.SetMsb(CommandCode.PS_CONF_3_MS, 0);
            Assert.True(vcnl4040.ProximitySensor.WhiteChannelEnabled);
        }

        [Fact]
        public void WhiteChannelEnabled_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            Assert.Equal(0, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.WhiteEn);
            vcnl4040.ProximitySensor.WhiteChannelEnabled = false;
            Assert.Equal((byte)BitMasks.WhiteEn, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.WhiteEn);
            vcnl4040.ProximitySensor.WhiteChannelEnabled = true;
            Assert.Equal(0, _testDevice.GetMsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.WhiteEn);
        }

        [Fact]
        public void MultiPulses_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)PsMultiPulse.Pulse4);
            Assert.Equal(PsMultiPulse.Pulse4, vcnl4040.ProximitySensor.MultiPulses);
        }

        [Fact]
        public void MultiPulses_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            Assert.Equal((byte)PsMultiPulse.Pulse1, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsMps);
            vcnl4040.ProximitySensor.MultiPulses = PsMultiPulse.Pulse2;
            Assert.Equal((byte)PsMultiPulse.Pulse2, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsMps);
        }

        [Fact]
        public void SunlightCancellationEnabled_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)PsSunlightCancellationState.Disabled);
            Assert.False(vcnl4040.ProximitySensor.SunlightCancellationEnabled);
            _testDevice.SetLsb(CommandCode.PS_CONF_3_MS, (byte)PsSunlightCancellationState.Enabled);
            Assert.True(vcnl4040.ProximitySensor.SunlightCancellationEnabled);
        }

        [Fact]
        public void SunlightCancellationEnabled_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            vcnl4040.ProximitySensor.SunlightCancellationEnabled = false;
            Assert.Equal(0, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsScEn);
            vcnl4040.ProximitySensor.SunlightCancellationEnabled = true;
            Assert.Equal((byte)BitMasks.PsScEn, _testDevice.GetLsb(CommandCode.PS_CONF_3_MS) & (byte)BitMasks.PsScEn);
        }

        [Fact]
        public void Cancellation_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetData(CommandCode.PS_CANC, 12345);
            Assert.Equal(12345, vcnl4040.ProximitySensor.CancellationLevel);
        }

        [Fact]
        public void Cancellation_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            vcnl4040.ProximitySensor.CancellationLevel = 13579;
            Assert.Equal(13579, _testDevice.GetData(CommandCode.PS_CANC));
        }
    }
}
