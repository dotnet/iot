// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/*
using Iot.Device.Vcnl4040.Common.Defnitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        [Theory]
        [InlineData(PsInterruptMode.Close)]
        [InlineData(PsInterruptMode.Away)]
        [InlineData(PsInterruptMode.CloseOrAway)]
        public void InterruptEnabled_Get(PsInterruptMode mode)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, (byte)PsInterruptMode.Disabled);
            Assert.False(vcnl4040.ProximitySensor.InterruptEnabled);
            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, (byte)mode);
            Assert.True(vcnl4040.ProximitySensor.InterruptEnabled);
        }

        [Theory]
        [InlineData(PsInterruptMode.Close)]
        [InlineData(PsInterruptMode.Away)]
        [InlineData(PsInterruptMode.CloseOrAway)]
        public void InterruptEnabled_Get(PsInterruptMode mode)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, (byte)PsInterruptMode.Disabled);
            Assert.False(vcnl4040.ProximitySensor.InterruptEnabled);
            _testDevice.SetMsb(CommandCode.PS_CONF_1_2, (byte)mode);
            Assert.True(vcnl4040.ProximitySensor.InterruptEnabled);
        }
    }
}
*/
