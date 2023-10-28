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
    /// Initializes a new instance of the <see cref="AmbientLightSensor"/> component.
    /// </summary>
    public class AmbientLightSensor
    {
        private AlsConfRegister _alsConfRegister;
        private AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private AlsDataRegister _alsDataRegister;

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
        /// ADD MORE DETAILS
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
                _alsConfRegister.AlsIt = value;
                _alsConfRegister.Write();
            }
        }

        /// <summary>
        /// Configures the interrupt behaviour for the ambient light sensor.
        /// MEHR ERKLÄRUNG!!!
        /// Ist Persistence eine Art von Tiefpass? Wie oft wird die Messung dann ausgeführt?
        /// </summary>
        /// <param name="lowerThreshold">Lower threshold for triggering the interrupt</param>
        /// <param name="uppderTreshold">Upper threshold for triggering the interrupt</param>
        /// <param name="persistence">Amount of consecutive hits needed for triggering the interrupt</param>
        public void ConfigureInterrupt(Illuminance lowerThreshold,
                                       Illuminance upperThreshold,
                                       AlsInterruptPersistence persistence)
        {
            // the maximum detection range and resolution depends on the integration time setting
            _alsConfRegister.Read();
            Illuminance max = _alsConfRegister.AlsIt switch
            {
                AlsIntegrationTime.Time80ms => Illuminance.FromLux(6553.5),
                AlsIntegrationTime.Time160ms => Illuminance.FromLux(3276.8),
                AlsIntegrationTime.Time320ms => Illuminance.FromLux(1638.4),
                AlsIntegrationTime.Time640ms => Illuminance.FromLux(819.2),
                _ => throw new NotImplementedException(),
            };

            if (lowerThreshold > max)
            {
                throw new ArgumentException($"Lower threshold exceed maximum detection range ({max})");
            }

            if (upperThreshold > max)
            {
                throw new ArgumentException($"Upper threshold exceed maximum detection range ({max})");
            }

            if (lowerThreshold > upperThreshold)
            {
                throw new ArgumentException("Lower threshold is higher than upper threshold");
            }
        }

        /// <summary>
        /// BLA BLA
        /// </summary>
        /// <returns></returns>
        public int GetAlsReading()
        {
            _alsDataRegister.Read();
            return _alsDataRegister.Data;
        }
    }
}
