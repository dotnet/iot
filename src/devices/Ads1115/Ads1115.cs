// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Analog-to-Digital Converter ADS1115
    /// </summary>
    public class Ads1115 : IDisposable
    {
        private I2cDevice _sensor = null;

        private readonly byte _inputMultiplexer;
        private readonly byte _measuringRange;
        private readonly byte _dataRate;

        /// <summary>
        /// Initialize a new Ads1115 device connected through I2C
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="inputMultiplexer">Input Multiplexer</param>
        /// <param name="measuringRange">Programmable Gain Amplifier</param>
        /// <param name="dataRate">Data Rate</param>
        public Ads1115(I2cDevice sensor, InputMultiplexer inputMultiplexer = InputMultiplexer.AIN0, MeasuringRange measuringRange = MeasuringRange.FS4096, DataRate dataRate = DataRate.SPS128)
        {
            _sensor = sensor;
            _inputMultiplexer = (byte)inputMultiplexer;
            _measuringRange = (byte)measuringRange;
            _dataRate = (byte)dataRate;

            Initialize();
        }

        /// <summary>
        /// Initialize ADS1115
        /// </summary>
        private void Initialize()
        {
            // Details in Datasheet P18
            byte configHi = (byte)((_inputMultiplexer << 4) +
                            (_measuringRange << 1) +
                            (byte)DeviceMode.Continuous);

            byte configLo = (byte)((_dataRate << 5) +
                            ((byte)(ComparatorMode.Traditional) << 4) +
                            ((byte)ComparatorPolarity.Low << 3) +
                            ((byte)ComparatorLatching.NonLatching << 2) +
                            (byte)ComparatorQueue.Disable);

            _sensor.Write(new [] { (byte)Register.ADC_CONFIG_REG_ADDR, configHi, configLo });
        }

        /// <summary>
        /// Read Raw Data
        /// </summary>
        /// <returns>Raw Value</returns>
        public short ReadRaw()
        {
            short val;
            Span<byte> data = stackalloc byte[2];

            _sensor.Write(new [] { (byte)Register.ADC_CONVERSION_REG_ADDR });
            _sensor.Read(data);

            val = BinaryPrimitives.ReadInt16BigEndian(data);

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

            switch ((MeasuringRange)_measuringRange)
            {
                case MeasuringRange.FS6144:
                    voltage = 6.144;
                    break;
                case MeasuringRange.FS4096:
                    voltage = 4.096;
                    break;
                case MeasuringRange.FS2048:
                    voltage = 2.048;
                    break;
                case MeasuringRange.FS1024:
                    voltage = 1.024;
                    break;
                case MeasuringRange.FS0512:
                    voltage = 0.512;
                    break;
                case MeasuringRange.FS0256:
                    voltage = 0.256;
                    break;
                default:
                    voltage = 0;
                    break;
            }

            if (_inputMultiplexer <= 0x03)
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
