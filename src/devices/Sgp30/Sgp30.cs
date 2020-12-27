// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
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
        private const ushort SGP30_INITIALISE_AIR_QUALITY = 0x2003; // DONE
        private const ushort SGP30_MEASURE_AIR_QUALITY = 0x2008; // DONE
        private const ushort SGP30_GET_BASELINE = 0x2015; // DONE
        private const ushort SGP30_SET_BASELINE = 0x201E; // DONE
        private const ushort SGP30_SET_HUMIDITY = 0x2061;
        private const ushort SGP30_GET_FEATURESET_VERSION = 0x202F; // DONE
        private const ushort SGP30_MEASURE_RAW_SIGNALS = 0x2050; // DONE
        private const ushort SGP30_GET_SERIAL_ID = 0x3682; // DONE

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
            return GetData(SGP30_MEASURE_AIR_QUALITY);
        }

        /// <summary>
        /// Get the raw signal data without on-device processing to TVOC ppb / eCO2 ppm values.
        /// </summary>
        /// <returns>Raw sensor data.</returns>
        public Sgp30Measurement GetRawSignalData()
        {
            return GetData(SGP30_MEASURE_RAW_SIGNALS);
        }

        /// <summary>
        /// Get device baseline measurement data.
        /// </summary>
        /// <returns>Measured baseline data.</returns>
        public Sgp30Measurement GetBaseline()
        {
            return GetData(SGP30_GET_BASELINE);
        }

        /// <summary>
        /// Generalised data read for getting Measurement, Raw Measurement and Baseline data.
        /// </summary>
        /// <param name="command">Command to send to the SGP30.</param>
        /// <returns>Resulting data returned from SGP30.</returns>
        private Sgp30Measurement GetData(ushort command)
        {
            Span<byte> resultBuffer = stackalloc byte[7];
            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { (byte)((command & 0xFF00) >> 8), (byte)(command & 0x00FF) }));
            Thread.Sleep(12);
            _i2cDevice.Read(resultBuffer.Slice(1));
            byte[] resultArray = resultBuffer.ToArray();

            Sgp30Measurement result = new Sgp30Measurement
            {
                Eco2 = (ushort)(resultArray[1] << 8 | resultArray[2]),
                Tvoc = (ushort)(resultArray[4] << 8 | resultArray[5])
            };

            byte[] checksums = new byte[]
            {
                CalculateChecksum(result.Eco2),
                CalculateChecksum(result.Tvoc)
            };

            if (checksums[0] != resultArray[3] || checksums[1] != resultArray[6])
            {
                throw new ChecksumFailedException();
            }

            return result;
        }

        /// <summary>
        /// Set SGP30 baseline values.
        /// </summary>
        /// <param name="tvoc">TVOC Baseline.</param>
        /// <param name="eco2">eCO2 Baseline.</param>
        public void SetBaseline(ushort tvoc, ushort eco2)
        {
            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[]
                {
                    (byte)((SGP30_SET_BASELINE & 0xFF00) >> 8),
                    (byte)(SGP30_SET_BASELINE & 0x00FF),
                    (byte)((tvoc & 0xFF00) >> 8),
                    (byte)(tvoc & 0x00FF),
                    CalculateChecksum(tvoc),
                    (byte)((eco2 & 0xFF00) >> 8),
                    (byte)(eco2 & 0x00FF),
                    CalculateChecksum(eco2)
                })
            );
        }

        /// <summary>
        /// Set Humidity value on the SGP30 for more accurate readings. Requires both temperature and humidity measurements
        /// from another active sensor to enable conversion from %RH to absolute humidity in g/m³.
        /// </summary>
        /// <param name="temperature">Temperature measurement.</param>
        /// <param name="humidity">Relative Humitity measurement.</param>
        public void SetHumidity(Temperature temperature, RelativeHumidity humidity)
        {
            double absoluteHumidity = WeatherHelper.CalculateAbsoluteHumidity(temperature, humidity).GramsPerCubicMeter;

            // First byte contains the whole number part, from 0 to 256 g/m³
            byte firstByte = (byte)((int)(Math.Floor(absoluteHumidity)) & 0xFF);

            // Second byte contains the decimal part, expressed as the nearest whole number of 256ths of a g/m³
            byte secondByte = (byte)((int)Math.Floor(((absoluteHumidity - Math.Floor(absoluteHumidity)) / (1 / 256))) & 0xFF);

            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[]
                {
                    (byte)((SGP30_SET_HUMIDITY & 0xFF00) >> 8),
                    (byte)(SGP30_SET_HUMIDITY & 0x00FF),
                    firstByte,
                    secondByte,
                    CalculateChecksum((ushort)(firstByte << 8 | secondByte))
                })
            );
        }

        /// <summary>
        /// Calculates the SGP30 CRC / Checksum for a given input word.
        /// </summary>
        /// <param name="data">Data word to checksum.</param>
        /// <returns>Resulting CRC checksum byte.</returns>
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
