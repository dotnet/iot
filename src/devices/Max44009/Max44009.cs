// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Max44009
{
    /// <summary>
    /// Ambient Light Sensor MAX44009
    /// </summary>
    public class Max44009 : IDisposable
    {
        private I2cDevice _sensor;

        /// <summary>
        /// MAX44009 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x4A;

        /// <summary>
        /// MAX44009 Illuminance (Lux)
        /// </summary>
        public double Illuminance { get => GetIlluminance(); }

        /// <summary>
        /// Creates a new instance of the MAX44009, MAX44009 working mode is default. (Consume lowest power)
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public Max44009(I2cDevice sensor)
        {
            _sensor = sensor;

            // Details in the Datasheet P8
            Span<byte> writeBuff = stackalloc byte[2] { (byte)Register.MAX_CONFIG, 0b_0000_0000 };

            _sensor.Write(writeBuff);
        }

        /// <summary>
        /// Creates a new instance of the MAX44009, MAX44009 working mode is continuous. (Consume slightly higher power than in the default mode)
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="integrationTime">Measurement Cycle</param>
        public Max44009(I2cDevice sensor, IntegrationTime integrationTime)
        {
            _sensor = sensor;

            // Details in the Datasheet P8
            Span<byte> writeBuff = stackalloc byte[2] { (byte)Register.MAX_CONFIG, (byte)(0b_1100_0000 | (byte)integrationTime) };

            _sensor.Write(writeBuff);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }

        /// <summary>
        /// Get MAX44009 Illuminance (Lux)
        /// </summary>
        /// <returns>Illuminance (Lux)</returns>
        private double GetIlluminance()
        {
            Span<byte> readBuff = stackalloc byte[2];

            _sensor.WriteByte((byte)Register.MAX_LUX_HIGH);
            _sensor.Read(readBuff);

            // Details in the Datasheet P9-10
            byte exponent = (byte)((readBuff[0] & 0b_1111_0000) >> 4);
            byte mantissa = (byte)(((readBuff[0] & 0b_0000_1111) << 4) | (readBuff[1]) & 0b_0000_1111);

            double lux = Math.Pow(2, exponent) * mantissa * 0.045;

            return Math.Round(lux, 3);
        }
    }
}
