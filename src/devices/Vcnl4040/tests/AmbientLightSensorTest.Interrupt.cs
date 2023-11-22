// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;
using UnitsNet;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class AmbientLightSensorTest
    {
        [Theory]
        [InlineData((byte)AlsInterrupt.Disabled)]
        [InlineData((byte)AlsInterrupt.Enabled)]
        public void InterruptEnabled_Get(byte state)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIntEn = (AlsInterrupt)state;
            WriteRegisters();

            Assert.Equal((AlsInterrupt)state == AlsInterrupt.Enabled, vcnl4040.AmbientLightSensor.IsInterruptEnabled);
        }

        [Fact]
        public void DisableInterrupts()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            WriteRegisters();

            Assert.True(vcnl4040.AmbientLightSensor.IsInterruptEnabled);

            vcnl4040.AmbientLightSensor.DisableInterrupts();

            Assert.False(vcnl4040.AmbientLightSensor.IsInterruptEnabled);
            ReadBackRegisters();
            Assert.Equal(AlsInterrupt.Disabled, _alsConfRegister.AlsIntEn);
        }

        [Fact]
        public void EnableInterrupts_RegularConfiguration()
        {
            int lowerThresholdLux = 100;
            int upperThresholdLux = 200;
            AlsInterruptPersistence persistence = AlsInterruptPersistence.Persistence2;
            AlsIntegrationTime integrationTime = AlsIntegrationTime.Time320ms;

            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            /*
                Approach:
                    1) Set integration time 320 ms => resolution = 0.025
                    2) Enable interrupts with lower threshold 100 lux and upper threshold 200 lux,
                       and interrupt persistence 2
                    3) Check threshold in threshold registers
                       - expected lower threshold counts = 100 lux * 1/0.025 lux = 4000
                       - expected upper threshold counts = 200 lux * 1/0.025 lux = 8000
                    4) Check persistence in register
            */
            vcnl4040.AmbientLightSensor.IntegrationTime = integrationTime;
            AmbientLightInterruptConfiguration configuration = new(LowerThreshold: Illuminance.FromLux(lowerThresholdLux),
                                                                   UpperThreshold: Illuminance.FromLux(upperThresholdLux),
                                                                   Persistence: persistence);
            vcnl4040.AmbientLightSensor.EnableInterrupts(configuration);

            int lowerThresholdCounts = (int)(lowerThresholdLux * (1 / vcnl4040.AmbientLightSensor.ResolutionAsIlluminance.Lux));
            int upperThresholdCounts = (int)(upperThresholdLux * (1 / vcnl4040.AmbientLightSensor.ResolutionAsIlluminance.Lux));

            ReadBackRegisters();
            Assert.Equal(lowerThresholdCounts, _alsLowInterruptThresholdRegister.Level);
            Assert.Equal(upperThresholdCounts, _alsHighInterruptThresholdRegister.Level);
            Assert.Equal(persistence, _alsConfRegister.AlsPers);
        }

        [Theory]
        // Ok
        [InlineData(100, 200, false)]
        // Lower is higher than uppper
        [InlineData(200, 100, true)]
        // Lower exceeds range
        [InlineData(820, 821, true)]
        // Upper exceeds range
        [InlineData(0, 820, true)]
        // Lower is negative
        [InlineData(-1, 1, true)]
        // Upper is negative
        [InlineData(0, -1, true)]
        public void EnableInterrupts_ThresholdChecks_Exception(int lowerThreshold, int upperThreshold, bool throws)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            vcnl4040.AmbientLightSensor.IntegrationTime = AlsIntegrationTime.Time640ms;

            AmbientLightInterruptConfiguration configuration = new(LowerThreshold: Illuminance.FromLux(lowerThreshold),
                                                                   UpperThreshold: Illuminance.FromLux(upperThreshold),
                                                                   Persistence: AlsInterruptPersistence.Persistence2);
            if (throws)
            {
                Assert.ThrowsAny<ArgumentException>(() => vcnl4040.AmbientLightSensor.EnableInterrupts(configuration));
            }
            else
            {
                vcnl4040.AmbientLightSensor.EnableInterrupts(configuration);
                ReadBackRegisters();
                Assert.Equal(AlsInterrupt.Enabled, _alsConfRegister.AlsIntEn);
            }
        }

        [Fact]
        public void EnableInterrupts_PsLogicOutputInterference_Exception()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            AmbientLightInterruptConfiguration configuration = new(LowerThreshold: Illuminance.FromLux(0),
                                                                   UpperThreshold: Illuminance.FromLux(0),
                                                                   Persistence: AlsInterruptPersistence.Persistence1);

            PsMsRegister psMsRegister = new(_testDevice);
            psMsRegister.PsMs = PsProximityDetectionOutput.LogicOutput;
            psMsRegister.Write();

            Assert.ThrowsAny<InvalidOperationException>(() => vcnl4040.AmbientLightSensor.EnableInterrupts(configuration));
        }

        [Theory]
        [InlineData(1000, 2000, AlsInterruptPersistence.Persistence1)]
        [InlineData(2000, 3000, AlsInterruptPersistence.Persistence2)]
        public void GetInterruptConfiguration(int lowerThreshold, int upperThreshold, AlsInterruptPersistence persistence)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            AmbientLightInterruptConfiguration referenceConfiguration = new(LowerThreshold: Illuminance.FromLux(lowerThreshold),
                                                                            UpperThreshold: Illuminance.FromLux(upperThreshold),
                                                                            Persistence: persistence);

            vcnl4040.AmbientLightSensor.EnableInterrupts(referenceConfiguration);

            AmbientLightInterruptConfiguration retrievedConfiguration = vcnl4040.AmbientLightSensor.GetInterruptConfiguration();

            Assert.Equal(referenceConfiguration.LowerThreshold, retrievedConfiguration.LowerThreshold);
            Assert.Equal(referenceConfiguration.UpperThreshold, retrievedConfiguration.UpperThreshold);
            Assert.Equal(referenceConfiguration.Persistence, retrievedConfiguration.Persistence);
        }
    }
}
