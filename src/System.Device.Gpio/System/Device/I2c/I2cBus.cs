// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.I2c
{
    /// <summary>
    /// I2C bus communication channel.
    /// </summary>
    public abstract partial class I2cBus : IDisposable
    {
        /// <summary>
        /// Creates default I2cBus
        /// </summary>
        /// <param name="busId">The bus ID.</param>
        /// <returns>I2cBus instance.</returns>
        public static I2cBus Create(int busId)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return CreateWindows10I2cBus(busId);
            }
            else
            {
                return UnixI2cBus.Create(busId);
            }
        }

        /// <summary>
        /// Creates I2C device.
        /// </summary>
        /// <param name="deviceAddress">Device address related with the device to create.</param>
        /// <returns>I2cDevice instance.</returns>
        public abstract I2cDevice CreateDevice(int deviceAddress);

        /// <summary>
        /// Removes I2C device.
        /// </summary>
        /// <param name="deviceAddress">Device address to create</param>
        public abstract void RemoveDevice(int deviceAddress);

        /// <summary>
        /// Reads data from the specified device.
        /// </summary>
        /// <param name="deviceAddress">Device address to read from.</param>
        /// <param name="buffer">Buffer to read data to.</param>
        public abstract void Read(int deviceAddress, Span<byte> buffer);

        /// <summary>
        /// Writes data to the specified device.
        /// </summary>
        /// <param name="deviceAddress">Device address to write to.</param>
        /// <param name="buffer">Buffer to write.</param>
        public abstract void Write(int deviceAddress, ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Reads and writes data to the specified device.
        /// </summary>
        /// <param name="deviceAddress">Device address to read and write.</param>
        /// <param name="writeBuffer">Buffer to write.</param>
        /// <param name="readBuffer">Buffer to read data to.</param>
        public virtual void WriteRead(int deviceAddress, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(deviceAddress, writeBuffer);
            Read(deviceAddress, readBuffer);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if explicitly disposing, <see langword="false"/> if in finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in the base class.
        }
    }
}
