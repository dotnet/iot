﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;
using UnitsNet;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientLightSensor"/> API class.
    /// </summary>
    public class AmbientLightSensor
    {
        private readonly AlsConfRegister _alsConfRegister;
        private readonly AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private readonly AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private readonly AlsDataRegister _alsDataRegister;
        private AlsIntegrationTime _lastKnownIntegrationTime = AlsIntegrationTime.Time80ms;
        private bool _loadReductionModeEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientLightSensor"/> class.
        /// </summary>
        internal AmbientLightSensor(I2cInterface i2cBus)
        {
            _alsConfRegister = new AlsConfRegister(i2cBus);
            _alsHighInterruptThresholdRegister = new AlsHighInterruptThresholdRegister(i2cBus);
            _alsLowInterruptThresholdRegister = new AlsLowInterruptThresholdRegister(i2cBus);
            _alsDataRegister = new AlsDataRegister(i2cBus);
        }

        #region General

        /// <summary>
        /// Attaches the binding instance to an already operating device.
        /// </summary>
        public void Attach()
        {
            _lastKnownIntegrationTime = _alsConfRegister.AlsIt;
        }

        /// <summary>
        /// Enables or disables the load reduction mode for the ambient light sensor.
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
                    _lastKnownIntegrationTime = _alsConfRegister.AlsIt;
                }

                _loadReductionModeEnabled = value;
            }
        }

        /// <summary>
        /// Get or sets the power state (power on, shutdown) of the ambient light sensor.
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
                _alsConfRegister.Read();
                if ((value && _alsConfRegister.AlsSd == PowerState.PowerOn)
                || (!value && _alsConfRegister.AlsSd == PowerState.PowerOff))
                {
                    return;
                }

                _alsConfRegister.AlsSd = value ? PowerState.PowerOn : PowerState.PowerOff;
                _alsConfRegister.Write();
            }
        }

        #endregion

        #region Measurement

        /// <summary>
        /// Gets the current ambient light sensor reading.
        /// </summary>
        public Illuminance Reading
        {
            get
            {
                _alsDataRegister.Read();
                Illuminance resolution;
                if (_loadReductionModeEnabled)
                {
                    resolution = GetDetectionRangeAndResolution(_lastKnownIntegrationTime).Resolution;
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
        /// </summary>
        public AlsIntegrationTime IntegrationTime
        {
            get
            {
                _alsConfRegister.Read();

                // Since we are already reading the current integration time from the chip,
                // we can also update the internally stored value. While not strictly necessary,
                // this could potentially resolve any existing inconsistency, ideally.
                _lastKnownIntegrationTime = _alsConfRegister.AlsIt;

                return _alsConfRegister.AlsIt;
            }

            set
            {
                DisableInterrupts();

                _alsConfRegister.Read();
                _alsConfRegister.AlsIt = value;
                _alsConfRegister.Write();
                _lastKnownIntegrationTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the detection range.
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
        /// Gets the range as illuminance value
        /// </summary>
        public Illuminance RangeAsIlluminance
        {
            get
            {
                return Range switch
                {
                    AlsRange.Range819 => Illuminance.FromLux(819.2),
                    AlsRange.Range1638 => Illuminance.FromLux(1638.4),
                    AlsRange.Range3276 => Illuminance.FromLux(3276.8),
                    AlsRange.Range6553 => Illuminance.FromLux(6553.5),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        /// <summary>
        /// Gets the resolution as illuminance value
        /// </summary>
        public Illuminance ResolutionAsIlluminance
        {
            get
            {
                return Resolution switch
                {
                    AlsResolution.Resolution_0_1 => Illuminance.FromLux(0.1),
                    AlsResolution.Resolution_0_05 => Illuminance.FromLux(0.05),
                    AlsResolution.Resolution_0_025 => Illuminance.FromLux(0.025),
                    AlsResolution.Resolution_0_0125 => Illuminance.FromLux(0.0125),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        /// <summary>
        /// Gets or initializes the detection resolution.
        /// Important note: the resolution can only be initialized if the dependent parameters, integration time,
        /// and range have not been initialized beforehand.
        /// The initialization of these three parameters is mutually exclusive for each one of them.
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
        #endregion

        #region Interrupt

        /// <summary>
        /// Gets whether ambient light sensor interrupts are enabled.
        /// </summary>
        public bool InterruptEnabled
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsIntEn == AlsInterrupt.Enabled;
            }
        }

        /// <summary>
        /// Disables the interrupts.
        /// </summary>
        public void DisableInterrupts()
        {
            _alsConfRegister.Read();
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Configures the interrupt behaviour for the ambient light sensor.
        /// MEHR ERKLÄRUNG!!!
        /// Ist Persistence eine Art von Tiefpass? Wie oft wird die Messung dann ausgeführt?
        /// Bevor any setting is altered the interrupt will be implicitly disabled.
        /// It must be (re-)enabled after configuring.
        /// </summary>
        /// <param name="lowerThreshold">Lower threshold for triggering the interrupt</param>
        /// <param name="upperThreshold">Upper threshold for triggering the interrupt</param>
        /// <param name="persistence">Amount of consecutive hits needed for triggering the interrupt</param>
        public void EnableInterrupts(Illuminance lowerThreshold,
                                     Illuminance upperThreshold,
                                     AlsInterruptPersistence persistence)
        {
            // the maximum detection range and resolution depends on the integration time setting
            _alsConfRegister.Read();
            (Illuminance maxDetectionRange, Illuminance resolution) = GetDetectionRangeAndResolution(_alsConfRegister.AlsIt);

            if (lowerThreshold > maxDetectionRange || upperThreshold > maxDetectionRange)
            {
                throw new ArgumentException($"Lower threshold (is: {lowerThreshold}) or upper threshold (is: {upperThreshold}) must not exceed maximum range of {maxDetectionRange} lux");
            }

            if (lowerThreshold > upperThreshold)
            {
                throw new ArgumentException("Lower threshold (is: {lowerThreshold}) must not be higher than upper threshold  (is: {upperThreshold})");
            }

            // disable interrupts before altering configuration to avoid transient side effects
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();

            _alsLowInterruptThresholdRegister.Threshold = (int)(lowerThreshold.Lux / resolution.Lux);
            _alsHighInterruptThresholdRegister.Threshold = (int)(upperThreshold.Lux / resolution.Lux);
            _alsLowInterruptThresholdRegister.Write();
            _alsHighInterruptThresholdRegister.Write();

            _alsConfRegister.AlsPers = persistence;
            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Gets the interrupt configuration of the ambient light sensor
        /// </summary>
        public (Illuminance LowerThreshold, Illuminance UpperThreshold, AlsInterruptPersistence Persistence) GetInterruptConfiguration()
        {
            _alsLowInterruptThresholdRegister.Read();
            _alsHighInterruptThresholdRegister.Read();
            _alsConfRegister.Read();
            (_, Illuminance resolution) = GetDetectionRangeAndResolution(_alsConfRegister.AlsIt);
            return (Illuminance.FromLux(_alsLowInterruptThresholdRegister.Threshold * resolution.Lux),
                    Illuminance.FromLux(_alsHighInterruptThresholdRegister.Threshold * resolution.Lux),
                    _alsConfRegister.AlsPers);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Helper method to get max detection range and resolution for the currently set integration time.
        /// </summary>
        private (Illuminance MaxDetectionRange, Illuminance Resolution) GetDetectionRangeAndResolution(AlsIntegrationTime integrationTime)
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