// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using Iot.Device.Common;
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

        /// <summary>
        /// Create a DHT10 sensor through I2C
        /// </summary>
        /// <param name="i2cDevice">I2C Device</param>
        public Dht10(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
            i2cDevice.WriteByte(DHT10_CMD_SOFTRESET);
            // make sure DHT10 stable (in the datasheet P7)
            DelayHelper.DelayMilliseconds(20, true);
            i2cDevice.WriteByte(DHT10_CMD_INIT);
        }

        internal override byte[] ReadThroughI2c()
        {
            if (_i2cDevice is null)
            {
                throw new Exception("I2C decvice not configured.");
            }

            // DHT10 has no checksum bits
            _isLastReadSuccessful = true;

            _i2cDevice.WriteByte(DHT10_CMD_START);
            // make sure DHT10 ends measurement (in the datasheet P7)
            DelayHelper.DelayMilliseconds(75, true);

            byte[] data = new byte[6];
            _i2cDevice.Read(data.AsSpan());

            return data;
        }

        internal override RelativeHumidity GetHumidity(Span<byte> readBuff)
        {
            int raw = (((readBuff[1] << 8) | readBuff[2]) << 4) | readBuff[3] >> 4;

            return RelativeHumidity.FromPercent(100.0 * raw / Math.Pow(2, 20));
        }

        internal override Temperature GetTemperature(Span<byte> readBuff)
        {
            int raw = ((((readBuff[3] & 0b_0000_1111) << 8) | readBuff[4]) << 8) | readBuff[5];

            return Temperature.FromDegreesCelsius(raw / Math.Pow(2, 20) * 200 - 50);
        }
    }
}
