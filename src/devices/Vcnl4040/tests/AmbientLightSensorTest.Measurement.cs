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
        public void Reading_Get()
        {
            // choose 160 ms non-default integration time resulting in a resolution of 0.05 lux;
            double resolution = 0.05;
            AlsIntegrationTime integrationTime = AlsIntegrationTime.Time160ms;
            Illuminance refReading = Illuminance.FromLux(123.4);
            int registerValue = (int)(refReading.Lux * (1 / resolution));

            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            // set data directly as the register is readonly
            _testDevice.SetData(CommandCode.ALS_Data, registerValue);
            _alsConfRegister.AlsIt = integrationTime;
            WriteRegisters();

            Assert.Equal(refReading, vcnl4040.AmbientLightSensor.Illuminance);
        }

        [Fact]
        public void Reading_Get_LoadReductionMode()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);
            InjectTestRegister(vcnl4040.AmbientLightSensor);

            /*
             Approach:
               1)  Switch load reduction mode on
               2)  Set data register to 10000
               3)  set integration time using the property to 320 ms
               4)  change the integration time in the register to 160 ms
                   This WILL NOT update the local integration time in the binding,
                   hance the reading will still base on the local integration time.
               5)  check expected reading
                   Expect: 320 ms => resolution = 0.025 => 10000 * 0.025 => reading = 250
               6)  modify the integration time using the property to 80 ms
               7)  check if new reading reflects the change of the local integration time
                   Expect: 80 ms => resolution = 0.1 => 10000 * 0.1 => reading = 1000
               8)  Switch load reduction mode off
               9)  Change the integration time in the device to 640 ms
               10) check if new reading bases integration time in the device as expected
                   Expect: 640 ms => resolution = 0.0125 => 10000 * 0.0125 => reading = 125
            */

            // 1
            vcnl4040.AmbientLightSensor.LoadReductionModeEnabled = true;
            // 2
            _testDevice.SetData(CommandCode.ALS_Data, 10000);
            // 3
            vcnl4040.AmbientLightSensor.IntegrationTime = AlsIntegrationTime.Time320ms;
            // 4
            _testDevice.SetLsb(CommandCode.ALS_CONF, (byte)AlsIntegrationTime.Time160ms);
            // 5
            Assert.Equal(250, vcnl4040.AmbientLightSensor.Illuminance.Lux);
            // 6
            vcnl4040.AmbientLightSensor.IntegrationTime = AlsIntegrationTime.Time80ms;
            // 7
            Assert.Equal(1000, vcnl4040.AmbientLightSensor.Illuminance.Lux);
            // 8
            vcnl4040.AmbientLightSensor.LoadReductionModeEnabled = false;
            // 9
            _testDevice.SetData(CommandCode.ALS_CONF, (byte)AlsIntegrationTime.Time640ms);
            // 10
            Assert.Equal(125, vcnl4040.AmbientLightSensor.Illuminance.Lux);
        }
    }
}
