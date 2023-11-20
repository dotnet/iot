// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Reflection;
using Iot.Device.Vcnl4040.Definitions;
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

        private static FieldInfo GetFieldInfoOrThrow(object instance, string fieldName) =>
            instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? throw new Exception($"Coulnd't get field named '{fieldName}' for type '{instance.GetType().Name}'");

        public ProximitySensorTest()
        {
            _psConf1Register = new(_testDevice);
            _psConf2Register = new(_testDevice);
            _psConf3Register = new(_testDevice);
            _psMsRegister = new(_testDevice);
            _psCancellationLevelRegister = new(_testDevice);
            _psLowInterruptThresholdRegister = new(_testDevice);
            _psHighInterruptThresholdRegister = new(_testDevice);
            _psDataRegister = new(_testDevice);
            _psWhiteDataRegister = new(_testDevice);
            _alsConfRegister = new(_testDevice);

            _testDevice.SetData(CommandCode.ID, 0x0186);
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

        private void ReadBackRegisters()
        {
            _psConf1Register.Read();
            _psConf2Register.Read();
            _psConf3Register.Read();
            _psMsRegister.Read();
            _psCancellationLevelRegister.Read();
            _psLowInterruptThresholdRegister.Read();
            _psHighInterruptThresholdRegister.Read();
            _psDataRegister.Read();
            _psWhiteDataRegister.Read();
            _alsConfRegister.Read();
        }

        private void WriteRegisters()
        {
            _psConf1Register.Write();
            _psConf2Register.Write();
            _psConf3Register.Write();
            _psMsRegister.Write();
            _psCancellationLevelRegister.Write();
            _psLowInterruptThresholdRegister.Write();
            _psHighInterruptThresholdRegister.Write();
            _alsConfRegister.Write();
        }
    }
}
