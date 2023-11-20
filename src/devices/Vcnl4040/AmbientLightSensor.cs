// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;
using UnitsNet;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Represents the ambient light sensor component of the VCNL4040 device.
    /// </summary>
    public class AmbientLightSensor
    {
        private readonly AlsConfRegister _alsConfRegister;
        private readonly AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private readonly AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private readonly AlsDataRegister _alsDataRegister;
        private readonly PsMsRegister _psMsRegister;
        private AlsIntegrationTime _localIntegrationTime = AlsIntegrationTime.Time80ms;
        private bool _loadReductionModeEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientLightSensor"/> class.
        /// </summary>
        internal AmbientLightSensor(I2cDevice device)
        {
            _alsConfRegister = new AlsConfRegister(device);
            _alsHighInterruptThresholdRegister = new AlsHighInterruptThresholdRegister(device);
            _alsLowInterruptThresholdRegister = new AlsLowInterruptThresholdRegister(device);
            _alsDataRegister = new AlsDataRegister(device);
            _psMsRegister = new PsMsRegister(device);
        }

        #region General

        /// <summary>
        /// Get or sets the power state of the ambient light sensor.
        /// The default setting is that the sensor is turned off.
        /// The sensor can be configured while in the off state.
        /// To query measurements, the sensor must be activated.
        /// The time until the first value is available depends on the set integration time.
        /// In the off state, the measurement value is not updated.
        /// The sensor can be turned on and off at any time.
        /// The configuration remains unaffected by this.
        /// </summary>
        public bool PowerOn
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsSd == PowerState.PowerOn;
            }

            set
            {
                _alsConfRegister.AlsSd = value ? PowerState.PowerOn : PowerState.PowerOff;
                _alsConfRegister.Write();
            }
        }

        /// <summary>
        /// Enables or disables the load reduction mode.
        /// If the Load Reduction Mode is enabled, the binding uses a local copy of the last set
        /// value for the integration time to calculate the measurement value. Otherwise, the
        /// current integration time is read from the device each time a measurement is queried.
        /// This causes additional load on the I2C bus, which may be relevant at a high query frequency.
        /// In Load Reduction Mode, this query is eliminated. However, if the integration time setting
        /// is changed without using the corresponding property of the binding, it may lead to inconsistency
        /// resulting in incorrect measurement value calculation.
        /// Therefore, it is crucial that any changes are made exclusively through the binding.
        /// The Load Reduction Mode should only be used when the bus load is relevant.
        /// The default state is that it is turned off.
        /// </summary>
        public bool LoadReductionModeEnabled
        {
            get => _loadReductionModeEnabled;

            set
            {
                // retrieve current integration time from the device for a last time
                if (value)
                {
                    _alsConfRegister.Read();
                    _localIntegrationTime = _alsConfRegister.AlsIt;
                }

                _loadReductionModeEnabled = value;
            }
        }

        #endregion

        #region Measurement

        /// <summary>
        /// Gets the current ambient light sensor reading.
        /// The device internal count is converted into a measurement value in Lux
        /// using the configured resolution.
        /// Note: the resolution is an indirect parameter derived from the integration time, as described in the datasheet.
        /// Note: the documentation for the Load Reduction Mode should be considered.
        /// </summary>
        public Illuminance Illuminance
        {
            get
            {
                _alsDataRegister.Read();
                Illuminance resolution;
                if (_loadReductionModeEnabled)
                {
                    resolution = GetDetectionRangeAndResolution(_localIntegrationTime).Resolution;
                }
                else
                {
                    resolution = GetDetectionRangeAndResolution(IntegrationTime).Resolution;
                }

                return Illuminance.FromLux(_alsDataRegister.Data * resolution.Lux);
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Gets or sets the ambient light sensor integration time.
        /// Note: Changing the integration time implicitly results in an adjustment of the resolution and range.
        ///       Range and resolution are parameters that indirectly depend on the integration time.
        ///       The specific relationship is specified in the datasheet.
        /// Important: when setting the integration time, possibly configured interrupts are implicitly deactivated.
        ///            This is done to prevent accidentally using invalid threshold levels.
        ///            Interrupts must be explicitly configured and re-enabled.
        /// </summary>
        public AlsIntegrationTime IntegrationTime
        {
            get
            {
                _alsConfRegister.Read();

                // Since we are already reading the current integration time from the chip,
                // we can also update the internally stored value. While not strictly necessary,
                // this could potentially resolve any existing inconsistency, ideally.
                _localIntegrationTime = _alsConfRegister.AlsIt;

                return _alsConfRegister.AlsIt;
            }

            set
            {
                DisableInterrupts();

                _alsConfRegister.AlsIt = value;
                _alsConfRegister.Write();
                _localIntegrationTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the detection range.
        /// Note: The range is a parameter that depends on the integration time.
        ///       Changing the range, therefore, implicitly adjusts the integration time and
        ///       consequently deactivates any set interrupts.
        ///       Refer to the description of the <see cref="IntegrationTime"/> property for more information.
        /// </summary>
        public AlsRange Range
        {
            get
            {
                // get range derived from corresponding integration time
                return IntegrationTime switch
                {
                    AlsIntegrationTime.Time80ms => AlsRange.Range6553,
                    AlsIntegrationTime.Time160ms => AlsRange.Range3276,
                    AlsIntegrationTime.Time320ms => AlsRange.Range1638,
                    AlsIntegrationTime.Time640ms => AlsRange.Range819,
                    _ => throw new NotImplementedException(),
                };
            }

            set
            {
                // set the range by setting the corresponding integration time
                IntegrationTime = value switch
                {
                    AlsRange.Range6553 => AlsIntegrationTime.Time80ms,
                    AlsRange.Range3276 => AlsIntegrationTime.Time160ms,
                    AlsRange.Range1638 => AlsIntegrationTime.Time320ms,
                    AlsRange.Range819 => AlsIntegrationTime.Time640ms,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        /// <summary>
        /// Gets the range as illuminance value.
        /// </summary>
        public Illuminance RangeAsIlluminance => GetDetectionRangeAndResolution(IntegrationTime).Range;

        /// <summary>
        /// Gets or sets the resolution.
        /// Note: The resolution is a parameter that depends on the integration time.
        ///       Changing the resolution, therefore, implicitly adjusts the integration time and
        ///       consequently deactivates any set interrupts.
        ///       Refer to the description of the <see cref="IntegrationTime"/> property for more information.
        /// </summary>
        public AlsResolution Resolution
        {
            get
            {
                // get resolution derived from corresponding integration time
                return IntegrationTime switch
                {
                    AlsIntegrationTime.Time80ms => AlsResolution.Resolution_0_1,
                    AlsIntegrationTime.Time160ms => AlsResolution.Resolution_0_05,
                    AlsIntegrationTime.Time320ms => AlsResolution.Resolution_0_025,
                    AlsIntegrationTime.Time640ms => AlsResolution.Resolution_0_0125,
                    _ => throw new NotImplementedException(),
                };
            }

            set
            {
                // set the range by setting the corresponding integration time
                IntegrationTime = value switch
                {
                    AlsResolution.Resolution_0_1 => AlsIntegrationTime.Time80ms,
                    AlsResolution.Resolution_0_05 => AlsIntegrationTime.Time160ms,
                    AlsResolution.Resolution_0_025 => AlsIntegrationTime.Time320ms,
                    AlsResolution.Resolution_0_0125 => AlsIntegrationTime.Time640ms,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        /// <summary>
        /// Gets the resolution as illuminance value.
        /// </summary>
        public Illuminance ResolutionAsIlluminance => GetDetectionRangeAndResolution(IntegrationTime).Resolution;
        #endregion

        #region Interrupt

        /// <summary>
        /// Gets whether interrupt function (INT-pin function) is enabled.
        /// </summary>
        public bool IsInterruptEnabled
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsIntEn == AlsInterrupt.Enabled;
            }
        }

        /// <summary>
        /// Disables the interrupts (INT-pin function).
        /// </summary>
        public void DisableInterrupts()
        {
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Configures the interrupt parameters and enables the interrupt (INT-pin function).
        /// Refer to <see cref="AmbientLightInterruptConfiguration"/> for more information on the parameters.
        /// Note: even in Load Reduction Mode the actual integration time setting from the device is
        ///       used. This would implicitly update the local copy if an inconsistency should prevail.
        ///       Refer to <see cref="LoadReductionModeEnabled"/> for further information.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if any threshold exceeds the limit defined by the range (integration time),
        /// or lower threshold is higher than the upper one, or a threshold is negative.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the proximity logic output of the proximity sensor is enabled.
        /// Refer to <see cref="ProximityInterruptConfiguration"/> for more information.</exception>
        public void EnableInterrupts(AmbientLightInterruptConfiguration configuration)
        {
            // the maximum detection range and resolution depends on the integration time setting
            _alsConfRegister.Read();
            (Illuminance maxDetectionRange, Illuminance resolution) = GetDetectionRangeAndResolution(_alsConfRegister.AlsIt);

            if (configuration.LowerThreshold.Lux < 0 || configuration.UpperThreshold.Lux < 0)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) and upper threshold (is: {configuration.UpperThreshold}) must be positive.");
            }

            if (configuration.LowerThreshold > maxDetectionRange || configuration.UpperThreshold > maxDetectionRange)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) or upper threshold (is: {configuration.UpperThreshold}) must not exceed maximum range of {maxDetectionRange} lux.");
            }

            if (configuration.LowerThreshold > configuration.UpperThreshold)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) must not be higher than upper threshold  (is: {configuration.UpperThreshold}).");
            }

            _psMsRegister.Read();
            if (_psMsRegister.PsMs == PsProximityDetectionOutput.LogicOutput)
            {
                throw new InvalidOperationException("Logic output mode interferes with ALS interrupt function. Logic output must be disabled.");
            }

            // disable interrupts before altering configuration to avoid transient side effects
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();

            // set threshold levels by calculating the register value in counts based on the current resolution.
            _alsLowInterruptThresholdRegister.Level = (ushort)(configuration.LowerThreshold.Lux / resolution.Lux);
            _alsHighInterruptThresholdRegister.Level = (ushort)(configuration.UpperThreshold.Lux / resolution.Lux);
            _alsLowInterruptThresholdRegister.Write();
            _alsHighInterruptThresholdRegister.Write();
            // set persistence and enable interrupts
            _alsConfRegister.AlsPers = configuration.Persistence;
            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Gets the current interrupt configuration from the device.
        /// Note: even in Load Reduction Mode the actual integration time setting from the device is
        ///       used. This would implicitly update the local copy if an inconsistency should prevail.
        ///       Refer to <see cref="LoadReductionModeEnabled"/> for further information.
        /// </summary>
        public AmbientLightInterruptConfiguration GetInterruptConfiguration()
        {
            _alsLowInterruptThresholdRegister.Read();
            _alsHighInterruptThresholdRegister.Read();
            _alsConfRegister.Read();

            (_, Illuminance resolution) = GetDetectionRangeAndResolution(_alsConfRegister.AlsIt);

            return new(LowerThreshold: Illuminance.FromLux(_alsLowInterruptThresholdRegister.Level * resolution.Lux),
                       UpperThreshold: Illuminance.FromLux(_alsHighInterruptThresholdRegister.Level * resolution.Lux),
                       Persistence: _alsConfRegister.AlsPers);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Helper method to get detection range and resolution for the given integration time.
        /// </summary>
        private static (Illuminance Range, Illuminance Resolution) GetDetectionRangeAndResolution(AlsIntegrationTime integrationTime)
        {
            return integrationTime switch
            {
                AlsIntegrationTime.Time80ms => (Illuminance.FromLux(6553.5), Illuminance.FromLux(0.1)),
                AlsIntegrationTime.Time160ms => (Illuminance.FromLux(3276.8), Illuminance.FromLux(0.05)),
                AlsIntegrationTime.Time320ms => (Illuminance.FromLux(1638.4), Illuminance.FromLux(0.025)),
                AlsIntegrationTime.Time640ms => (Illuminance.FromLux(819.2), Illuminance.FromLux(0.0125)),
                _ => throw new NotImplementedException(),
            };
        }
        #endregion
    }
}
