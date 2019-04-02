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

        private Config _config;
        /// <summary>
        /// Returns the last set ADS1115 config
        /// </summary>
        public Config Config
        {
            get => _config;
            set
            {
                _config = value;
                WriteConfig();
            }
        }

        /// <summary>
        /// Initialize a new Ads1115 device connected through I2C
        /// Default Config Register as per datasheet 0x8583
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public Ads1115(I2cDevice sensor)
        {
            _sensor = sensor;

            // Initalise ADS1115 default hardware config as per datasheet, Default Config Register = 0x8583
            _config = (Config.ADS1015_REG_CONFIG_OS_NOTBUSY | // 0x0xxx prevents SingleShot trigger on init, but device will have Config 0x1xxx bit set as not performing a conversion
                Config.ADS1015_REG_CONFIG_MUX_DIFF_0_1 | Config.ADS1015_REG_CONFIG_PGA_2_048V |
                Config.ADS1015_REG_CONFIG_MODE_SINGLE | Config.ADS1015_REG_CONFIG_DR_8SPS |
                Config.ADS1015_REG_CONFIG_CMODE_TRAD | Config.ADS1015_REG_CONFIG_CPOL_ACTVLOW |
                Config.ADS1015_REG_CONFIG_CLAT_NONLAT | Config.ADS1015_REG_CONFIG_CQUE_NONE);

            Initialize();
        }

        /// <summary>
        /// Initialize a new Ads1115 device connected through I2C and sets Config Register
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="config">Config Register/param>
        public Ads1115(I2cDevice sensor, Config config)
        {
            _sensor = sensor;

            // Set Config Register to config params passed in Constructor
            _config = config;

            Initialize();
        }

        /// <summary>
        /// Initialize ADS1115
        /// </summary>
        private void Initialize()
        {
            WriteConfig();
        }

        /// <summary>
        /// Writes config to ADS1115 Config Register
        /// </summary>
        private void WriteConfig()
        {
            // Details in Datasheet P18
            byte configHi = (byte)(((ushort)_config) >> 8);
            byte configLo = (byte)(((ushort)_config) & 0x00FF);

            Span<byte> writeBuff = stackalloc byte[3] { (byte)Registers.ADC_CONFIG_REG_ADDR, configHi, configLo };

            _sensor.Write(writeBuff);
        }

        /// <summary>
        /// Returns the current config from the ADS1115 Config Register
        /// </summary>
        /// <returns></returns>
        public Config ReadConfig()
        {
            Config result;
            Span<byte> readBuff = stackalloc byte[2];

            _sensor.WriteByte((byte)Registers.ADC_CONFIG_REG_ADDR);
            _sensor.Read(readBuff);

            result = (Config)BinaryPrimitives.ReadInt16BigEndian(readBuff);

            return result;
        }

        /// <summary>
        /// Sets paramater passed & writes config to ADS1115 Config Register
        /// </summary>
        /// <param name="param">Config Register parameter to set</param>
        /// <param name="mask">Config Register mask used when setting parameter</param>
        public void SetConfig(Config param, Config mask)
        {
            _config = (~mask & _config) | param;

            WriteConfig();
        }

        /// <summary>
        /// Returns the Config Parameter passed in mask
        /// </summary>
        /// <param name="mask">Config Register mask used when returning parameter</param>
        /// <returns>Config</returns>
        public Config GetConfig(Config mask)
        {
            return (this._config & mask);
        }

        /// <summary>
        /// Read Raw Data from ADS1115's Config Register
        /// </summary>
        /// <returns>Raw Value</returns>
        public short ReadRaw()
        {
            short val;
            Span<byte> readBuff = stackalloc byte[2];

            // Only for SingleShot mode, set the OS register to begin a single shot conversion
            if ((this._config & Config.ADS1015_REG_CONFIG_MODE_SINGLE) == Config.ADS1015_REG_CONFIG_MODE_SINGLE)
            {
                // Set single shot conversion start flag
                SetConfig(Config.ADS1015_REG_CONFIG_OS_SINGLE, Config.ADS1015_REG_CONFIG_OS_MASK);

                // Block for conversion (depends on the SPS DataRate set, typically 125ms (8SPS) to 1ms (860SPS)
                do
                {
                    this._sensor.WriteByte((byte)Registers.ADC_CONFIG_REG_ADDR);
                    this._sensor.Read(readBuff);
                }
                while ((BinaryPrimitives.ReadInt16BigEndian(readBuff) & (ushort)Config.ADS1015_REG_CONFIG_OS_MASK) == (ushort)Config.ADS1015_REG_CONFIG_OS_BUSY);
            }

            // Read the conversion register
            this._sensor.WriteByte((byte)Registers.ADC_CONVERSION_REG_ADDR);
            this._sensor.Read(readBuff);

            val = BinaryPrimitives.ReadInt16BigEndian(readBuff);

            return val;
        }

        /// <summary>
        /// Convert Raw Data to voltage depending on PGA Gain set
        /// </summary>
        /// <param name="val">Raw Data</param>
        /// <returns>Voltage</returns>
        public double RawToVoltage(short val)
        {
            double mvPerBit;

            // Calculate voltage pased on FullScale constant (+- 15bit)
            switch (GetConfig(Config.ADS1015_REG_CONFIG_PGA_MASK))
            {
                case Config.ADS1015_REG_CONFIG_PGA_6_144V:
                    mvPerBit = 6.144 / 0x8000;
                    break;
                case Config.ADS1015_REG_CONFIG_PGA_4_096V:
                    mvPerBit = 4.096 / 0x8000;
                    break;
                case Config.ADS1015_REG_CONFIG_PGA_2_048V:
                    mvPerBit = 2.048 / 0x8000;
                    break;
                case Config.ADS1015_REG_CONFIG_PGA_1_024V:
                    mvPerBit = 1.024 / 0x8000;
                    break;
                case Config.ADS1015_REG_CONFIG_PGA_0_512V:
                    mvPerBit = 0.512 / 0x8000;
                    break;
                case Config.ADS1015_REG_CONFIG_PGA_0_256V:
                    mvPerBit = 0.256 / 0x8000;
                    break;
                default:
                    mvPerBit = 0;
                    break;
            }

            return val * mvPerBit;
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
