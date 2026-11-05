// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Holds an FT-X configuration snapshot and its null-terminated native USB descriptor buffers.
    /// </summary>
    internal sealed class FtXSeriesConfiguration
    {
        /// <summary>
        /// Gets or sets the configuration fields represented by the native FT-X EEPROM structure.
        /// </summary>
        public FtXSeriesEeprom Data { get; set; } = new FtXSeriesEeprom
        {
            Common = new FtEepromHeader { DeviceType = FtDeviceType.FtXSeries }
        };

        /// <summary>
        /// Gets the null-terminated manufacturer string buffer.
        /// </summary>
        /// <remarks>The capacity exceeds the documented descriptor limits because D2XX accepts no buffer length.</remarks>
        public byte[] Manufacturer { get; } = new byte[256];

        /// <summary>
        /// Gets the null-terminated manufacturer ID string buffer.
        /// </summary>
        public byte[] ManufacturerId { get; } = new byte[256];

        /// <summary>
        /// Gets the null-terminated device description string buffer.
        /// </summary>
        public byte[] Description { get; } = new byte[256];

        /// <summary>
        /// Gets the null-terminated serial number string buffer.
        /// </summary>
        public byte[] SerialNumber { get; } = new byte[256];

        /// <summary>
        /// Verifies the device type and termination of all descriptor strings.
        /// </summary>
        /// <exception cref="IOException">The device type is unexpected or a descriptor lacks a null terminator.</exception>
        public void Validate()
        {
            if (Data.Common.DeviceType != FtDeviceType.FtXSeries)
            {
                throw new IOException("The FT-X configuration contains an unexpected EEPROM device type.");
            }

            GetStringBytes(Manufacturer);
            GetStringBytes(ManufacturerId);
            GetStringBytes(Description);
            GetStringBytes(SerialNumber);
        }

        /// <summary>
        /// Creates an independent copy of the configuration and all descriptor buffers.
        /// </summary>
        /// <returns>A snapshot containing copies of the configuration fields and descriptor buffers.</returns>
        public FtXSeriesConfiguration Clone()
        {
            var configuration = new FtXSeriesConfiguration { Data = Data };
            Manufacturer.CopyTo(configuration.Manufacturer, 0);
            ManufacturerId.CopyTo(configuration.ManufacturerId, 0);
            Description.CopyTo(configuration.Description, 0);
            SerialNumber.CopyTo(configuration.SerialNumber, 0);
            return configuration;
        }

        /// <summary>
        /// Compares the exposed configuration fields and descriptor strings, ignoring bytes after their null terminators.
        /// </summary>
        /// <param name="other">The configuration snapshot to compare.</param>
        /// <returns>Whether all configuration fields and descriptor string contents match.</returns>
        /// <exception cref="IOException">A compared descriptor lacks a null terminator.</exception>
        public bool HasSameContents(FtXSeriesConfiguration other)
        {
            return Data.Equals(other.Data) &&
                GetStringBytes(Manufacturer).SequenceEqual(GetStringBytes(other.Manufacturer)) &&
                GetStringBytes(ManufacturerId).SequenceEqual(GetStringBytes(other.ManufacturerId)) &&
                GetStringBytes(Description).SequenceEqual(GetStringBytes(other.Description)) &&
                GetStringBytes(SerialNumber).SequenceEqual(GetStringBytes(other.SerialNumber));
        }

        private static ReadOnlySpan<byte> GetStringBytes(byte[] buffer)
        {
            int length = buffer.AsSpan().IndexOf((byte)0);
            if (length < 0)
            {
                throw new IOException("The FT-X configuration contains an unterminated EEPROM descriptor string.");
            }

            return buffer.AsSpan(0, length);
        }
    }
}
