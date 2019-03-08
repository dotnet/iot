// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Iot.Units;

namespace Iot.Device.Dht12
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT12
    /// </summary>
    public class Dht12 : IDisposable
    {
        private I2cDevice _sensor;

        /// <summary>
        /// DHT12 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x5C;

        private double _temperature;
        /// <summary>
        /// DHT12 Temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                GetHumiAndTemp();
                return Temperature.FromCelsius(_temperature);
            }
        }

        private double _humidity;
        /// <summary>
        /// DHT12 Humidity
        /// </summary>
        public double Humidity
        {
            get
            {
                GetHumiAndTemp();
                return _humidity;
            }
        }

        /// <summary>
        /// Creates a new instance of the DHT12
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public Dht12(I2cDevice sensor)
        {
            _sensor = sensor;
        }

        /// <summary>
        /// Get DHT12 Humidity and Temperature
        /// </summary>
        private void GetHumiAndTemp()
        {
            // humidity int, humidity decimal, temperature int, temperature decimal, checksum
            Span<byte> readBuff = stackalloc byte[5];

            _sensor.WriteByte((byte)Register.DHT_HUMI_INT);
            _sensor.Read(readBuff);

            // checksum error
            if ((byte)(readBuff[0]+readBuff[1]+readBuff[2]+readBuff[3]) != readBuff[4])
            {
                return;
            }

            _humidity = readBuff[0] + readBuff[1] * 0.1;
            // bit[7] = 1 represents a negative temperature
            _temperature = (readBuff[3] & 0x80) == 0 ? readBuff[2] + (readBuff[3] & 0x7F) * 0.1 : -(readBuff[2] + (readBuff[3] & 0x7F) * 0.1);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }
    }
}
