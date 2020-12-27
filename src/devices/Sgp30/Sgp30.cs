// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Sgp30
{
    /// <summary>
    /// Device binding for Sensorion SGP30 TVOC + eCO2 Gas Sensor.
    /// </summary>
    public class Sgp30 : IDisposable
    {
        // SGP30 Air Quality Sensor I2C Commands
        // NOTE: The 'Measure Test' command is used for production verification only and is not included here
        private const ushort SGP30_INITIALISE_AIR_QUALITY = 0x2003;
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
        /// <exception cref="ChecksumFailedException">Thrown if checksum validation fails.</exception>
        public ushort[] GetSerialId()
        {
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

            return serialValues;
        }

        /// <summary>
        /// Get the device featureset version.
        /// </summary>
        /// <returns>Device Features.</returns>
        public ushort GetFeaturesetVersion()
        {
            Span<byte> resultBuffer = stackalloc byte[4];
            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { (byte)((SGP30_GET_FEATURESET_VERSION & 0xFF00) >> 8), (byte)(SGP30_GET_FEATURESET_VERSION & 0x00FF) }));
            _i2cDevice.Read(resultBuffer.Slice(1));
            byte[] resultArray = resultBuffer.ToArray();
            ushort result = (ushort)(resultArray[1] << 8 | resultArray[2]);
            byte checksum = CalculateChecksum(result);

            if (checksum != resultArray[3])
            {
                throw new ChecksumFailedException();
            }

            return result;
        }

        /// <summary>
        /// Initialises the SGP30 for measurement. NOTE: This call will block for up to 20 seconds.
        /// </summary>
        /// <returns>Initial SGP30 measurement result.</returns>
        public Sgp30Measurement InitialiseMeasurement()
        {
            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { (byte)((SGP30_INITIALISE_AIR_QUALITY & 0xFF00) >> 8), (byte)(SGP30_INITIALISE_AIR_QUALITY & 0x00FF) }));
            Thread.Sleep(10);

            Sgp30Measurement result = new Sgp30Measurement
            {
                Tvoc = 0x0000,
                Eco2 = 0x0000
            };

            for (int i = 0; i < 20; i++)
            {
                result = GetMeasurement();

                if (result.Tvoc != 0 || result.Eco2 != 400)
                {
                    // If either eCO2 != 400ppm or TVOC != 0ppb, the device has initialised, don't wait for the
                    // remaining initialisation measurements to complete.
                    break;
                }

                Thread.Sleep(1000);
            }

            return result;
        }

        /// <summary>
        /// Get the current eCO2 and TVOC measurements from the SGP30. NOTE: This should be called at least once per
        /// second as per the device datasheet in order to maintain accuracy of the readings. If several seconds have
        /// passed, call <see cref="InitialiseMeasurement" />.
        /// </summary>
        /// <returns>SGP30 measurement result.</returns>
        public Sgp30Measurement GetMeasurement()
        {
            Span<byte> resultBuffer = stackalloc byte[7];
            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { (byte)((SGP30_INITIALISE_AIR_QUALITY & 0xFF00) >> 8), (byte)(SGP30_INITIALISE_AIR_QUALITY & 0x00FF) }));
            Thread.Sleep(12);
            _i2cDevice.Read(resultBuffer.Slice(1));
            byte[] resultArray = resultBuffer.ToArray();

            Sgp30Measurement result = new Sgp30Measurement
            {
                Tvoc = (ushort)(resultArray[1] << 8 | resultArray[2]),
                Eco2 = (ushort)(resultArray[4] << 8 | resultArray[5])
            };

            byte[] checksums = new byte[]
            {
                CalculateChecksum(result.Tvoc),
                CalculateChecksum(result.Eco2)
            };

            if (checksums[0] != resultArray[3] || checksums[1] != resultArray[6])
            {
                throw new ChecksumFailedException();
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
