// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Definitions;
using UnitsNet;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class AmbientLightSensorTest
    {
        [Theory]
        [InlineData(AlsInterrupt.Disabled)]
        [InlineData(AlsInterrupt.Enabled)]
        public void InterruptEnabled_Get(AlsInterrupt state)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);
            _testDevice.SetLsb(CommandCode.ALS_CONF, (byte)state);

            Assert.Equal(state == AlsInterrupt.Enabled, vcnl4040.AmbientLightSensor.InterruptsEnabled);
        }

        [Fact]
        public void DisableInterrupts()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);
            _testDevice.SetLsb(CommandCode.ALS_CONF, (byte)AlsInterrupt.Enabled);

            Assert.True(vcnl4040.AmbientLightSensor.InterruptsEnabled);

            vcnl4040.AmbientLightSensor.DisableInterrupts();

            Assert.Equal((byte)AlsInterrupt.Disabled, _testDevice.GetLsb(CommandCode.ALS_CONF));
            Assert.False(vcnl4040.AmbientLightSensor.InterruptsEnabled);
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
            Assert.Equal(lowerThresholdCounts, _testDevice.GetData(CommandCode.ALS_THDL));
            Assert.Equal(upperThresholdCounts, _testDevice.GetData(CommandCode.ALS_THDH));
            // Persistence: bits 3:2
            Assert.Equal((byte)persistence, _testDevice.GetLsb(CommandCode.ALS_CONF) & 0b00001100);
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
        public void EnableInterrupts_ThresholdChecks(int lowerThreshold, int upperThreshold, bool throws)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            vcnl4040.AmbientLightSensor.IntegrationTime = AlsIntegrationTime.Time640ms;

            AmbientLightInterruptConfiguration configuration = new(LowerThreshold: Illuminance.FromLux(lowerThreshold),
                                                                   UpperThreshold: Illuminance.FromLux(upperThreshold),
                                                                   Persistence: AlsInterruptPersistence.Persistence2);
            if (throws)
            {
                Assert.ThrowsAny<Exception>(() => vcnl4040.AmbientLightSensor.EnableInterrupts(configuration));
            }
            else
            {
                vcnl4040.AmbientLightSensor.EnableInterrupts(configuration);
                Assert.Equal(AlsInterrupt.Enabled, _alsConfRegister.AlsIntEn);
            }
        }

        [Fact]
        public void GetInterruptConfiguration()
        {
            AlsIntegrationTime integrationTime = AlsIntegrationTime.Time80ms;

            int lowerThresholdLux = 1000;
            int upperThresholdLux = 2000;
            AlsInterruptPersistence persistence = AlsInterruptPersistence.Persistence1;

            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            int lowerThresholdCounts = (int)(lowerThresholdLux * (1 / vcnl4040.AmbientLightSensor.ResolutionAsIlluminance.Lux));
            int upperThresholdCounts = (int)(upperThresholdLux * (1 / vcnl4040.AmbientLightSensor.ResolutionAsIlluminance.Lux));
            _testDevice.SetData(CommandCode.ALS_THDL, lowerThresholdCounts);
            _testDevice.SetData(CommandCode.ALS_THDH, upperThresholdCounts);
            _testDevice.SetData(CommandCode.ALS_CONF, (byte)integrationTime | (byte)persistence);

            AmbientLightInterruptConfiguration configuration = vcnl4040.AmbientLightSensor.GetInterruptConfiguration();
            Assert.Equal(lowerThresholdLux, configuration.LowerThreshold.Lux);
            Assert.Equal(upperThresholdLux, configuration.UpperThreshold.Lux);
            Assert.Equal(persistence, configuration.Persistence);
        }
    }
}
