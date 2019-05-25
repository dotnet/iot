// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading.Tasks;

namespace Iot.Device.Bmx280
{
    public class Bme280 : BmxBase
    {
        private const byte DeviceId = 0x60;
        public const byte DefaultI2cAddress = 0x77;

        public Bme280(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _deviceId = DeviceId;
            _communicationProtocol = CommunicationProtocol.I2c;
        }

        /// <summary>
        /// Get the current sample rate for humidity measurements.
        /// </summary>
        /// <returns></returns>
        public Sampling ReadHumiditySampling()
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_HUM);
            status = (byte)(status & 0b0000_0111);
            return ByteToSampling(status);
        }

        /// <summary>
        /// Sets the humidity sampling to the given value.
        /// </summary>
        /// <param name="sampling"></param>
        public void SetHumiditySampling(Sampling sampling)
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_HUM);
            status = (byte)(status & 0b1111_1000);
            status = (byte)(status | (byte)sampling);
            _i2cDevice.Write(new[] { (byte)Register.CTRL_HUM, status });

            // Changes to the above register only become effective after a write operation to "CTRL_MEAS".
            byte measureState = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            _i2cDevice.Write(new[] { (byte)Register.CTRL_MEAS, measureState });
        }

        /// <summary>
        /// Reads the Humidity from the sensor as %rH.
        /// </summary>
        /// <returns>
        /// Returns a percentage from 0 to 100.
        /// </returns>
        public async Task<double> ReadHumidityAsync()
        {
            // Make sure the I2C device is initialized
            if (!_initialized)
            {
                Begin();
            }

            if (ReadPowerMode() == PowerMode.Forced)
            {
                await Task.Delay(GetMeasurementTimeForForcedMode(ReadHumiditySampling()));
            }

            // Read the temperature first to load the t_fine value for compensation
            if (TemperatureFine == int.MinValue)
            {
                await ReadTemperatureAsync();
            }

            byte msb = Read8BitsFromRegister((byte)Register.HUMIDDATA_MSB);
            byte lsb = Read8BitsFromRegister((byte)Register.HUMIDDATA_LSB);

            // Combine the values into a 32-bit integer
            int t = (msb << 8) | lsb;

            return CompensateHumidity(t);
        }

        private double CompensateHumidity(int adcHumidity)
        {
            // The humidity is calculated using the compensation formula in the BME280 datasheet
            double varH = TemperatureFine - 76800.0;
            varH = (adcHumidity - (CalibrationData.DigH4 * 64.0 + CalibrationData.DigH5 / 16384.0 * varH)) *
                (CalibrationData.DigH2 / 65536.0 * (1.0 + CalibrationData.DigH6 / 67108864.0 * varH *
                                               (1.0 + CalibrationData.DigH3 / 67108864.0 * varH)));
            varH *= 1.0 - CalibrationData.DigH1 * varH / 524288.0;

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

        internal override CalibrationData ReadCalibrationData()
        {
            return new CalibrationData
            {
                // Read temperature calibration data
                DigT1 = Read16BitsFromRegister((byte)Register.DIG_T1),
                DigT2 = (short)Read16BitsFromRegister((byte)Register.DIG_T2),
                DigT3 = (short)Read16BitsFromRegister((byte)Register.DIG_T3),

                // Read pressure calibration data
                DigP1 = Read16BitsFromRegister((byte)Register.DIG_P1),
                DigP2 = (short)Read16BitsFromRegister((byte)Register.DIG_P2),
                DigP3 = (short)Read16BitsFromRegister((byte)Register.DIG_P3),
                DigP4 = (short)Read16BitsFromRegister((byte)Register.DIG_P4),
                DigP5 = (short)Read16BitsFromRegister((byte)Register.DIG_P5),
                DigP6 = (short)Read16BitsFromRegister((byte)Register.DIG_P6),
                DigP7 = (short)Read16BitsFromRegister((byte)Register.DIG_P7),
                DigP8 = (short)Read16BitsFromRegister((byte)Register.DIG_P8),
                DigP9 = (short)Read16BitsFromRegister((byte)Register.DIG_P9),

                // Read humidity calibration data
                DigH1 = Read8BitsFromRegister((byte)Register.DIG_H1),
                DigH2 = (short)Read16BitsFromRegister((byte)Register.DIG_H2),
                DigH3 = Read8BitsFromRegister((byte)Register.DIG_H3),
                DigH4 = (short)((Read8BitsFromRegister((byte)Register.DIG_H4) << 4) | (Read8BitsFromRegister((byte)Register.DIG_H4 + 1) & 0xF)),
                DigH5 = (short)((Read8BitsFromRegister((byte)Register.DIG_H5 + 1) << 4) | (Read8BitsFromRegister((byte)Register.DIG_H5) >> 4)),
                DigH6 = (sbyte)Read8BitsFromRegister((byte)Register.DIG_H6)
            };
        }
    }
}
