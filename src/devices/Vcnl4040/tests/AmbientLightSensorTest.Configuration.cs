// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Definitions;
using UnitsNet;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class AmbientLightSensorTest
    {
        [Fact]
        public void IntegrationTime_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIt = AlsIntegrationTime.Time160ms;
            WriteRegisters();

            Assert.Equal(AlsIntegrationTime.Time160ms, vcnl4040.AmbientLightSensor.IntegrationTime);
        }

        [Fact]
        public void IntegrationTime_Set()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            /*
              1) set initial integration time to 160 ms
              2) set interrupts enabled, to be able to check implicit deactivation
              3) set integration time using property to 320 ms
              4) do register read back from device to assure they have actually been written
              5) check integration time in register
              6) check disabled interrupts
            */

            // 1 & 2
            _alsConfRegister.AlsIt = AlsIntegrationTime.Time160ms;
            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            WriteRegisters();
            // 3
            vcnl4040.AmbientLightSensor.IntegrationTime = AlsIntegrationTime.Time320ms;
            // 4
            ReadBackRegisters();
            // 5
            Assert.Equal(AlsIntegrationTime.Time320ms, _alsConfRegister.AlsIt);
            // 6
            Assert.Equal(AlsInterrupt.Disabled, _alsConfRegister.AlsIntEn);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, AlsRange.Range6553)]
        [InlineData(AlsIntegrationTime.Time160ms, AlsRange.Range3276)]
        [InlineData(AlsIntegrationTime.Time320ms, AlsRange.Range1638)]
        [InlineData(AlsIntegrationTime.Time640ms, AlsRange.Range819)]
        public void Range_Get(AlsIntegrationTime integrationTime, AlsRange range)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIt = integrationTime;
            WriteRegisters();

            Assert.Equal(range, vcnl4040.AmbientLightSensor.Range);
            // bonus check
            Assert.Equal(integrationTime, vcnl4040.AmbientLightSensor.IntegrationTime);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, 6553.5)]
        [InlineData(AlsIntegrationTime.Time160ms, 3276.8)]
        [InlineData(AlsIntegrationTime.Time320ms, 1638.4)]
        [InlineData(AlsIntegrationTime.Time640ms, 819.2)]
        public void RangeAsIlluminance_Get(AlsIntegrationTime integrationTime, double range)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIt = integrationTime;
            WriteRegisters();

            Assert.Equal(Illuminance.FromLux(range), vcnl4040.AmbientLightSensor.RangeAsIlluminance);
            // bonus check
            Assert.Equal(integrationTime, vcnl4040.AmbientLightSensor.IntegrationTime);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, AlsRange.Range6553)]
        [InlineData(AlsIntegrationTime.Time160ms, AlsRange.Range3276)]
        [InlineData(AlsIntegrationTime.Time320ms, AlsRange.Range1638)]
        [InlineData(AlsIntegrationTime.Time640ms, AlsRange.Range819)]
        public void Range_Set(AlsIntegrationTime integrationTime, AlsRange range)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            vcnl4040.AmbientLightSensor.Range = range;

            ReadBackRegisters();
            Assert.Equal(integrationTime, _alsConfRegister.AlsIt);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, AlsResolution.Resolution_0_1)]
        [InlineData(AlsIntegrationTime.Time160ms, AlsResolution.Resolution_0_05)]
        [InlineData(AlsIntegrationTime.Time320ms, AlsResolution.Resolution_0_025)]
        [InlineData(AlsIntegrationTime.Time640ms, AlsResolution.Resolution_0_0125)]
        public void Resolution_Get(AlsIntegrationTime integrationTime, AlsResolution resolution)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIt = integrationTime;
            WriteRegisters();

            Assert.Equal(resolution, vcnl4040.AmbientLightSensor.Resolution);
            // bonus check
            Assert.Equal(integrationTime, vcnl4040.AmbientLightSensor.IntegrationTime);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, 0.1)]
        [InlineData(AlsIntegrationTime.Time160ms, 0.05)]
        [InlineData(AlsIntegrationTime.Time320ms, 0.025)]
        [InlineData(AlsIntegrationTime.Time640ms, 0.0125)]
        public void ResolutionAsIlluminance_Get(AlsIntegrationTime integrationTime, double resolution)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsIt = integrationTime;
            WriteRegisters();

            Assert.Equal(Illuminance.FromLux(resolution), vcnl4040.AmbientLightSensor.ResolutionAsIlluminance);
            // bonus check
            Assert.Equal(integrationTime, vcnl4040.AmbientLightSensor.IntegrationTime);
        }

        [Theory]
        [InlineData(AlsIntegrationTime.Time80ms, AlsResolution.Resolution_0_1)]
        [InlineData(AlsIntegrationTime.Time160ms, AlsResolution.Resolution_0_05)]
        [InlineData(AlsIntegrationTime.Time320ms, AlsResolution.Resolution_0_025)]
        [InlineData(AlsIntegrationTime.Time640ms, AlsResolution.Resolution_0_0125)]
        public void Resolution_Set(AlsIntegrationTime integrationTime, AlsResolution resolution)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            vcnl4040.AmbientLightSensor.Resolution = resolution;

            ReadBackRegisters();
            Assert.Equal(integrationTime, _alsConfRegister.AlsIt);
        }
    }
}
