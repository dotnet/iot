// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Add documentation here
    /// </summary>
    public class Amg88xx : IDisposable
    {
        /// <summary>
        /// Standard device address (AD_SELECT pin is low, c.f. reference specification, pg. 11)
        /// </summary>
        public const int DeviceAddress = 0x68;

        /// <summary>
        /// Alternative device address (AD_SELECT pin is high, c.f. reference specification, pg. 11)
        /// </summary>
        public const int AlternativeDeviceAddress = 0x69;

        /// <summary>
        /// Number of columns of the sensor array
        /// </summary>
        public const int Columns = 0x8;

        /// <summary>
        /// Number of rows of the sensor array
        /// </summary>
        public const int Rows = 0x8;

        // Definition of data link packet constants, c.f. "grid-eye_communication-protocol-v1.0.pdf"
        private const string PacketHead = "***";
        private const string PacketTail = "\r\n";
        private const int BytesPerPixel = 2;
        private const int BytesForThermistor = 2;
        private const int SensorDataPacketSize = Columns * Rows * BytesPerPixel + BytesForThermistor;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Amg88xx"/> binding.
        /// </summary>
        public Amg88xx(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Gets the temperature reading from the sensor internal thermistor.
        /// </summary>
        /// <returns>Chip temperature reading</returns>
        public Temperature GetSensorTemperature()
        {
            _i2cDevice.WriteByte((byte)Register.TTHL);
            byte tthl = _i2cDevice.ReadByte();
            _i2cDevice.WriteByte((byte)Register.TTHH);
            byte tthh = _i2cDevice.ReadByte();

            return Amg88xxUtils.ConvertThermistorReading(tthl, tthh);
        }

        /// <summary>
        /// Gets the current thermal image from the sensor.
        /// </summary>
        /// <returns>...</returns>
        public Temperature[,] GetImage()
        {
            var rawImage = GetRawImage();
            var temperatureImage = new Temperature[Columns, Rows];

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    temperatureImage[c, r] = Amg88xxUtils.ConvertThermophileReading((byte)(rawImage[c, r] & 0xff), (byte)(rawImage[c, r] >> 8));
                }
            }

            return temperatureImage;
        }

        /// <summary>
        /// Gets the current thermal image from the sensor.
        /// </summary>
        /// <returns>...</returns>
        public int[,] GetRawImage()
        {
            var image = new int[Columns, Rows];

            _i2cDevice.WriteByte((byte)Register.T01L);
            Span<byte> buffer = stackalloc byte[Rows * Columns * BytesPerPixel];
            _i2cDevice.Read(buffer);

            int idx = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    byte tl = buffer[idx++];
                    byte th = buffer[idx++];
                    image[c, r] = th << 8 | tl;
                }
            }

            return image;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
