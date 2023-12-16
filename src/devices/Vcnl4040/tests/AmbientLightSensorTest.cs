// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Reflection;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class AmbientLightSensorTest
    {
        private readonly AlsConfRegister _alsConfRegister;
        private readonly AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private readonly AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private readonly AlsDataRegister _alsDataRegister;
        private readonly Vcnl4040TestDevice _testDevice = new();

        private static FieldInfo GetFieldInfoOrThrow(object instance, string fieldName) =>
            instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? throw new Exception($"Couldn't get field named '{fieldName}' for type '{instance.GetType().Name}'");

        public AmbientLightSensorTest()
        {
            _alsConfRegister = new(_testDevice);
            _alsHighInterruptThresholdRegister = new(_testDevice);
            _alsLowInterruptThresholdRegister = new(_testDevice);
            _alsDataRegister = new(_testDevice);

            _testDevice.SetData(CommandCode.ID, 0x0186);
        }

        private void InjectTestRegister(AmbientLightSensor als)
        {
            GetFieldInfoOrThrow(als, "_alsConfRegister").SetValue(als, _alsConfRegister);
            GetFieldInfoOrThrow(als, "_alsHighInterruptThresholdRegister").SetValue(als, _alsHighInterruptThresholdRegister);
            GetFieldInfoOrThrow(als, "_alsLowInterruptThresholdRegister").SetValue(als, _alsLowInterruptThresholdRegister);
            GetFieldInfoOrThrow(als, "_alsDataRegister").SetValue(als, _alsDataRegister);
        }

        private void ReadBackRegisters()
        {
            _alsConfRegister.Read();
            _alsHighInterruptThresholdRegister.Read();
            _alsLowInterruptThresholdRegister.Read();
            _alsDataRegister.Read();
        }

        private void WriteRegisters()
        {
            _alsConfRegister.Write();
            _alsHighInterruptThresholdRegister.Write();
            _alsLowInterruptThresholdRegister.Write();
        }
    }
}
