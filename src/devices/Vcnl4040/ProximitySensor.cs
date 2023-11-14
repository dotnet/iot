// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Definitions;
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
        internal ProximitySensor(I2cDevice device)
        {
            _psConf1Register = new PsConf1Register(device);
            _psConf2Register = new PsConf2Register(device);
            _psConf3Register = new PsConf3Register(device);
            _psMsRegister = new PsMsRegister(device);
            _psCancellationLevelRegister = new PsCancellationLevelRegister(device);
            _psLowInterruptThresholdRegister = new PsLowInterruptThresholdRegister(device);
            _psHighInterruptThresholdRegister = new PsHighInterruptThresholdRegister(device);
            _psDataRegister = new PsDataRegister(device);
            _psWhiteDataRegister = new WhiteDataRegister(device);
            _alsConfRegister = new AlsConfRegister(device);
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

        /// <summary>
        /// Gets or sets the state of the active force mode.
        /// If set to true, the active force mode is activated; otherwise it is deactivated.
        /// IN POWER SAVE UMBENENNEN?
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
        /// Configures the IR led emitter.
        /// </summary>
        public void ConfigureEmitter(EmitterConfiguration configuration)
        {
            _psMsRegister.LedI = configuration.Current;
            _psConf1Register.PsDuty = configuration.DutyRatio;
            _psConf3Register.PsMps = configuration.MultiPulses;

            _psConf1Register.Write();
            _psConf3Register.Write();
            _psMsRegister.Write();
        }

        /// <summary>
        /// Gets the current emitter configuration from the device.
        /// </summary>
        public EmitterConfiguration GetEmitterConfiguration()
        {
            _psConf1Register.Read();
            _psConf3Register.Read();
            _psMsRegister.Read();

            return new EmitterConfiguration(
                _psMsRegister.LedI,
                _psConf1Register.PsDuty,
                _psConf3Register.PsMps);
        }

        /// <summary>
        /// Configures the IR receiver.
        /// </summary>
        public void ConfigureReceiver(ReceiverConfiguration configuration)
        {
            _psConf1Register.PsIt = configuration.IntegrationTime;
            _psConf2Register.PsHd = configuration.ExtendedOutputRange ? PsOutputRange.Bits16 : PsOutputRange.Bits12;
            _psMsRegister.WhiteEn = configuration.WhiteChannelEnabled ? PsWhiteChannelState.Enabled : PsWhiteChannelState.Disabled;
            _psCancellationLevelRegister.Level = configuration.CancellationLevel;
            _psConf3Register.PsScEn = configuration.SunlightCancellationEnabled ? PsSunlightCancellationState.Enabled : PsSunlightCancellationState.Disabled;
            _psConf1Register.Write();
            _psConf2Register.Write();
            _psConf3Register.Write();
            _psMsRegister.Write();
            _psCancellationLevelRegister.Write();
        }

        /// <summary>
        /// Gets the current receiver configuration from the device.
        /// </summary>
        public ReceiverConfiguration GetReceiverConfiguration()
        {
            _psCancellationLevelRegister.Read();
            _psMsRegister.Read();
            _psConf1Register.Read();
            _psConf2Register.Read();
            _psConf3Register.Read();

            return new ReceiverConfiguration(_psConf1Register.PsIt,
                                             _psConf2Register.PsHd == PsOutputRange.Bits16,
                                             _psCancellationLevelRegister.Level,
                                             _psMsRegister.WhiteEn == PsWhiteChannelState.Enabled,
                                             _psConf3Register.PsScEn == PsSunlightCancellationState.Enabled);
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
        public bool LogicOutputModeEnabled
        {
            get
            {
                _psMsRegister.Read();
                return _psMsRegister.PsMs == PsProximityDetectionOutputMode.LogicOutput;
            }
        }

        /// <summary>
        /// Disables the interrupts and proximity detection mode.
        /// </summary>
        public void DisableInterrupt()
        {
            _psConf2Register.Read();
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            _psMsRegister.Read();
            _psMsRegister.PsMs = PsProximityDetectionOutputMode.Interrupt;
            _psMsRegister.Write();
        }

        /// <summary>
        /// ...disables proximity detection mode...
        /// </summary>
        public void EnableInterrupt(ProximityInterruptConfiguration configuration)
        {
            // disable interrupts before altering configuration to avoid transient side effects
            _psConf2Register.Read();
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            // Design consideration: The configured output range (12-bit or 16-bit) is not verified at this point.
            // Even if the range is set to the default of 12-bit, threshold values above it work reliably.
            // Therefore, the configuration of the output range and the interrupts are considered independently.
            if (configuration.LowerThreshold < 0 || configuration.LowerThreshold > MaximumSensorCounts16Bit)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) must be positive and must not exceed the maximum range of {MaximumSensorCounts16Bit} counts");
            }

            if (configuration.LowerThreshold > configuration.UpperThreshold || configuration.UpperThreshold > MaximumSensorCounts16Bit)
            {
                throw new ArgumentException($"Upper threshold (is: {configuration.UpperThreshold}) must be higher than the lower threshold (is: {configuration.LowerThreshold}) and must not exceed the maximum range of ({MaximumSensorCounts16Bit}) counts");
            }

            // set new thresholds
            _psLowInterruptThresholdRegister.Threshold = configuration.LowerThreshold;
            _psHighInterruptThresholdRegister.Threshold = configuration.UpperThreshold;
            _psLowInterruptThresholdRegister.Write();
            _psHighInterruptThresholdRegister.Write();

            // set persistence
            _psConf1Register.Read();
            _psConf1Register.PsPers = configuration.Persistence;
            _psConf1Register.Write();

            // set smart persistence
            _psConf3Register.Read();
            _psConf3Register.PsSmartPers = configuration.SmartPersistenceEnabled ? PsSmartPersistenceState.Enabled : PsSmartPersistenceState.Disabled;
            _psConf3Register.Write();

            // enable interrupts / proximity dectection logic output
            _psMsRegister.Read();
            if (configuration.Mode == ProximityInterruptMode.LogicOutput)
            {
                // disable ALS interrupts
                // (required according to datasheet, but it may work even if still enabled)
                _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
                _alsConfRegister.Write();

                _psMsRegister.PsMs = PsProximityDetectionOutputMode.LogicOutput;
            }
            else
            {
                _psMsRegister.PsMs = PsProximityDetectionOutputMode.Interrupt;
            }

            // enable interrupts
            _psConf2Register.PsInt = configuration.Mode switch
            {
                ProximityInterruptMode.CloseInterrupt => PsInterruptMode.Close,
                ProximityInterruptMode.AwayInterrupt => PsInterruptMode.Away,
                ProximityInterruptMode.CloseOrAwayInterrupt => PsInterruptMode.CloseOrAway,
                ProximityInterruptMode.LogicOutput => PsInterruptMode.CloseOrAway,
                _ => throw new ArgumentException("Invalid mode", nameof(configuration))
            };

            _psMsRegister.Write();
            _psConf2Register.Write();
        }

        /// <summary>
        /// Gets the interrupt configuration of the proximity sensor
        /// </summary>
        public ProximityInterruptConfiguration GetInterruptConfiguration()
        {
            _psLowInterruptThresholdRegister.Read();
            _psHighInterruptThresholdRegister.Read();
            _psConf1Register.Read();
            _psConf2Register.Read();
            _psConf3Register.Read();
            _psMsRegister.Read();
            _psCancellationLevelRegister.Read();

            ProximityInterruptMode mode;
            if (_psMsRegister.PsMs == PsProximityDetectionOutputMode.LogicOutput)
            {
                mode = ProximityInterruptMode.LogicOutput;
            }
            else
            {
                mode = _psConf2Register.PsInt switch
                {
                    PsInterruptMode.Disabled => ProximityInterruptMode.Nothing,
                    PsInterruptMode.Close => ProximityInterruptMode.CloseInterrupt,
                    PsInterruptMode.Away => ProximityInterruptMode.AwayInterrupt,
                    PsInterruptMode.CloseOrAway => ProximityInterruptMode.CloseOrAwayInterrupt,
                    _ => throw new ArgumentException("Invalid interrupt")
                };
            }

            return new ProximityInterruptConfiguration(_psLowInterruptThresholdRegister.Threshold,
                                                       _psHighInterruptThresholdRegister.Threshold,
                                                       _psConf1Register.PsPers,
                                                       _psConf3Register.PsSmartPers == PsSmartPersistenceState.Enabled,
                                                       mode);
        }
        #endregion
    }
}
