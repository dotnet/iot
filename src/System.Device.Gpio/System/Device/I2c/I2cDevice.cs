// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    /// <summary>
    /// The communications channel to a device on an I2C bus.
    /// </summary>
    public abstract class I2cDevice : IDisposable
    {
        /// <summary>
        /// The connection settings of a device on an I2C bus.
        /// </summary>
        public abstract I2cConnectionSettings ConnectionSettings { get; }

        /// <summary>
        /// Reads a byte from the I2C device.        
        /// </summary>
        /// <returns>A byte read from the I2C device.</returns>
        public abstract byte ReadByte();

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public abstract void Read(Span<byte> buffer);

        /// <summary>
        /// Writes a byte to the I2C device.
        /// </summary>
        /// <param name="data">The byte to be written to the I2C device.</param>
        public abstract void WriteByte(byte data);

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public abstract void Write(Span<byte> buffer);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }
    }
}
