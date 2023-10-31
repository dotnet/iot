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
    /// Initializes a new instance of the <see cref="AmbientLightSensor"/> API class.
    /// </summary>
    public class AmbientLightSensor
    {
        private AlsConfRegister _alsConfRegister;
        private AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private AlsDataRegister _alsDataRegister;

        private bool _interruptIsConfigured = false;

        internal AmbientLightSensor(I2cInterface i2cBus)
        {
            _alsConfRegister = new AlsConfRegister(i2cBus);
            _alsHighInterruptThresholdRegister = new AlsHighInterruptThresholdRegister(i2cBus);
            _alsLowInterruptThresholdRegister = new AlsLowInterruptThresholdRegister(i2cBus);
            _alsDataRegister = new AlsDataRegister(i2cBus);
        }

        #region General

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
        /// Note: the update interval depends on the integration time and persistence setting.
        ///       Interval = integration time * persistence, e.g. 320 ms * 4 = 1280 ms.
        /// </summary>
        public Illuminance Reading
        {
            get
            {
                _alsDataRegister.Read();
                return Illuminance.FromLux(_alsDataRegister.Data / 10);
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Gets and sets the ALS integration time.
        /// Note: changing the integration time will implicitly change the
        ///       depending detection range and resolution, hence the
        ///       corresponding properties.
        /// Important: the integration time setting influences detection range,
        ///            hence may lead to an invalid interrupt threshold setting.
        ///            Therefore changing the integration time will disable the
        ///            ALS interrupt and require to re-configure and re-enable
        ///            the interrupt.
        ///            This is only valid if the ALS interrupt is used.
        /// </summary>
        public AlsIntegrationTime IntegrationTime
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsIt;
            }

            set
            {
                _alsConfRegister.Read();
                if (_alsConfRegister.AlsIt == value)
                {
                    return;
                }

                InterruptEnabled = false;
                _interruptIsConfigured = false;
                _alsConfRegister.AlsIt = value;
                _alsConfRegister.Write();
            }
        }

        /// <summary>
        /// Gets or sets the detection range.
        /// Note: changing the detection range will implicitly change the
        ///       depending integration time and resolution, hence the
        ///       corresponding properties.
        /// Important: changing the detection range may lead to an invalid
        ///            interrupt threshold setting.
        ///            Therefore changing the detection range will disable the
        ///            ALS interrupt and require to re-configure and re-enable
        ///            the interrupt.
        ///            This is only valid if the ALS interrupt is used.
        /// </summary>
        public AlsRange Range
        {
            get
            {
                return IntegrationTime switch
                {
                    AlsIntegrationTime.Time80ms => AlsRange.Range_6553,
                    AlsIntegrationTime.Time160ms => AlsRange.Range_3276,
                    AlsIntegrationTime.Time320ms => AlsRange.Range_1638,
                    AlsIntegrationTime.Time640ms => AlsRange.Range_819,
                    _ => throw new NotImplementedException(),
                };
            }

            set
            {
                IntegrationTime = value switch
                {
                    AlsRange.Range_819 => AlsIntegrationTime.Time640ms,
                    AlsRange.Range_1638 => AlsIntegrationTime.Time320ms,
                    AlsRange.Range_3276 => AlsIntegrationTime.Time160ms,
                    AlsRange.Range_6553 => AlsIntegrationTime.Time80ms,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        /// <summary>
        /// Gets or sets the resolution.
        /// Note: changing the resolution will implicitly change the
        ///       depending integration time and detection range, hence the
        ///       corresponding properties.
        /// Important: changing the resolution may lead to an invalid
        ///            interrupt threshold setting.
        ///            Therefore changing the resolution will disable the
        ///            ALS interrupt and require to re-configure and re-enable
        ///            the interrupt.
        ///            This is only valid if the ALS interrupt is used.
        /// </summary>
        public AlsResolution Resolution
        {
            get
            {
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
        /// Enables or disables the interrupt of the ambient light sensor
        /// </summary>
        public bool InterruptEnabled
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsIntEn == AlsInterrupt.Enabled;
            }
            set
            {
                if (value && !_interruptIsConfigured)
                {
                    throw new InvalidOperationException("The interrupt must be configured before enabling it.");
                }

                _alsConfRegister.AlsIntEn = value ? AlsInterrupt.Enabled : AlsInterrupt.Disabled;
            }
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
        public void ConfigureInterrupt(Illuminance lowerThreshold,
                                       Illuminance upperThreshold,
                                       AlsInterruptPersistence persistence)
        {
            _interruptIsConfigured = false;

            // the maximum detection range and resolution depends on the integration time setting
            _alsConfRegister.Read();
            (Illuminance maxDetectionRange, Illuminance resolution) = GetMaxDetectionRangeAndResolution();

            if (lowerThreshold > maxDetectionRange)
            {
                throw new ArgumentException($"Lower threshold exceed maximum detection range ({maxDetectionRange})");
            }

            if (upperThreshold > maxDetectionRange)
            {
                throw new ArgumentException($"Upper threshold exceed maximum detection range ({maxDetectionRange})");
            }

            if (lowerThreshold > upperThreshold)
            {
                throw new ArgumentException("Lower threshold is higher than upper threshold");
            }

            _alsLowInterruptThresholdRegister.Threshold = (int)(lowerThreshold.Lux * resolution.Lux);
            _alsHighInterruptThresholdRegister.Threshold = (int)(upperThreshold.Lux * resolution.Lux);
            _alsLowInterruptThresholdRegister.Write();
            _alsHighInterruptThresholdRegister.Write();

            _alsConfRegister.AlsPers = persistence;
            _alsConfRegister.Write();

            _interruptIsConfigured = true;
        }

        /// <summary>
        /// Gets the interrupt configuration from the device
        /// </summary>
        public (bool IsConfigured, Illuminance LowerThreshold, Illuminance UpperThreshold, AlsInterruptPersistence Persistence) GetInterruptConfiguration()
        {
            if (!_interruptIsConfigured)
            {
                return (false, Illuminance.Zero, Illuminance.Zero, AlsInterruptPersistence.Persistence1);
            }

            _alsLowInterruptThresholdRegister.Read();
            _alsHighInterruptThresholdRegister.Read();
            _alsConfRegister.Read();
            return (true,
                    Illuminance.FromLux(_alsLowInterruptThresholdRegister.Threshold / 10),
                    Illuminance.FromLux(_alsHighInterruptThresholdRegister.Threshold / 10),
                    _alsConfRegister.AlsPers);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Helper method to get max detection range and resolution for the currently set integration time.
        /// </summary>
        public (Illuminance MaxDetectionRange, Illuminance Resolution) GetMaxDetectionRangeAndResolution()
        {
            _alsConfRegister.Read();
            return _alsConfRegister.AlsIt switch
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
