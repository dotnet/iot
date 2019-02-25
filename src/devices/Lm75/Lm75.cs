// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Lm75
{
    /// <summary>
    /// Digital Temperature Sensor LM75
    /// </summary>
    public class Lm75 : IDisposable
    {
        private I2cDevice _sensor;

        /// <summary>
        /// LM75 I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x48;

        #region prop

        /// <summary>
        /// LM75 Temperature (℃)
        /// </summary>
        public double Temperature { get => GetTemperature(); }

        private bool _disable;
        /// <summary>
        /// Disable LM75
        /// </summary>
        public bool Disabled { get { return _disable; } set { SetShutdown(value); _disable = value; } }

        #endregion

        /// <summary>
        /// Creates a new instance of the LM75
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public Lm75(I2cDevice sensor)
        {
            _sensor = sensor;

            Disabled = false;
        }

        /// <summary>
        /// Read LM75 Temperature (℃)
        /// </summary>
        /// <returns>Temperature</returns>
        private double GetTemperature()
        {
            Span<byte> readBuff = stackalloc byte[2];

            _sensor.WriteByte((byte)Register.LM_TEMP);
            _sensor.Read(readBuff);

            // Details in Datasheet P10
            double temp = 0;
            ushort raw = (ushort)((readBuff[0] << 3) | (readBuff[1] >> 5));
            if ((readBuff[0] & 0x80) == 0)
            {
                // temperature >= 0
                temp = raw * 0.125;
            }
            else
            {
                // temperature < 0
                // two's complement
                raw |= 0xF800;
                raw = (ushort)(~raw + 1);

                temp = raw * (-1) * 0.125;
            }

            return Math.Round(temp, 1);
        }

        /// <summary>
        /// Set LM75 Shutdown
        /// </summary>
        /// <param name="isShutdown">Shutdown when value is true.</param>
        private void SetShutdown(bool isShutdown)
        {
            _sensor.WriteByte((byte)Register.LM_CONFIG);
            byte config = _sensor.ReadByte();

            config &= 0xFE;
            if (isShutdown)
                config |= 0x01;
            else
                config |= 0x00;
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
