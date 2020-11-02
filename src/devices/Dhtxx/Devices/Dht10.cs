// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT10
    /// </summary>
    public class Dht10 : DhtBase
    {
        /// <summary>
        /// DHT10 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x38;

        private const byte DHT10_CMD_INIT = 0b_1110_0001;
        private const byte DHT10_CMD_START = 0b_1010_1100;
        private const byte DHT10_CMD_SOFTRESET = 0b_1011_1010;

        // state, humi[20-13], humi[12-5], humi[4-1]temp[20-17], temp[16-9], temp[8-1]
        private byte[] _dht10ReadBuff = new byte[6];

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public override Ratio Humidity
        {
            get
            {
                ReadData();
                return GetHumidity(_dht10ReadBuff);
            }
        }

        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public override Temperature Temperature
        {
            get
            {
                ReadData();
                return GetTemperature(_dht10ReadBuff);
            }
        }

        /// <summary>
        /// Create a DHT10 sensor through I2C
        /// </summary>
        /// <param name="i2cDevice">I2C Device</param>
        public Dht10(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
            _i2cDevice.WriteByte(DHT10_CMD_SOFTRESET);
            // make sure DHT10 stable (in the datasheet P7)
            DelayHelper.DelayMilliseconds(20, true);
            _i2cDevice.WriteByte(DHT10_CMD_INIT);
        }

        internal override void ReadThroughI2c()
        {
            // DHT10 has no calibration bits
            IsLastReadSuccessful = true;

            _i2cDevice.WriteByte(DHT10_CMD_START);
            // make sure DHT10 ends measurement (in the datasheet P7)
            DelayHelper.DelayMilliseconds(75, true);

            _i2cDevice.Read(_dht10ReadBuff);
        }

        internal override Ratio GetHumidity(byte[] readBuff)
        {
            int raw = (((readBuff[1] << 8) | readBuff[2]) << 4) | readBuff[3] >> 4;

            return Ratio.FromDecimalFractions(raw / Math.Pow(2, 20));
        }

        internal override Temperature GetTemperature(byte[] readBuff)
        {
            int raw = ((((readBuff[3] & 0b_0000_1111) << 8) | readBuff[4]) << 8) | readBuff[5];

            return Temperature.FromDegreesCelsius(raw / Math.Pow(2, 20) * 200 - 50);
        }
    }
}
