// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Analog-to-Digital Converter ADS1115
    /// </summary>
    public class Ads1115 : IDisposable
    {
        private I2cDevice _sensor = null;

        private InputMultiplexer _inputMultiplexer;
        /// <summary>
        /// ADS1115 Input Multiplexer
        /// </summary>
        public InputMultiplexer InputMultiplexer
        {
            get => _inputMultiplexer;
            set
            {
                _inputMultiplexer = value;
                SetConfig();
            }
        }

        private MeasuringRange _measuringRange;
        /// <summary>
        /// ADS1115 Programmable Gain Amplifier
        /// </summary>
        public MeasuringRange MeasuringRange
        {
            get => _measuringRange;
            set
            {
                _measuringRange = value;
                SetConfig();
            }
        }

        private DataRate _dataRate;
        /// <summary>
        /// ADS1115 Data Rate
        /// </summary>
        public DataRate DataRate
        {
            get => _dataRate;
            set
            {
                _dataRate = value;
                SetConfig();
            }
        }

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
            _inputMultiplexer = inputMultiplexer;
            _measuringRange = measuringRange;
            _dataRate = dataRate;

            SetConfig();
        }

        /// <summary>
        /// Set ADS1115 Config Register
        /// </summary>
        private void SetConfig()
        {
            // Details in Datasheet P18
            byte configHi = (byte)(((byte)_inputMultiplexer << 4) |
                            ((byte)_measuringRange << 1) |
                            (byte)DeviceMode.Continuous);

            byte configLo = (byte)(((byte)_dataRate << 5) |
                            ((byte)(ComparatorMode.Traditional) << 4) |
                            ((byte)ComparatorPolarity.Low << 3) |
                            ((byte)ComparatorLatching.NonLatching << 2) |
                            (byte)ComparatorQueue.Disable);

            Span<byte> writeBuff = stackalloc byte[3] { (byte)Register.ADC_CONFIG_REG_ADDR, configHi, configLo };

            _sensor.Write(writeBuff);

            // waiting for the sensor stability
            Thread.Sleep(10);
        }

        /// <summary>
        /// Read Raw Data
        /// </summary>
        /// <returns>Raw Value</returns>
        public short ReadRaw()
        {
            short val;
            Span<byte> readBuff = stackalloc byte[2];

            _sensor.WriteByte((byte)Register.ADC_CONVERSION_REG_ADDR);
            _sensor.Read(readBuff);

            val = BinaryPrimitives.ReadInt16BigEndian(readBuff);

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

            if ((byte)_inputMultiplexer <= 0x03)
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
