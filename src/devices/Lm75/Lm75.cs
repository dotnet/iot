// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Lm75
{
    /// <summary>
    /// Digital Temperature Sensor LM75
    /// </summary>
    public class Lm75 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// LM75 I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x48;

        #region prop

        /// <summary>
        /// LM75 Temperature
        /// </summary>
        public Temperature Temperature { get => Temperature.FromDegreesCelsius(GetTemperature()); }

        private bool _disable;

        /// <summary>
        /// Disable LM75
        /// </summary>
        public bool Disabled
        {
            get => _disable;
            set
            {
                SetShutdown(value);
                _disable = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the LM75
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Lm75(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException($"{nameof(i2cDevice)} cannot be null");
            Disabled = false;
        }

        /// <summary>
        /// Read LM75 Temperature (℃)
        /// </summary>
        /// <returns>Temperature</returns>
        private double GetTemperature()
        {
            Span<byte> readBuff = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)Register.LM_TEMP);
            _i2cDevice.Read(readBuff);

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
            _i2cDevice.WriteByte((byte)Register.LM_CONFIG);
            byte config = _i2cDevice.ReadByte();

            config &= 0xFE;
            if (isShutdown)
            {
                config |= 0x01;
            }

            Span<byte> writeBuff = stackalloc byte[]
            {
                (byte)Register.LM_CONFIG, config
            };
            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}
