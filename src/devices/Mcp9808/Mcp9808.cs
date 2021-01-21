// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Mcp9808
{
    /// <summary>
    /// Microchip's MCP9808 I2C Temp sensor
    /// </summary>
    [Interface("Microchip's MCP9808 I2C Temp sensor")]
    public class Mcp9808 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// MCP9808 I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x18;

        #region prop

        /// <summary>
        /// MCP9808 Temperature
        /// </summary>
        [Telemetry]
        public Temperature Temperature { get => Temperature.FromDegreesCelsius(GetTemperature()); }

        private bool _disable;

        /// <summary>
        /// Disable MCP9808
        /// </summary>
        [Property]
        public bool Disabled
        {
            get => _disable;
            set
            {
                SetShutdown(value);
            }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the MCP9808
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Mcp9808(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            Disabled = false;

            if (!Init())
            {
                throw new Exception("Unable to identify manufacturer/device id");
            }
        }

        /// <summary>
        /// Checks if the device is a MCP9808
        /// </summary>
        /// <returns>True if device has been correctly detected</returns>
        private bool Init()
        {
            if (Read16(Register8.MCP_MANUF_ID) != 0x0054)
            {
                return false;
            }

            if (Read16(Register8.MCP_DEVICE_ID) != 0x0400)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return the internal resolution register
        /// </summary>
        /// <returns>Resolution setting</returns>
        [Property("Resolution")]
        public byte GetResolution() => Read8(Register8.MCP_RESOLUTION);

        /// <summary>
        /// Wakes-up the device
        /// </summary>
        [Command]
        public void Wake()
        {
            SetShutdown(false);

            // Sleep 250 ms, which is the typical temperature conversion time for the highest resolution (page 3 in datasheet, tCONV)
            Thread.Sleep(250);
        }

        /// <summary>
        /// Shuts down the device
        /// </summary>
        [Command]
        public void Shutdown() => SetShutdown(true);

        /// <summary>
        /// Read MCP9808 Temperature (℃)
        /// </summary>
        /// <returns>Temperature</returns>
        private double GetTemperature()
        {
            ushort value = Read16(Register8.MCP_AMBIENT_TEMP);

            if (value == 0xFFFF)
            {
                // Return NaN if the MCP9808 doesn't have a measurement ready at the time of reading
                return double.NaN;
            }

            double temp = value & 0x0FFF;
            temp /= 16.0;
            if ((value & 0x1000) != 0)
            {
                temp -= 256;
            }

            return Math.Round(temp, 5);
        }

        /// <summary>
        /// Set MCP9808 Shutdown
        /// </summary>
        /// <param name="isShutdown">Shutdown when value is true.</param>
        private void SetShutdown(bool isShutdown)
        {
            Register16 curVal = ReadRegister16(Register8.MCP_CONFIG);

            if (isShutdown)
            {
                curVal |= Register16.MCP_CONFIG_SHUTDOWN;
            }
            else
            {
                curVal &= ~Register16.MCP_CONFIG_SHUTDOWN;
            }

            Write16(Register8.MCP_CONFIG, curVal);

            _disable = isShutdown;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        internal Register16 ReadRegister16(Register8 reg) => (Register16)Read16(reg);

        internal ushort Read16(Register8 reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            Span<byte> buf = stackalloc byte[2];
            _i2cDevice.Read(buf);

            return (ushort)(buf[0] << 8 | buf[1]);
        }

        internal void Write16(Register8 reg, Register16 value)
        {
            _i2cDevice.Write(new byte[]
            {
                (byte)reg,
                (byte)((ushort)value >> 8),
                (byte)((ushort)value & 0xFF)
            });
        }

        internal byte Read8(Register8 reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        internal void Write8(Register8 reg, byte value)
        {
            _i2cDevice.Write(new byte[]
            {
                (byte)reg,
                value
            });
        }
    }
}
