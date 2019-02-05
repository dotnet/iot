// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Runtime.InteropServices;

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Analog-to-Digital Converter ADS1115
    /// </summary>
    public class Ads1115 : IDisposable
    {
        const byte ADC_CONVERSION_REG_ADDR = 0x00;
        const byte ADC_CONFIG_REG_ADDR = 0x01;

        private I2cDevice _sensor = null;

        private readonly byte _adcMux;
        private readonly byte _adcPga;
        private readonly byte _adcRate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="mux">Input Multiplexer</param>
        /// <param name="pga">Programmable Gain Amplifier</param>
        /// <param name="rate">Data Rate</param>
        public Ads1115(I2cDevice sensor, InputMultiplexeConfig mux = InputMultiplexeConfig.AIN0, PgaConfig pga = PgaConfig.FS4096, DataRate rate = DataRate.SPS128)
        {
            _sensor = sensor;
            _adcMux = (byte)mux;
            _adcPga = (byte)pga;
            _adcRate = (byte)rate;

            Initialize();
        }

        /// <summary>
        /// Initialize ADS1115
        /// </summary>
        private void Initialize()
        {
            byte configHi = (byte)((_adcMux << 4) +
                            (_adcPga << 1) +
                            (byte)DeviceMode.Continuous);

            byte configLo = (byte)((_adcRate << 5) +
                            ((byte)(ComparatorMode.Traditional) << 4) +
                            ((byte)ComparatorPolarity.Low << 3) +
                            ((byte)ComparatorLatching.NonLatching << 2) +
                            (byte)ComparatorQueue.Disable);

            _sensor.Write(new byte[] { ADC_CONFIG_REG_ADDR, configHi, configLo });
        }

        /// <summary>
        /// Read Raw Data
        /// </summary>
        /// <returns>Raw Value</returns>
        public short ReadRaw()
        {
            short val;
            var data = new byte[2];

            _sensor.Write(new byte[] { ADC_CONVERSION_REG_ADDR });
            _sensor.Read(data);

            Array.Reverse(data);
            val = BitConverter.ToInt16(data, 0);

            return val;
        }

        /// <summary>
        /// Convert Raw Data to Voltage
        /// </summary>
        /// <param name="val">Raw Data</param>
        /// <returns>Voltage</returns>
        public double RawToVoltage(short val)
        {
            double voltage;
            double resolution;

            if (_adcPga == 0x00)
            {
                voltage = 6.144;
            }
            else if (_adcPga == 0x01)
            {
                voltage = 4.096;
            }
            else if (_adcPga == 0x02)
            {
                voltage = 2.048;
            }
            else if (_adcPga == 0x03)
            {
                voltage = 1.024;
            }
            else if (_adcPga == 0x04)
            {
                voltage = 0.512;
            }
            else
            {
                voltage = 0.256;
            }

            if (_adcMux <= 0x03)
            {
                resolution = 65535.0;
            }
            else
            {
                resolution = 32768.0;
            }

            return val * (voltage / resolution);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
                _sensor = null;
            }
        }
    }
}