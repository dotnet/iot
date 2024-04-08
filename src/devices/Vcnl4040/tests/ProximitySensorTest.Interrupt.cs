// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Vcnl4040.Definitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        [Theory]
        [InlineData(1000, 2000, PsInterruptPersistence.Persistence1, false, ProximityInterruptMode.AwayInterrupt)]
        [InlineData(2000, 3000, PsInterruptPersistence.Persistence2, true, ProximityInterruptMode.CloseInterrupt)]
        [InlineData(3000, 4000, PsInterruptPersistence.Persistence3, false, ProximityInterruptMode.CloseOrAwayInterrupt)]
        [InlineData(4000, 5000, PsInterruptPersistence.Persistence4, true, ProximityInterruptMode.LogicOutput)]
        public void EnableInterrupt(ushort lowerThreshold, ushort upperThreshold, PsInterruptPersistence persistence, bool smartPersistenceEnabled, ProximityInterruptMode mode)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ProximityInterruptConfiguration configuration = new(LowerThreshold: lowerThreshold,
                                                                UpperThreshold: upperThreshold,
                                                                Persistence: persistence,
                                                                SmartPersistenceEnabled: smartPersistenceEnabled,
                                                                Mode: mode);

            // apply configuration and enable
            vcnl4040.ProximitySensor.EnableInterrupts(configuration);

            ReadBackRegisters();
            Assert.Equal(lowerThreshold, _psLowInterruptThresholdRegister.Level);
            Assert.Equal(upperThreshold, _psHighInterruptThresholdRegister.Level);
            Assert.Equal(persistence, _psConf1Register.PsPers);
            Assert.Equal(smartPersistenceEnabled ? PsSmartPersistenceState.Enabled : PsSmartPersistenceState.Disabled, _psConf3Register.PsSmartPers);
            Assert.Equal(mode == ProximityInterruptMode.LogicOutput ? PsProximityDetectionOutput.LogicOutput : PsProximityDetectionOutput.Interrupt, _psMsRegister.PsMs);
            Assert.Equal(mode switch
            {
                ProximityInterruptMode.Nothing => PsInterruptMode.Disabled,
                ProximityInterruptMode.CloseInterrupt => PsInterruptMode.Close,
                ProximityInterruptMode.AwayInterrupt => PsInterruptMode.Away,
                ProximityInterruptMode.CloseOrAwayInterrupt => PsInterruptMode.CloseOrAway,
                ProximityInterruptMode.LogicOutput => PsInterruptMode.CloseOrAway,
                _ => throw new NotImplementedException(),
            }, _psConf2Register.PsInt);
        }

        [Theory]
        [InlineData(1000, 2000, false)]
        [InlineData(1000, 1000, false)]
        [InlineData(2000, 1999, true)]
        public void EnableInterrupt_ThresholdCheck_Exception(ushort lowerThreshold, ushort upperThreshold, bool exceptionExpected)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ProximityInterruptConfiguration configuration = new(LowerThreshold: lowerThreshold,
                                                                UpperThreshold: upperThreshold,
                                                                Persistence: PsInterruptPersistence.Persistence1,
                                                                SmartPersistenceEnabled: false,
                                                                Mode: ProximityInterruptMode.CloseInterrupt);

            // apply configuration and enable
            if (exceptionExpected)
            {
                Assert.Throws<ArgumentException>(() => vcnl4040.ProximitySensor.EnableInterrupts(configuration));
            }
            else
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void EnableInterrupt_AlsInterruptInterference_Exception()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ProximityInterruptConfiguration configuration = new(LowerThreshold: 1000,
                                                                UpperThreshold: 2000,
                                                                Persistence: PsInterruptPersistence.Persistence1,
                                                                SmartPersistenceEnabled: false,
                                                                Mode: ProximityInterruptMode.LogicOutput);

            // fake that ALS interrupt is enabled
            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            WriteRegisters();

            Assert.Throws<InvalidOperationException>(() => vcnl4040.ProximitySensor.EnableInterrupts(configuration));
        }

        [Fact]
        public void InterruptEnabled()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ProximityInterruptConfiguration configuration = new(LowerThreshold: 1000,
                                                                UpperThreshold: 2000,
                                                                Persistence: PsInterruptPersistence.Persistence1,
                                                                SmartPersistenceEnabled: false,
                                                                Mode: ProximityInterruptMode.AwayInterrupt);

            ReadBackRegisters();
            Assert.Equal(PsInterruptMode.Disabled, _psConf2Register.PsInt);
            Assert.Equal(PsProximityDetectionOutput.Interrupt, _psMsRegister.PsMs);
            Assert.False(vcnl4040.ProximitySensor.IsInterruptEnabled);
            Assert.False(vcnl4040.ProximitySensor.IsLogicOutputEnabled);

            vcnl4040.ProximitySensor.EnableInterrupts(configuration);

            ReadBackRegisters();
            Assert.Equal(PsInterruptMode.Away, _psConf2Register.PsInt);
            Assert.Equal(PsProximityDetectionOutput.Interrupt, _psMsRegister.PsMs);
            Assert.True(vcnl4040.ProximitySensor.IsInterruptEnabled);
            Assert.False(vcnl4040.ProximitySensor.IsLogicOutputEnabled);
        }

        [Fact]
        public void LogicOutputEnabled()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ProximityInterruptConfiguration configuration = new(LowerThreshold: 1000,
                                                                UpperThreshold: 2000,
                                                                Persistence: PsInterruptPersistence.Persistence1,
                                                                SmartPersistenceEnabled: false,
                                                                Mode: ProximityInterruptMode.LogicOutput);

            ReadBackRegisters();
            Assert.Equal(PsInterruptMode.Disabled, _psConf2Register.PsInt);
            Assert.Equal(PsProximityDetectionOutput.Interrupt, _psMsRegister.PsMs);
            Assert.False(vcnl4040.ProximitySensor.IsInterruptEnabled);
            Assert.False(vcnl4040.ProximitySensor.IsLogicOutputEnabled);

            vcnl4040.ProximitySensor.EnableInterrupts(configuration);

            ReadBackRegisters();
            Assert.Equal(PsInterruptMode.CloseOrAway, _psConf2Register.PsInt);
            Assert.Equal(PsProximityDetectionOutput.LogicOutput, _psMsRegister.PsMs);
            Assert.True(vcnl4040.ProximitySensor.IsInterruptEnabled);
            Assert.True(vcnl4040.ProximitySensor.IsLogicOutputEnabled);
        }

        [Theory]
        [InlineData(1000, 2000, PsInterruptPersistence.Persistence1, false, ProximityInterruptMode.AwayInterrupt)]
        [InlineData(2000, 3000, PsInterruptPersistence.Persistence2, true, ProximityInterruptMode.CloseInterrupt)]
        [InlineData(3000, 4000, PsInterruptPersistence.Persistence3, false, ProximityInterruptMode.CloseOrAwayInterrupt)]
        [InlineData(4000, 5000, PsInterruptPersistence.Persistence4, true, ProximityInterruptMode.LogicOutput)]
        public void GetInterruptConfiguration(ushort lowerThreshold, ushort upperThreshold, PsInterruptPersistence persistence, bool smartPersistenceEnabled, ProximityInterruptMode mode)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            ProximityInterruptConfiguration referenceConfiguration = new(LowerThreshold: lowerThreshold,
                                                                UpperThreshold: upperThreshold,
                                                                Persistence: persistence,
                                                                SmartPersistenceEnabled: smartPersistenceEnabled,
                                                                Mode: mode);

            // apply configuration and enable
            vcnl4040.ProximitySensor.EnableInterrupts(referenceConfiguration);

            ProximityInterruptConfiguration retrievedConfiguration = vcnl4040.ProximitySensor.GetInterruptConfiguration();
            Assert.Equal(referenceConfiguration.LowerThreshold, retrievedConfiguration.LowerThreshold);
            Assert.Equal(referenceConfiguration.UpperThreshold, retrievedConfiguration.UpperThreshold);
            Assert.Equal(referenceConfiguration.Persistence, retrievedConfiguration.Persistence);
            Assert.Equal(referenceConfiguration.SmartPersistenceEnabled, retrievedConfiguration.SmartPersistenceEnabled);
            Assert.Equal(referenceConfiguration.Mode, retrievedConfiguration.Mode);
        }
    }
}
