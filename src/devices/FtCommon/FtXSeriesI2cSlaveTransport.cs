// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Implements FT-X I2C slave FIFO and MTP configuration access using the native D2XX library.
    /// </summary>
    /// <remarks>
    /// The caller must select an FT-X I2C slave device, such as an FT200XD or FT201X.
    /// Other FT-X devices share the D2XX device type but do not provide the same I2C functionality.
    /// The caller serializes operations and owns address validation and programming verification.
    /// </remarks>
    internal sealed class FtXSeriesI2cSlaveTransport : IFtXSeriesI2cSlaveTransport
    {
        private readonly SafeFtHandle _handle;

        /// <summary>
        /// Opens an FT-X I2C slave by serial number and configures its USB transfer timeouts.
        /// </summary>
        /// <param name="serialNumber">The unique serial number used to select the device.</param>
        /// <param name="readTimeoutMilliseconds">The D2XX read timeout in milliseconds.</param>
        /// <param name="writeTimeoutMilliseconds">The D2XX write timeout in milliseconds.</param>
        internal FtXSeriesI2cSlaveTransport(string serialNumber, int readTimeoutMilliseconds, int writeTimeoutMilliseconds)
        {
            FtStatus status = FtFunction.FT_OpenEx(serialNumber, FtOpenType.OpenBySerialNumber, out SafeFtHandle handle);
            if (status != FtStatus.Ok || handle is null || handle.IsInvalid)
            {
                handle?.Dispose();
                throw new IOException($"FT-X I2C slave failed to open, D2XX status: {status}.");
            }

            _handle = handle;
            try
            {
                CheckStatus(FtFunction.FT_SetTimeouts(_handle, (uint)readTimeoutMilliseconds, (uint)writeTimeoutMilliseconds), "set timeouts");
            }
            catch
            {
                _handle.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public uint BytesAvailable
        {
            get
            {
                uint count = 0;
                CheckStatus(FtFunction.FT_GetQueueStatus(_handle, ref count), "query receive queue");
                return count;
            }
        }

        /// <inheritdoc/>
        public int Read(Span<byte> buffer)
        {
            uint count = 0;
            CheckStatus(FtFunction.FT_Read(_handle, in MemoryMarshal.GetReference(buffer), (uint)buffer.Length, ref count), "read");
            if (count > buffer.Length)
            {
                throw new IOException($"FT-X I2C slave returned an invalid read count ({count}).");
            }

            return (int)count;
        }

        /// <inheritdoc/>
        public int Write(ReadOnlySpan<byte> buffer)
        {
            uint count = 0;
            CheckStatus(FtFunction.FT_Write(_handle, in MemoryMarshal.GetReference(buffer), (uint)buffer.Length, ref count), "write");
            if (count > buffer.Length)
            {
                throw new IOException($"FT-X I2C slave returned an invalid write count ({count}).");
            }

            return (int)count;
        }

        /// <summary>
        /// Releases the owned native D2XX handle.
        /// </summary>
        public void Dispose()
        {
            _handle.Dispose();
        }

        /// <inheritdoc/>
        public FtXSeriesConfiguration ReadConfiguration()
        {
            var configuration = new FtXSeriesConfiguration();
            FtXSeriesEeprom data = configuration.Data;
            CheckStatus(
                FtFunction.FT_EEPROM_Read(
                    _handle, ref data, (uint)Marshal.SizeOf<FtXSeriesEeprom>(),
                    configuration.Manufacturer, configuration.ManufacturerId, configuration.Description, configuration.SerialNumber),
                "read EEPROM configuration");
            configuration.Data = data;
            configuration.Validate();
            return configuration;
        }

        /// <inheritdoc/>
        public void ProgramConfiguration(FtXSeriesConfiguration configuration)
        {
            configuration.Validate();
            FtXSeriesEeprom data = configuration.Data;
            CheckStatus(
                FtFunction.FT_EEPROM_Program(
                    _handle, ref data, (uint)Marshal.SizeOf<FtXSeriesEeprom>(),
                    configuration.Manufacturer, configuration.ManufacturerId, configuration.Description, configuration.SerialNumber),
                "program EEPROM configuration");
        }

        private static void CheckStatus(FtStatus status, string operation)
        {
            if (status != FtStatus.Ok)
            {
                throw new IOException($"FT-X I2C slave failed to {operation}, D2XX status: {status}.");
            }
        }
    }
}
