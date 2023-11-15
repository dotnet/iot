// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Definitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        [Fact]
        public void PowerOn_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)PowerState.PowerOff);
            Assert.False(vcnl4040.ProximitySensor.PowerOn);

            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)PowerState.PowerOn);
            Assert.True(vcnl4040.ProximitySensor.PowerOn);
        }

        [Theory]
        [InlineData(PowerState.PowerOff, PowerState.PowerOff)]
        [InlineData(PowerState.PowerOff, PowerState.PowerOn)]
        [InlineData(PowerState.PowerOn, PowerState.PowerOff)]
        [InlineData(PowerState.PowerOn, PowerState.PowerOn)]
        public void PowerOn_Set(PowerState initialState, PowerState newState)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)initialState);
            vcnl4040.ProximitySensor.PowerOn = newState == PowerState.PowerOn;
            Assert.Equal((byte)newState, _testDevice.GetLsb(CommandCode.PS_CONF_1_2));
        }
    }
}
