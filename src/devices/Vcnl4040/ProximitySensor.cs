// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProximitySensor"/> class.
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
        private readonly WhiteDataRegister _psWhiteDataRegister;
        private readonly AlsConfRegister _alsConfRegister;
        private bool _activeForceModeEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximitySensor"/> class.
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
        /// Gets or sets the state of the active force mode for proximity measurement.
        /// When the sensor is operated in Active Force Mode, an explicit request for a measurement
        /// is required before a current measurement value can be read.
        /// This is in contrast to normal operation, where measurements are continuous.
        /// This significantly reduces power consumption, as the IR LED is only activated for each
        /// individual measurement.
        /// Important: In Active Force Mode, the interrupt function is not available,
        /// as no measurement is performed, and therefore, threshold checks are not possible.
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
        /// Note: if active force mode is enabled reading this property implicitly triggers
        /// the measurement for once cycle.
        /// </summary>
        public int Distance
        {
            get
            {
                if (_activeForceModeEnabled)
                {
                    _psConf3Register.PsTrig = PsActiveForceModeTrigger.OneTimeCycle;
                    _psConf3Register.Write();
                }

                _psDataRegister.Read();
                return _psDataRegister.Data;
            }
        }

        /// <summary>
        /// Gets the current measurement value from the white channel of the proximity sensor.
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
        /// Configures the IR LED emitter.
        /// Refer to <see cref="EmitterConfiguration"/> for more information on the parameters.
        /// </summary>
        public void ConfigureEmitter(EmitterConfiguration configuration)
        {
            _psMsRegister.LedI = configuration.Current;
            _psConf1Register.PsDuty = configuration.DutyRatio;
            _psConf1Register.PsIt = configuration.IntegrationTime;
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

            return new EmitterConfiguration(Current: _psMsRegister.LedI,
                                            DutyRatio: _psConf1Register.PsDuty,
                                            IntegrationTime: _psConf1Register.PsIt,
                                            MultiPulses: _psConf3Register.PsMps);
        }

        /// <summary>
        /// Configures the IR receiver.
        /// Refer to <see cref="ReceiverConfiguration"/> for more information on the parameters.
        /// </summary>
        public void ConfigureReceiver(ReceiverConfiguration configuration)
        {
            _psConf2Register.PsHd = configuration.ExtendedOutputRange ? PsOutputRange.Bits16 : PsOutputRange.Bits12;
            _psMsRegister.WhiteEn = configuration.WhiteChannelEnabled ? PsWhiteChannelState.Enabled : PsWhiteChannelState.Disabled;
            _psCancellationLevelRegister.Level = configuration.CancellationLevel;
            _psConf3Register.PsScEn = configuration.SunlightCancellationEnabled ? PsSunlightCancellationState.Enabled : PsSunlightCancellationState.Disabled;
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

            return new ReceiverConfiguration(ExtendedOutputRange: _psConf2Register.PsHd == PsOutputRange.Bits16,
                                             CancellationLevel: _psCancellationLevelRegister.Level,
                                             WhiteChannelEnabled: _psMsRegister.WhiteEn == PsWhiteChannelState.Enabled,
                                             SunlightCancellationEnabled: _psConf3Register.PsScEn == PsSunlightCancellationState.Enabled);
        }

        #endregion

        #region Interrupt

        /// <summary>
        /// Gets whether the proximity sensor interrupts are enabled.
        /// Important: will also return TRUE if in proximity detection mode,
        /// as this is a specific interrupt function.
        /// </summary>
        public bool IsInterruptEnabled
        {
            get
            {
                _psConf2Register.Read();
                return _psConf2Register.PsInt != PsInterruptMode.Disabled;
            }
        }

        /// <summary>
        /// Gets whether the proximity detection logic output is enabled.
        /// </summary>
        public bool IsLogicOutputEnabled
        {
            get
            {
                _psMsRegister.Read();
                return _psMsRegister.PsMs == PsProximityDetectionOutput.LogicOutput;
            }
        }

        /// <summary>
        /// Disables the interrupts or logic output.
        /// </summary>
        public void DisableInterrupts()
        {
            _psConf2Register.Read();
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            _psMsRegister.Read();
            _psMsRegister.PsMs = PsProximityDetectionOutput.Interrupt;
            _psMsRegister.Write();
        }

        /// <summary>
        /// Configures the interrupt parameters and enables the interrupt (INT-pin function).
        /// Refer to <see cref="ProximityInterruptConfiguration"/> for more information on the parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if upper threshold is less than lower threshold.</exception>
        /// <exception cref="InvalidOperationException">Thrown if Interrupt function of the ambient light sensor is enabled.
        /// Refer to <see cref="ProximityInterruptConfiguration"/> for more information.</exception>
        public void EnableInterrupts(ProximityInterruptConfiguration configuration)
        {
            // Design consideration: The configured output range (12-bit or 16-bit) is not verified at this point.
            // Even if the range is set to the default of 12-bit, threshold values above it work reliably.
            // Therefore, the configuration of the output range and the interrupts are considered independently.
            if (configuration.LowerThreshold > configuration.UpperThreshold)
            {
                throw new ArgumentException($"Upper threshold (is: {configuration.UpperThreshold}) must be higher than the lower threshold (is: {configuration.LowerThreshold}).");
            }

            // enable interrupts / proximity detection logic output
            if (configuration.Mode == ProximityInterruptMode.LogicOutput)
            {
                if (_alsConfRegister.AlsIntEn == AlsInterrupt.Enabled)
                {
                    throw new InvalidOperationException("Logic output mode interferes with ALS interrupt function. ALS interrupts must be disabled.");
                }

                _psMsRegister.PsMs = PsProximityDetectionOutput.LogicOutput;
            }
            else
            {
                _psMsRegister.PsMs = PsProximityDetectionOutput.Interrupt;
            }

            // disable interrupts before altering configuration to avoid transient side effects
            _psConf2Register.PsInt = PsInterruptMode.Disabled;
            _psConf2Register.Write();

            // set new thresholds
            _psLowInterruptThresholdRegister.Level = configuration.LowerThreshold;
            _psHighInterruptThresholdRegister.Level = configuration.UpperThreshold;
            _psLowInterruptThresholdRegister.Write();
            _psHighInterruptThresholdRegister.Write();

            // set persistence
            _psConf1Register.PsPers = configuration.Persistence;
            _psConf1Register.Write();

            // set smart persistence
            _psConf3Register.PsSmartPers = configuration.SmartPersistenceEnabled ? PsSmartPersistenceState.Enabled : PsSmartPersistenceState.Disabled;
            _psConf3Register.Write();

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
        /// Gets the interrupt configuration from the device.
        /// </summary>
        public ProximityInterruptConfiguration GetInterruptConfiguration()
        {
            _psLowInterruptThresholdRegister.Read();
            _psHighInterruptThresholdRegister.Read();
            _psConf1Register.Read();
            _psConf2Register.Read();
            _psConf3Register.Read();
            _psMsRegister.Read();

            ProximityInterruptMode mode;
            if (_psMsRegister.PsMs == PsProximityDetectionOutput.LogicOutput)
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

            return new ProximityInterruptConfiguration(_psLowInterruptThresholdRegister.Level,
                                                       _psHighInterruptThresholdRegister.Level,
                                                       _psConf1Register.PsPers,
                                                       _psConf3Register.PsSmartPers == PsSmartPersistenceState.Enabled,
                                                       mode);
        }
        #endregion
    }
}
