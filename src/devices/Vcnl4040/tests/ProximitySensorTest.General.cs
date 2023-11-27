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
        [InlineData((byte)PowerState.PowerOff, (byte)PowerState.PowerOff)]
        [InlineData((byte)PowerState.PowerOff, (byte)PowerState.PowerOn)]
        [InlineData((byte)PowerState.PowerOn, (byte)PowerState.PowerOff)]
        [InlineData((byte)PowerState.PowerOn, (byte)PowerState.PowerOn)]
        public void PowerOn_Set(byte initialState, byte newState)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetLsb(CommandCode.PS_CONF_1_2, (byte)initialState);
            vcnl4040.ProximitySensor.PowerOn = (PowerState)newState == PowerState.PowerOn;
            ReadBackRegisters();
            Assert.Equal((PowerState)newState, _psConf1Register.PsSd);
        }
    }
}
