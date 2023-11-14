// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Common.Definitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class AmbientLightSensorTest
    {
        [Fact]
        public void PowerOn_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _testDevice.SetLsb(CommandCode.ALS_CONF, (byte)PowerState.PowerOff);
            Assert.False(vcnl4040.AmbientLightSensor.PowerOn);

            _testDevice.SetLsb(CommandCode.ALS_CONF, (byte)PowerState.PowerOn);
            Assert.True(vcnl4040.AmbientLightSensor.PowerOn);
        }

        [Theory]
        [InlineData(PowerState.PowerOff, PowerState.PowerOff)]
        [InlineData(PowerState.PowerOff, PowerState.PowerOn)]
        [InlineData(PowerState.PowerOn, PowerState.PowerOff)]
        [InlineData(PowerState.PowerOn, PowerState.PowerOn)]
        public void PowerOn_Set(PowerState initialState, PowerState newState)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _testDevice.SetLsb(CommandCode.ALS_CONF, (byte)initialState);
            vcnl4040.AmbientLightSensor.PowerOn = newState == PowerState.PowerOn;
            Assert.Equal((byte)newState, _testDevice.GetLsb(CommandCode.ALS_CONF));
        }
    }
}
