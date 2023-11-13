// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Reflection;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;

namespace Iot.Device.Vcnl4040.Tests
{
    public partial class ProximitySensorTest
    {
        private readonly PsConf1Register _psConf1Register;
        private readonly PsConf2Register _psConf2Register;
        private readonly PsConf3Register _psConf3Register;
        private readonly PsMsRegister _psMsRegister;
        private readonly PsCancellationLevelRegister _psCancellationLevelRegister;
        private readonly PsLowInterruptThresholdRegister _psLowInterruptThresholdRegister;
        private readonly PsHighInterruptThresholdRegister _psHighInterruptThresholdRegister;
        private readonly PsDataRegister _psDataRegister;
        private readonly WhiteDataRegister _psWhiteDataRegister;
        private readonly AlsConfRegister _alsConfRegister;
        private readonly Vcnl4040TestDevice _testDevice = new();
        private readonly I2cInterface _testBus;

        private static FieldInfo GetFieldInfoOrThrow(object instance, string fieldName) =>
            instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? throw new Exception($"Coulnd't get field named '{fieldName}' for type '{instance.GetType().Name}'");

        public ProximitySensorTest()
        {
            _testBus = new(_testDevice);
            _psConf1Register = new(_testBus);
            _psConf2Register = new(_testBus);
            _psConf3Register = new(_testBus);
            _psMsRegister = new(_testBus);
            _psCancellationLevelRegister = new(_testBus);
            _psLowInterruptThresholdRegister = new(_testBus);
            _psHighInterruptThresholdRegister = new(_testBus);
            _psDataRegister = new(_testBus);
            _psWhiteDataRegister = new(_testBus);
            _alsConfRegister = new(_testBus);
        }

        private void InjectTestRegister(ProximitySensor ps)
        {
            GetFieldInfoOrThrow(ps, "_alsConfRegister").SetValue(ps, _alsConfRegister);
            GetFieldInfoOrThrow(ps, "_psConf1Register").SetValue(ps, _psConf1Register);
            GetFieldInfoOrThrow(ps, "_psConf2Register").SetValue(ps, _psConf2Register);
            GetFieldInfoOrThrow(ps, "_psConf3Register").SetValue(ps, _psConf3Register);
            GetFieldInfoOrThrow(ps, "_psMsRegister").SetValue(ps, _psMsRegister);
            GetFieldInfoOrThrow(ps, "_psCancellationLevelRegister").SetValue(ps, _psCancellationLevelRegister);
            GetFieldInfoOrThrow(ps, "_psLowInterruptThresholdRegister").SetValue(ps, _psLowInterruptThresholdRegister);
            GetFieldInfoOrThrow(ps, "_psHighInterruptThresholdRegister").SetValue(ps, _psHighInterruptThresholdRegister);
            GetFieldInfoOrThrow(ps, "_psDataRegister").SetValue(ps, _psDataRegister);
            GetFieldInfoOrThrow(ps, "_psWhiteDataRegister").SetValue(ps, _psWhiteDataRegister);
        }
    }
}
