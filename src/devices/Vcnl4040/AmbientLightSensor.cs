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
        private bool _interruptEnabled;

        internal AmbientLightSensor(I2cInterface i2cBus)
        {
            _alsConfRegister = new AlsConfRegister(i2cBus);
            _alsHighInterruptThresholdRegister = new AlsHighInterruptThresholdRegister(i2cBus);
            _alsLowInterruptThresholdRegister = new AlsLowInterruptThresholdRegister(i2cBus);
            _alsDataRegister = new AlsDataRegister(i2cBus);
        }

        /// <summary>
        /// Gets and sets the ALS power state.
        /// ADD MORE DETAILS
        /// </summary>
        public PowerState PowerState
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsSd;
            }

            set
            {
                _alsConfRegister.Read();
                _alsConfRegister.AlsSd = value;
                _alsConfRegister.Write();
            }
        }

        /// <summary>
        /// Gets and sets the ALS integration time.
        /// Important: the integration time setting influences the interrupt
        ///            threshold detection range. Therefore changing the
        ///            integration time will disable the ALS interrupt and
        ///            require to re-configure and re-enable the interrupt.
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
                InterruptEnabled = false;
                _interruptIsConfigured = false;
                _alsConfRegister.Read();
                _alsConfRegister.AlsIt = value;
                _alsConfRegister.Write();
            }
        }

        /// <summary>
        /// Enables or disables the interrupt of the ambient light sensor
        /// </summary>
        public bool InterruptEnabled
        {
            get => _interruptEnabled;
            set
            {
                if (value && !_interruptIsConfigured)
                {
                    throw new InvalidOperationException("The interrupt must be configured before enabling it.");
                }

                _alsConfRegister.AlsIntEn = value ? AlsInterrupt.Enabled : AlsInterrupt.Disabled;
                _interruptEnabled = value;
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

        /// <summary>
        /// BLA BLA
        /// </summary>
        /// <returns></returns>
        public Illuminance GetAlsReading()
        {
            _alsDataRegister.Read();
            return Illuminance.FromLux(_alsDataRegister.Data / 10);
        }

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
    }
}
