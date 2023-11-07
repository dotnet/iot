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
        /// Important: if the sensor is in force mode it is required triggering a
        ///            proximity measurement first using the Trigger method.
        /// </summary>
        public int Reading
        {
            get
            {
                _psDataRegister.Read();
                return _psDataRegister.Data;
            }
        }

        /// <summary>
        /// Triggers a single proximity measurement if the sensor is in force mode.
        /// </summary>
        public void Trigger()
        {
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
        /// Gets or sets the sensor output size (12 or 16 bits).
        /// </summary>
        public PsOutput OutputSize
        {
            get
            {
                _psConf2Register.Read();
                return _psConf2Register.PsHd;
            }

            set
            {
                _psConf2Register.Read();
                if (value == _psConf2Register.PsHd)
                {
                    return;
                }

                _psConf2Register.PsHd = value;
                _psConf2Register.Write();
            }
        }

        #endregion
    }
}
