// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft200X
{
    /// <summary>
    /// Provides USB-side access to the FT200X I2C slave's receive and transmit FIFOs.
    /// </summary>
    /// <remarks>
    /// This is not an I2C master or an I2cDevice. An external I2C master must
    /// initiate all bus transfers. Reads and writes contain only payload bytes,
    /// not I2C addresses or register commands. The device owns its D2XX connection.
    /// </remarks>
    public sealed class Ft200XDevice : IDisposable
    {
        private readonly object _sync = new object();
        private readonly IFtXSeriesI2cSlaveTransport _transport;
        private bool _disposed;

        /// <summary>
        /// Opens the FT200X by serial number, without resetting it or purging its FIFOs.
        /// </summary>
        /// <param name="deviceInformation">The FT200X descriptor returned by <see cref="GetDevices"/>.</param>
        /// <param name="readTimeoutMilliseconds">Positive D2XX read timeout, in milliseconds.</param>
        /// <param name="writeTimeoutMilliseconds">Positive D2XX write timeout, in milliseconds.</param>
        /// <remarks>
        /// For devices with a customized USB description, a descriptor from
        /// FtCommon.GetDevices can be supplied. Verify that it is physically an
        /// FT200X: D2XX reports the same FtXSeries type for several different chips.
        /// A nonempty, unique serial number is required on all platforms.
        /// </remarks>
        public Ft200XDevice(FtDevice deviceInformation, int readTimeoutMilliseconds = 1000, int writeTimeoutMilliseconds = 1000)
            : this(Open(deviceInformation, readTimeoutMilliseconds, writeTimeoutMilliseconds))
        {
        }

        internal Ft200XDevice(IFtXSeriesI2cSlaveTransport transport)
        {
            ArgumentNullException.ThrowIfNull(transport);
            _transport = transport;
        }

        /// <summary>
        /// Gets the number of bytes currently available in the USB driver's receive queue.
        /// These bytes were written to the FT200X by the external I2C master.
        /// </summary>
        /// <remarks>This does not report free space or pending data in the USB-to-I2C FIFO.</remarks>
        public uint BytesAvailable
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return _transport.BytesAvailable;
                }
            }
        }

        /// <summary>
        /// Gets the I2C slave address stored in MTP memory, without programming it.
        /// </summary>
        /// <remarks>
        /// This is the programmed configuration, not a measurement of the active
        /// address. A USB disconnect/reconnect is required after programming.
        /// </remarks>
        public int ProgrammedI2cAddress
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    FtXSeriesConfiguration configuration = _transport.ReadConfiguration();
                    configuration.Validate();
                    return configuration.Data.I2cSlaveAddress;
                }
            }
        }

        /// <summary>
        /// Enumerates FT-X devices whose USB description starts with FT200X.
        /// </summary>
        /// <returns>Descriptors of likely FT200X adapters.</returns>
        /// <remarks>
        /// The default VID/PID 0403:6015 and D2XX device type are shared with other
        /// FT-X chips. The description filter avoids selecting all FT-X devices.
        /// Devices with customized descriptions must be selected using FtCommon.GetDevices.
        /// </remarks>
        public static List<FtDevice> GetDevices()
        {
            return FtCommon.FtCommon.GetDevices().Where(IsFt200X).ToList();
        }

        /// <summary>
        /// Reads bytes that the external I2C master has written to the FT200X.
        /// </summary>
        /// <param name="buffer">Destination buffer; only the returned number of bytes is valid.</param>
        /// <returns>
        /// The number of bytes read. D2XX waits for the requested length or the
        /// read timeout, so a timeout can return fewer bytes, including zero.
        /// An empty buffer returns zero without accessing USB.
        /// </returns>
        public int Read(Span<byte> buffer)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                if (buffer.IsEmpty)
                {
                    return 0;
                }

                int count = _transport.Read(buffer);
                if (count < 0 || count > buffer.Length)
                {
                    throw new IOException($"FT200X returned an invalid read count ({count}).");
                }

                return count;
            }
        }

        /// <summary>
        /// Queues bytes on USB for the external I2C master to read from the FT200X.
        /// </summary>
        /// <param name="buffer">Payload bytes, without an I2C address. An empty buffer does nothing.</param>
        /// <exception cref="IOException">
        /// D2XX failed or accepted fewer bytes than requested. A partial write is
        /// not retried automatically, since some bytes may already have been queued.
        /// </exception>
        /// <remarks>
        /// Success means the USB driver accepted the bytes, not that the external
        /// master consumed them. Large writes can time out if the master is not
        /// draining the FIFO. This method does not initiate I2C activity.
        /// </remarks>
        public void Write(ReadOnlySpan<byte> buffer)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                if (buffer.IsEmpty)
                {
                    return;
                }

                int count = _transport.Write(buffer);
                if (count != buffer.Length)
                {
                    throw new IOException($"FT200X queued {count} of {buffer.Length} bytes. The write was not retried.");
                }
            }
        }

        /// <summary>
        /// Persistently programs the I2C slave address while preserving the other
        /// FT-X configuration fields and USB descriptor strings.
        /// </summary>
        /// <param name="address">An unshifted 7-bit address from 0x08 to 0x77; reserved addresses are rejected.</param>
        /// <returns>
        /// <see langword="true"/> if a write was performed and verified, or
        /// <see langword="false"/> if the stored address already matched.
        /// </returns>
        /// <remarks>
        /// Stop the external I2C master and back up the configuration before use.
        /// This consumes MTP endurance (specified as 2,000 cycles), so do not call
        /// it routinely on startup. The entire exposed configuration is read back
        /// and compared after programming. Once a write is attempted, this instance
        /// is disposed even on failure; no automatic retry or rollback is attempted.
        /// Disconnect/reconnect USB and open a new instance to activate the change.
        /// A no-op leaves the instance open. A read failure before programming also
        /// leaves it open. Requires a D2XX runtime exposing the FT-X EEPROM APIs.
        /// </remarks>
        /// <exception cref="IOException">
        /// Reading, programming, or verification failed. If programming was
        /// attempted, inspect the stored configuration before trying again.
        /// </exception>
        public bool ProgramI2cAddress(int address)
        {
            if (address < 0x08 || address > 0x77)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Use a nonreserved 7-bit I2C address from 0x08 to 0x77.");
            }

            lock (_sync)
            {
                ThrowIfDisposed();
                FtXSeriesConfiguration original = _transport.ReadConfiguration();
                original.Validate();
                if (original.Data.I2cSlaveAddress == address)
                {
                    return false;
                }

                FtXSeriesConfiguration updated = original.Clone();
                FtXSeriesEeprom data = updated.Data;
                data.I2cSlaveAddress = (ushort)address;
                updated.Data = data;
                try
                {
                    _transport.ProgramConfiguration(updated);
                    FtXSeriesConfiguration verified = _transport.ReadConfiguration();
                    verified.Validate();
                    if (!updated.HasSameContents(verified))
                    {
                        throw new IOException("FT200X EEPROM verification failed. Disconnect/reconnect USB and inspect the configuration before retrying.");
                    }

                    return true;
                }
                finally
                {
                    // A reconnect is needed to reload MTP on all supported operating systems.
                    Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_sync)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _transport.Dispose();
                }
            }
        }

        internal static bool IsFt200X(FtDevice device)
        {
            return device.Type == FtDeviceType.FtXSeries && device.Description.StartsWith("FT200X", StringComparison.OrdinalIgnoreCase);
        }

        private static IFtXSeriesI2cSlaveTransport Open(FtDevice deviceInformation, int readTimeoutMilliseconds, int writeTimeoutMilliseconds)
        {
            ArgumentNullException.ThrowIfNull(deviceInformation);
            if (deviceInformation.Type != FtDeviceType.FtXSeries)
            {
                throw new ArgumentException("An FT200X must be reported as an FT-X series device.", nameof(deviceInformation));
            }

            if (string.IsNullOrWhiteSpace(deviceInformation.SerialNumber) || deviceInformation.SerialNumber.Contains('\0'))
            {
                throw new ArgumentException("A nonempty FT200X serial number without NUL characters is required.", nameof(deviceInformation));
            }

            if (readTimeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(readTimeoutMilliseconds));
            }

            if (writeTimeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(writeTimeoutMilliseconds));
            }

            return new FtXSeriesI2cSlaveTransport(deviceInformation.SerialNumber, readTimeoutMilliseconds, writeTimeoutMilliseconds);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}
