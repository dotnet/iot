// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Reflection;
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class AmbientLightSensorTest
    {
        private AlsConfRegister _alsConfRegister;
        private AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private AlsDataRegister _alsDataRegister;
        private I2cTestDevice _testI2cDevice = new();
        private I2cInterface _testBus;

        private static FieldInfo GetFieldInfoOrThrow(object instance, string fieldName) =>
            instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? throw new Exception($"Coulnd't get field named '{fieldName}' for type '{instance.GetType().Name}'");

        public AmbientLightSensorTest()
        {
            _testBus = new(_testI2cDevice);
            _alsConfRegister = new(_testBus);
            _alsHighInterruptThresholdRegister = new(_testBus);
            _alsLowInterruptThresholdRegister = new(_testBus);
            _alsDataRegister = new(_testBus);
        }

        private void InjectTestRegister(AmbientLightSensor als)
        {
            GetFieldInfoOrThrow(als, "_alsConfRegister").SetValue(als, _alsConfRegister);
            GetFieldInfoOrThrow(als, "_alsHighInterruptThresholdRegister").SetValue(als, _alsHighInterruptThresholdRegister);
            GetFieldInfoOrThrow(als, "_alsLowInterruptThresholdRegister").SetValue(als, _alsLowInterruptThresholdRegister);
            GetFieldInfoOrThrow(als, "_alsDataRegister").SetValue(als, _alsDataRegister);
        }

        [Fact]
        public void PowerOn_Get()
        {
            Vcnl4040Device device = new(_testI2cDevice);
            InjectTestRegister(device.AmbientLightSensor);

            _testI2cDevice.DataToRead.Enqueue((byte)PowerState.PowerOff);
            _testI2cDevice.DataToRead.Enqueue(0x00);
            Assert.False(device.AmbientLightSensor.PowerOn);

            _testI2cDevice.DataToRead.Enqueue((byte)PowerState.PowerOn);
            _testI2cDevice.DataToRead.Enqueue(0x00);
            Assert.True(device.AmbientLightSensor.PowerOn);
        }

        [Fact]
        public void PowerOn_Set()
        {
            Vcnl4040Device device = new(_testI2cDevice);
            InjectTestRegister(device.AmbientLightSensor);

            _testI2cDevice.DataToRead.Enqueue((byte)PowerState.PowerOff);
            _testI2cDevice.DataToRead.Enqueue(0x00);
            device.AmbientLightSensor.PowerOn = true;
            Assert.Equal((byte)CommandCode.ALS_CONF, _testI2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)(0b0000_0000 | PowerState.PowerOn), _testI2cDevice.DataWritten.Dequeue());
            _ = _testI2cDevice.DataWritten.Dequeue();
        }
    }
}
