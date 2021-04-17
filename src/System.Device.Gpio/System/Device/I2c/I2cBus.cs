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

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Queries whether the device is available for communication. This is accomplished by sending the device address on the bus
        /// with the write bit set, but without sending any subsequent data bytes. If the device is available, it will respond with an
        /// ACK and this function will return true. If the device is not available, the device will not acknowlege and this funciton will
        /// return false.
        /// </summary>
        /// <returns>Whether the device responded with an ACK to the address query.</returns>
        public virtual bool IsDeviceReady(int deviceAddress)
        {
            // Should be overridden in derived classes, however not sure this is possible to support using Windows10I2cBus
            throw new NotSupportedException();
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
