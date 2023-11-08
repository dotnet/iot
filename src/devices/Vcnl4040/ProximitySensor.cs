// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Common.Defnitions;
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
        private readonly PsConf1Register _psConf1Register;
        private readonly PsConf2Register _psConf2Register;
        private readonly PsConf3Register _psConf3Register;
        private readonly PsMsRegister _psMsRegister;
        private readonly PsCancellationLevelRegister _psCancellationLevelRegister;
        private readonly PsLowInterruptThresholdRegister _psLowInterruptThresholdRegister;
        private readonly PsHighInterruptThresholdRegister _psHighInterruptThresholdRegister;
        private readonly PsDataRegister _psDataRegister;
        private readonly WhiteDataRegister _whiteDataRegister;
        private bool _activeForceModeEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximitySensor"/> API class.
        /// </summary>
        internal ProximitySensor(I2cInterface i2cBus)
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

        #region General

        /// <summary>
        /// Attaches the binding instance to an already operating device.
        /// </summary>
        public void Attach()
        {
            _activeForceModeEnabled = ActiveForceMode;
        }

        /// <summary>
        /// Get or sets the power state (power on, shutdown) of the proximity sensor.
        /// </summary>
        public bool PowerOn
        {
            get
            {
                _psConf1Register.Read();
                return _psConf1Register.PsSd == PowerState.PowerOn;
            }

            set
            {
                _psConf1Register.Read();
                if ((value && _psConf1Register.PsSd == PowerState.PowerOn)
                || (!value && _psConf1Register.PsSd == PowerState.PowerOff))
                {
                    return;
                }

                _psConf1Register.PsSd = value ? PowerState.PowerOn : PowerState.PowerOff;
                _psConf1Register.Write();
            }
        }
        #endregion

        #region Measurement

        /// <summary>
        /// Gets the current proximity sensor reading.
        /// Important: if the sensor is in force mode a proximity measurement is triggered first.
        ///            Depending on the integration time this takes a while.
        /// </summary>
        public int Reading
        {
            get
            {
                if (_activeForceModeEnabled)
                {
                    _psConf3Register.Read();
                    _psConf3Register.PsTrig = PsActiveForceModeTrigger.OneTimeCycle;
                    _psConf3Register.Write();
                }

                _psDataRegister.Read();
                return _psDataRegister.Data;
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Gets or sets the IR LED duty ratio.
        /// </summary>
        public PsDuty DutyRatio
        {
            get
            {
                _psConf1Register.Read();
                return _psConf1Register.PsDuty;
            }

            set
            {
                _psConf1Register.Read();
                if (_psConf1Register.PsDuty != value)
                {
                    _psConf1Register.PsDuty = value;
                    _psConf1Register.Write();
                }
            }
        }

        /// <summary>
        /// Gets or sets the IR LED current.
        /// </summary>
        public PsLedCurrent LedCurrent
        {
            get
            {
                _psMsRegister.Read();
                return _psMsRegister.LedI;
            }

            set
            {
                _psMsRegister.Read();
                if (_psMsRegister.LedI != value)
                {
                    _psMsRegister.LedI = value;
                    _psMsRegister.Write();
                }
            }
        }

        /// <summary>
        /// Gets or sets the integration time.
        /// </summary>
        public PsIntegrationTime IntegrationTime
        {
            get
            {
                _psConf1Register.Read();
                return _psConf1Register.PsIt;
            }

            set
            {
                _psConf1Register.Read();
                if (_psConf1Register.PsIt == value)
                {
                    return;
                }

                _psConf1Register.PsIt = value;
                _psConf1Register.Write();
            }
        }

        /// <summary>
        /// Gets or sets the extended sensor output range state.
        /// If set to false, the range is at the 12-bit default.
        /// If set to true, the range is extended to 16-bit.
        /// </summary>
        public bool ExtendedOutputRange
        {
            get
            {
                _psConf2Register.Read();
                return _psConf2Register.PsHd == PsOutputRange.Bits16;
            }

            set
            {
                _psConf2Register.Read();
                PsOutputRange newRange = value ? PsOutputRange.Bits16 : PsOutputRange.Bits12;
                if (_psConf2Register.PsHd == newRange)
                {
                    return;
                }

                _psConf2Register.PsHd = newRange;
                _psConf2Register.Write();
            }
        }

        /// <summary>
        /// Gets or sets the state of the active force mode.
        /// If set to true, the active force mode is activated; otherwise it is deactivated.
        /// </summary>
        public bool ActiveForceMode
        {
            get
            {
                _psConf3Register.Read();
                _activeForceModeEnabled = _psConf3Register.PsAf == PsActiveForceMode.Enabled;
                return _activeForceModeEnabled;
            }

            set
            {
                _psConf3Register.Read();
                PsActiveForceMode newMode = value ? PsActiveForceMode.Enabled : PsActiveForceMode.Disabled;
                if (_psConf3Register.PsAf == newMode)
                {
                    return;
                }

                _psConf3Register.PsAf = newMode;
                _psConf3Register.Write();

                _activeForceModeEnabled = value;
            }
        }

        #endregion
    }
}
