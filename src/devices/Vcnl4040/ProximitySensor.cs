// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProximitySensor"/> API class.
    /// </summary>
    public class ProximitySensor
    {
        private const int MaximumSensorCounts16Bit = 65535;

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
            _psWhiteDataRegister = new WhiteDataRegister(i2cBus);
            _alsConfRegister = new AlsConfRegister(i2cBus);
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
        /// </summary>
        public int Reading
        {
            get
            {
                if (_activeForceModeEnabled)
                {
                    // design consideration: When the active force mode is in use, it can be assumed
                    // that the measurement is retrieved rather infrequently.
                    // Therefore, it is legitimate to read the current register content instead of
                    // working with a local copy. This only minimally increases the bus load and
                    // avoids potential inconsistencies.
                    _psConf3Register.PsTrig = PsActiveForceModeTrigger.OneTimeCycle;
                    _psConf3Register.Write();
                }

                _psDataRegister.Read();
                return _psDataRegister.Data;
            }
        }

        /// <summary>
        /// Gets the current reading of the proximity sensor's white channel.
        /// </summary>
        public int WhiteChannelReading
        {
            get
            {
                _psWhiteDataRegister.Read();
                return _psWhiteDataRegister.Data;
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
                _psConf1Register.PsDuty = value;
                _psConf1Register.Write();
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
                _psMsRegister.LedI = value;
                _psMsRegister.Write();
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
                _psConf2Register.PsHd = value ? PsOutputRange.Bits16 : PsOutputRange.Bits12;
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
                _psConf3Register.PsAf = value ? PsActiveForceMode.Enabled : PsActiveForceMode.Disabled;
                _psConf3Register.Write();
                _activeForceModeEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the white channel enabled state
        /// </summary>
        public bool WhiteChannelEnabled
        {
            get
            {
                _psMsRegister.Read();
                return _psMsRegister.WhiteEn == PsWhiteChannelState.Enabled;
            }
            set
            {
                _psMsRegister.WhiteEn = value ? PsWhiteChannelState.Enabled : PsWhiteChannelState.Disabled;
                _psMsRegister.Write();
            }
        }

        /// <summary>
        /// Gets or sets the number of multi pulses.
        /// </summary>
        public PsMultiPulse MultiPulses
        {
            get
            {
                _psConf3Register.Read();
                return _psConf3Register.PsMps;
            }

            set
            {
                _psConf3Register.PsMps = value;
                _psConf3Register.Write();
            }
        }

        /// <summary>
        /// Enables/disables the sunligh cancellation
        /// </summary>
        public bool SunlightCancellationEnabled
        {
            get
            {
                _psConf3Register.Read();
                return _psConf3Register.PsScEn == PsSunlightCancellationState.Enabled;
            }

            set
            {
                _psConf3Register.PsScEn = value ? PsSunlightCancellationState.Enabled : PsSunlightCancellationState.Disabled;
                _psConf3Register.Write();
            }
        }

        /// <summary>
        /// Gets or sets the cancellation level for the interrupt thresholds
        /// </summary>
        public int CancellationLevel
        {
            get
            {
                _psCancellationLevelRegister.Read();
                return _psCancellationLevelRegister.Level;
            }

            set
            {
                _psCancellationLevelRegister.Level = value;
                _psCancellationLevelRegister.Write();
            }
        }

        /// <summary>
        /// Configures the IR led emitter
        /// </summary>
        /// <param name="current">...</param>
        /// <param name="dutyRation">...</param>
        /// <param name="multiPulses">...</param>
        public void ConfigureEmitter(PsLedCurrent current, PsDuty dutyRation, PsMultiPulse multiPulses)
        {
        }

        /// <summary>
        /// Configures the IR receiver.
        /// </summary>
        /// <param name="integrationTime">...</param>
        /// <param name="extendedOutputRange">...</param>
        /// <param name="cancellationLevel">...</param>
        /// <param name="whiteChannelEnabled">...</param>
        /// <param name="sunlightCancellationEnabled">...</param>
        public void ConfigureReceiver(PsIntegrationTime integrationTime, bool extendedOutputRange, int cancellationLevel, bool whiteChannelEnabled, bool sunlightCancellationEnabled)
        {
        }

        #endregion

        #region Interrupt

        /// <summary>
        /// Gets whether proximity sensor interrupts are enabled.
        /// Important: will also return TRUE if in proximity detection mode
        /// </summary>
        public bool InterruptEnabled
        {
            get
            {
                _psConf2Register.Read();
                return _psConf2Register.PsInt != PsInterruptMode.Disabled;
            }
        }

        /// <summary>
        /// Gets whether the proximity detection logic output mode is enabled.
        /// </summary>
        public bool ProximityDetecionModeEnabled
        {
            get
            {
                _psMsRegister.Read();
                return _psMsRegister.PsMs == PsDetectionLogicOutputMode.LogicOutput;
            }
        }

        /// <summary>
        /// Disables the interrupts and proximity detection mode.
        /// </summary>
        public void DisableInterruptsAndProximityDetection()
        {
            _psConf2Register.Read();
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            _psMsRegister.Read();
            _psMsRegister.PsMs = PsDetectionLogicOutputMode.Interrupt;
            _psMsRegister.Write();
        }

        /// <summary>
        /// ...disables proximity detection mode...
        /// </summary>
        /// <param name="lowerThreshold">Lower threshold for triggering the interrupt</param>
        /// <param name="upperThreshold">Upper threshold for triggering the interrupt</param>
        /// <param name="persistence">Amount of consecutive hits needed for triggering the interrupt</param>
        /// <param name="mode">Interrupt mode</param>
        /// <param name="enableSmartPersistence">Enable smart persistence</param>
        public void EnableInterrupts(int lowerThreshold,
                                     int upperThreshold,
                                     PsInterruptPersistence persistence,
                                     PsInterruptMode mode,
                                     bool enableSmartPersistence)
        {
            // disable interrupts before altering configuration to avoid transient side effects
            _psConf2Register.Read();
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            ConfigureThresholds(lowerThreshold, upperThreshold);

            _psMsRegister.Read();
            _psMsRegister.PsMs = PsDetectionLogicOutputMode.Interrupt;
            _psMsRegister.Write();

            // set persistence
            _psConf1Register.Read();
            _psConf1Register.PsPers = persistence;
            _psConf1Register.Write();

            // set smart persistence
            _psConf3Register.Read();
            _psConf3Register.PsSmartPers = enableSmartPersistence ? PsSmartPersistenceState.Enabled : PsSmartPersistenceState.Disabled;
            _psConf3Register.Write();

            // enable interrupts
            _psConf2Register.PsInt = mode;
            _psConf2Register.Write();
        }

        /// <summary>
        /// ....interrupts are not available in this mode (incl. ALS interrupts)
        /// </summary>
        /// <param name="lowerThreshold">Lower threshold for triggering the interrupt</param>
        /// <param name="upperThreshold">Upper threshold for triggering the interrupt</param>
        /// <param name="persistence">Amount of consecutive hits needed for triggering the interrupt</param>
        public void EnableProximityDetectionMode(int lowerThreshold,
                                                 int upperThreshold,
                                                 PsInterruptPersistence persistence)
        {
            // disable interrupts before altering configuration to avoid transient side effects
            _psConf2Register.Read();
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            ConfigureThresholds(lowerThreshold, upperThreshold);

            // set persistence
            _psConf1Register.Read();
            _psConf1Register.PsPers = persistence;
            _psConf1Register.Write();

            // enable interrupts
            _psConf2Register.PsInt = PsInterruptMode.CloseOrAway;
            _psConf2Register.Write();

            _psMsRegister.Read();
            _psMsRegister.PsMs = PsDetectionLogicOutputMode.LogicOutput;
            _psMsRegister.Write();
        }

        private void ConfigureThresholds(int lowerThreshold, int upperThreshold)
        {
            // Design consideration: The configured output range (12-bit or 16-bit) is not verified at this point.
            // Even if the range is set to the default of 12-bit, threshold values above it work reliably.
            // Therefore, the configuration of the output range and the interrupts are considered independently.
            if (lowerThreshold < 0 || lowerThreshold > MaximumSensorCounts16Bit)
            {
                throw new ArgumentException($"Lower threshold (is: {lowerThreshold}) must be positive and must not exceed the maximum range of {MaximumSensorCounts16Bit} counts");
            }

            if (lowerThreshold > upperThreshold || upperThreshold > MaximumSensorCounts16Bit)
            {
                throw new ArgumentException($"Upper threshold (is: {upperThreshold}) must be higher than the lower threshold (is: {lowerThreshold}) and must not exceed the maximum range of ({MaximumSensorCounts16Bit}) counts");
            }

            // set new thresholds
            _psLowInterruptThresholdRegister.Threshold = lowerThreshold;
            _psHighInterruptThresholdRegister.Threshold = upperThreshold;
            _psLowInterruptThresholdRegister.Write();
            _psHighInterruptThresholdRegister.Write();
        }

        /// <summary>
        /// Gets the interrupt configuration of the proximity sensor
        /// </summary>
        public (int LowerThreshold,
                int UpperThreshold,
                PsInterruptPersistence Persistence,
                PsInterruptMode Mode,
                bool SmartPersistenceEnabled,
                int CancellationLevel) GetInterruptConfiguration()
        {
            _psLowInterruptThresholdRegister.Read();
            _psHighInterruptThresholdRegister.Read();
            _psConf1Register.Read();
            _psConf2Register.Read();
            _psConf3Register.Read();
            _psCancellationLevelRegister.Read();
            return (_psLowInterruptThresholdRegister.Threshold,
                    _psHighInterruptThresholdRegister.Threshold,
                    _psConf1Register.PsPers,
                    _psConf2Register.PsInt,
                    _psConf3Register.PsSmartPers == PsSmartPersistenceState.Enabled,
                    _psCancellationLevelRegister.Level);
        }
        #endregion
    }
}
