// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Definitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        [Fact]
        public void Reading_Get_ActiveForceModeDisabled()
        {
            int refReading = 1234;

            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetData(CommandCode.PS_Data, refReading);

            ReadBackRegisters();
            Assert.Equal(PsActiveForceModeTrigger.NoTrigger, _psConf3Register.PsTrig);

            Assert.Equal(refReading, vcnl4040.ProximitySensor.Distance);

            ReadBackRegisters();
            Assert.Equal(PsActiveForceModeTrigger.NoTrigger, _psConf3Register.PsTrig);
        }

        [Fact]
        public void Reading_Get_ActiveForceModeEnabled()
        {
            int refReading = 1234;

            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);

            _testDevice.SetData(CommandCode.PS_Data, refReading);

            vcnl4040.ProximitySensor.ActiveForceMode = true;

            ReadBackRegisters();
            Assert.Equal(PsActiveForceModeTrigger.NoTrigger, _psConf3Register.PsTrig);

            Assert.Equal(refReading, vcnl4040.ProximitySensor.Distance);

            ReadBackRegisters();
            Assert.Equal(PsActiveForceModeTrigger.OneTimeCycle, _psConf3Register.PsTrig);
        }

        [Fact]
        public void WhiteChannel_Get()
        {
            int refReading = 5678;

            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.ProximitySensor);
            _testDevice.SetData(CommandCode.White_Data, refReading);

            Assert.Equal(refReading, vcnl4040.ProximitySensor.WhiteChannelReading);
        }
    }
}
