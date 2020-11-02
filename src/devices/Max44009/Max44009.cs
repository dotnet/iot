// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Max44009
{
    /// <summary>
    /// Ambient Light Sensor MAX44009
    /// </summary>
    public class Max44009 : IDisposable
    {
        private I2cDevice _i2cDevice;

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
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Max44009(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;

            // Details in the Datasheet P8
            Span<byte> writeBuff = stackalloc byte[2]
            {
                (byte)Register.MAX_CONFIG, 0b_0000_0000
            };

            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Creates a new instance of the MAX44009, MAX44009 working mode is continuous. (Consume slightly higher power than in the default mode)
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="integrationTime">Measurement Cycle</param>
        public Max44009(I2cDevice i2cDevice, IntegrationTime integrationTime)
        {
            _i2cDevice = i2cDevice;

            // Details in the Datasheet P8
            Span<byte> writeBuff = stackalloc byte[2]
            {
                (byte)Register.MAX_CONFIG, (byte)(0b_1100_0000 | (byte)integrationTime)
            };

            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Get MAX44009 Illuminance (Lux)
        /// </summary>
        /// <returns>Illuminance (Lux)</returns>
        private double GetIlluminance()
        {
            Span<byte> readBuff = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)Register.MAX_LUX_HIGH);
            _i2cDevice.Read(readBuff);

            // Details in the Datasheet P9-10
            byte exponent = (byte)((readBuff[0] & 0b_1111_0000) >> 4);
            byte mantissa = (byte)(((readBuff[0] & 0b_0000_1111) << 4) | (readBuff[1]) & 0b_0000_1111);

            double lux = Math.Pow(2, exponent) * mantissa * 0.045;

            return Math.Round(lux, 3);
        }
    }
}
