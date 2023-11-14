// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Defnitions;
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
        }

        #region General

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
                    _localIntegrationTime = _alsConfRegister.AlsIt;
                }

                _loadReductionModeEnabled = value;
            }
        }

        /// <summary>
        /// Get or sets the power state of the ambient light sensor.
        /// <value><c>true</c> powered up;<c>false</c> shutdown</value>
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
        /// Important: when setting the integration time, possibly configured
        ///            interrupts are implicitly deactivated.
        ///            This is done to prevent accidentally using changed threshold
        ///            values (based on the integration time).
        ///            The user must explicitly reconfigure and activate the interrupts.
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
        public void EnableInterrupts(AmbientLightInterruptConfiguration configuration)
        {
            // the maximum detection range and resolution depends on the integration time setting
            _alsConfRegister.Read();
            (Illuminance maxDetectionRange, Illuminance resolution) = GetDetectionRangeAndResolution(_alsConfRegister.AlsIt);

            if (configuration.LowerThreshold.Lux < 0 || configuration.UpperThreshold.Lux < 0)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) and upper threshold (is: {configuration.UpperThreshold}) must be positive");
            }

            if (configuration.LowerThreshold > maxDetectionRange || configuration.UpperThreshold > maxDetectionRange)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) or upper threshold (is: {configuration.UpperThreshold}) must not exceed maximum range of {maxDetectionRange} lux");
            }

            if (configuration.LowerThreshold > configuration.UpperThreshold)
            {
                throw new ArgumentException($"Lower threshold (is: {configuration.LowerThreshold}) must not be higher than upper threshold  (is: {configuration.UpperThreshold})");
            }

            // disable interrupts before altering configuration to avoid transient side effects
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();

            _alsLowInterruptThresholdRegister.Threshold = (int)(configuration.LowerThreshold.Lux / resolution.Lux);
            _alsHighInterruptThresholdRegister.Threshold = (int)(configuration.UpperThreshold.Lux / resolution.Lux);
            _alsLowInterruptThresholdRegister.Write();
            _alsHighInterruptThresholdRegister.Write();

            _alsConfRegister.AlsPers = configuration.Persistence;
            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Gets the interrupt configuration of the ambient light sensor
        /// </summary>
        public AmbientLightInterruptConfiguration GetInterruptConfiguration()
        {
            _alsLowInterruptThresholdRegister.Read();
            _alsHighInterruptThresholdRegister.Read();
            _alsConfRegister.Read();
            (_, Illuminance resolution) = GetDetectionRangeAndResolution(_alsConfRegister.AlsIt);
            return new(Illuminance.FromLux(_alsLowInterruptThresholdRegister.Threshold * resolution.Lux),
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
