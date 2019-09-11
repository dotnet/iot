// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Iot.Device.Bmxx80.CalibrationData;
using Iot.Device.Bmxx80.Register;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents a BME280 temperature, barometric pressure and humidity sensor.
    /// </summary>
    public sealed class Bme280 : Bmx280Base
    {
        /// <summary>
        /// The expected chip ID of the BME280.
        /// </summary>
        private const byte DeviceId = 0x60;

        /// <summary>
        /// Calibration data for the <see cref="Bme680"/>.
        /// </summary>
        private Bme280CalibrationData _bme280Calibration;

        private Sampling _humiditySampling;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bme280"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bme280(I2cDevice i2cDevice)
            : base(DeviceId, i2cDevice)
        {
            _bme280Calibration = (Bme280CalibrationData)_calibrationData;
            _communicationProtocol = CommunicationProtocol.I2c;
        }

        /// <summary>
        /// Gets or sets the humidity sampling.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Sampling"/> is set to an undefined mode.</exception>
        public Sampling HumiditySampling
        {
            get => _humiditySampling;
            set
            {
                if (!Enum.IsDefined(typeof(Sampling), value))
                    throw new ArgumentOutOfRangeException();

                byte status = Read8BitsFromRegister((byte)Bme280Register.CTRL_HUM);
                status = (byte)(status & 0b_1111_1000);
                status = (byte)(status | (byte)value);

                Span<byte> command = stackalloc[] {(byte)Bme280Register.CTRL_HUM, status};
                _i2cDevice.Write(command);

                // Changes to the above register only become effective after a write operation to "CTRL_MEAS".
                byte measureState = Read8BitsFromRegister((byte)Bmx280Register.CTRL_MEAS);

                command = stackalloc[] {(byte)Bmx280Register.CTRL_MEAS, measureState};
                _i2cDevice.Write(command);
                _humiditySampling = value;
            }
        }

        /// <summary>
        /// Reads the humidity. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="humidity">
        /// Contains the measured humidity as %rH if the <see cref="HumiditySampling"/> was not set to <see cref="Sampling.Skipped"/>.
        /// Contains <see cref="double.NaN"/> otherwise.
        /// </param>
        /// <returns><code>true</code> if measurement was not skipped, otherwise <code>false</code>.</returns>
        public bool TryReadHumidity(out double humidity)
        {
            if (HumiditySampling == Sampling.Skipped)
            {
                humidity = double.NaN;
                return false;
            }

            // Read the temperature first to load the t_fine value for compensation.
            TryReadTemperature(out _);

            var hum = Read16BitsFromRegister((byte)Bme280Register.HUMIDDATA, Endianness.BigEndian);

            humidity = CompensateHumidity(hum);
            return true;
        }

        /// <summary>
        /// Gets the required time in ms to perform a measurement with the current sampling modes.
        /// </summary>
        /// <returns>The time it takes for the chip to read data in milliseconds rounded up.</returns>
        public override int GetMeasurementDuration()
        {
            return s_osToMeasCycles[(int)PressureSampling] + s_osToMeasCycles[(int)TemperatureSampling] + s_osToMeasCycles[(int)HumiditySampling];
        }

        /// <summary>
        /// Sets the default configuration for the sensor.
        /// </summary>
        protected override void SetDefaultConfiguration()
        {
            base.SetDefaultConfiguration();
            HumiditySampling = Sampling.UltraLowPower;
        }

        /// <summary>
        /// Compensates the humidity.
        /// </summary>
        /// <param name="adcHumidity">The humidity value read from the device.</param>
        /// <returns>The percentage relative humidity.</returns>
        private double CompensateHumidity(int adcHumidity)
        {
            // The humidity is calculated using the compensation formula in the BME280 datasheet.
            double varH = TemperatureFine - 76800.0;
            varH = (adcHumidity - (_bme280Calibration.DigH4 * 64.0 + _bme280Calibration.DigH5 / 16384.0 * varH)) *
                (_bme280Calibration.DigH2 / 65536.0 * (1.0 + _bme280Calibration.DigH6 / 67108864.0 * varH *
                                               (1.0 + _bme280Calibration.DigH3 / 67108864.0 * varH)));
            varH *= 1.0 - _bme280Calibration.DigH1 * varH / 524288.0;

            if (varH > 100)
            {
                varH = 100;
            }
            else if (varH < 0)
            {
                varH = 0;
            }

            return varH;
        }
    }
}
