// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using UnitsNet;

namespace Iot.Device.Sgp30
{
    /// <summary>
    /// Device binding for Sensorion SGP30 TVOC + eCO2 Gas Sensor.
    /// </summary>
    public class Sgp30 : IDisposable
    {
        // SGP30 Air Quality Sensor I2C Commands
        // NOTE: The 'Measure Test' command is used for production verification only and is not included here
        private const ushort SGP30_INIT_AIR_QUALITY = 0x2003;
        private const ushort SGP30_MEASURE_AIR_QUALITY = 0x2008;
        private const ushort SGP30_GET_BASELINE = 0x2015;
        private const ushort SGP30_SET_BASELINE = 0x201E;
        private const ushort SGP30_SET_HUMIDITY = 0x2061;
        private const ushort SGP30_GET_FEATURESET_VERSION = 0x202F;
        private const ushort SGP30_MEASURE_RAW_SIGNALS = 0x2050;
        private const ushort SGP30_GET_SERIAL_ID = 0x3682;

        /// <summary>
        /// Default I2C Address, up to four IS31FL3730's can be on the same I2C Bus.
        /// </summary>
        public const byte DefaultI2cAddress = 0x58;

        /// <summary>
        /// I2C Device instance to communicate with the IS31FL3730.
        /// </summary>
        protected I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sgp30"/> class.
        /// </summary>
        public Sgp30(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Gets the Serial ID of the SGP30 sensor.
        /// </summary>
        /// <returns>Device Serial ID.</returns>
        public ushort[]? GetSerialId()
        {
            return ExecuteCommand(SGP30_GET_SERIAL_ID, null);
        }

        private ushort[]? ExecuteCommand(ushort command, ushort[]? parameters)
        {
            ushort[]? result = null;

            switch (command)
            {
                case SGP30_GET_SERIAL_ID:
                    Span<byte> resultBuffer = stackalloc byte[10];
                    _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { (byte)((SGP30_GET_SERIAL_ID & 0xFF00) >> 8), (byte)(SGP30_GET_SERIAL_ID & 0x00FF) }));
                    _i2cDevice.Read(resultBuffer.Slice(1));
                    byte[] resultArray = resultBuffer.ToArray();

                    ushort[] serialValues = new ushort[]
                    {
                        (ushort)(resultArray[1] << 8 | resultArray[2]),
                        (ushort)(resultArray[4] << 8 | resultArray[5]),
                        (ushort)(resultArray[7] << 8 | resultArray[8])
                    };

                    byte[] checksums = new byte[]
                    {
                        CalculateChecksum(serialValues[0]),
                        CalculateChecksum(serialValues[1]),
                        CalculateChecksum(serialValues[2]),
                    };

                    if (checksums[0] != resultArray[3] || checksums[1] != resultArray[6] || checksums[2] != resultArray[9])
                    {
                        throw new ChecksumFailedException();
                    }

                    result = serialValues;
                    break;
            }

            return result;
        }

        private byte CalculateChecksum(ushort data)
        {
            byte crc = 0xFF;
            byte[] bytes = new byte[]
            {
                (byte)((data & 0xFF00) >> 8),
                (byte)(data & 0x00FF)
            };

            foreach (byte item in bytes)
            {
                crc ^= item;
                for (int i = 0; i <= 7; i++)
                {
                    if ((crc & 0x80) == 0x80)
                    {
                        crc = (byte)((crc << 1) ^ 0x31);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            return (byte)(crc & 0xFF);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
