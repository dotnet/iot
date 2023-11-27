// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Definitions;
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

            _alsConfRegister.AlsSd = PowerState.PowerOff;
            WriteRegisters();
            Assert.False(vcnl4040.AmbientLightSensor.PowerOn);

            _alsConfRegister.AlsSd = PowerState.PowerOn;
            WriteRegisters();
            Assert.True(vcnl4040.AmbientLightSensor.PowerOn);
        }

        [Theory]
        [InlineData((byte)PowerState.PowerOff, (byte)PowerState.PowerOff)]
        [InlineData((byte)PowerState.PowerOff, (byte)PowerState.PowerOn)]
        [InlineData((byte)PowerState.PowerOn, (byte)PowerState.PowerOff)]
        [InlineData((byte)PowerState.PowerOn, (byte)PowerState.PowerOn)]
        public void PowerOn_Set(byte initialState, byte newState)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            _alsConfRegister.AlsSd = (PowerState)initialState;
            WriteRegisters();
            vcnl4040.AmbientLightSensor.PowerOn = (PowerState)newState == PowerState.PowerOn;
            ReadBackRegisters();
            Assert.Equal((PowerState)newState, _alsConfRegister.AlsSd);
        }
    }
}
