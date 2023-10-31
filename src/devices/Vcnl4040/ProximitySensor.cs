// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;
using UnitsNet;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProximitySensor"/> API class.
    /// </summary>
    public class ProximitySensor
    {
        private PsConf1Register _psConf1Register;
        private PsConf2Register _psConf2Register;
        private PsConf3Register _psConf3Register;
        private PsMsRegister _psMsRegister;
        private PsCancellationLevelRegister _psCancellationLevelRegister;
        private PsLowInterruptThresholdRegister _psLowInterruptThresholdRegister;
        private PsHighInterruptThresholdRegister _psHighInterruptThresholdRegister;
        private PsDataRegister _psDataRegister;
        private WhiteDataRegister _whiteDataRegister;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximitySensor"/> API class.
        /// </summary>
        public ProximitySensor(I2cInterface i2cBus)
        {
            _psConf1Register = new PsConf1Register(i2cBus);
            _psConf2Register = new PsConf2Register(i2cBus);
            _psConf3Register = new PsConf3Register(i2cBus);
            _psMsRegister = new PsMsRegister(i2cBus);
            _psCancellationLevelRegister = new PsCancellationLevelRegister(i2cBus);
            _psLowInterruptThresholdRegister = new PsLowInterruptThresholdRegister(i2cBus);
            _psHighInterruptThresholdRegister = new PsHighInterruptThresholdRegister(i2cBus);
            _psDataRegister = new PsDataRegister(i2cBus);
            _whiteDataRegister = new WhiteDataRegister(i2cBus);
        }
    }
}
