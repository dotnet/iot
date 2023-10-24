// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vncl4040.Definitions;

namespace Iot.Device.Vncl4040.Infrastructure
{
    /// <summary>
    /// Represents an convinient interface to the I2C bus in the context of this binding.
    /// It provides aids to simplify read and write operations.
    /// </summary>
    internal class I2cInterface
        : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public I2cInterface(I2cDevice device)
        {
            _i2cDevice = device;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }

        /// <summary>
        /// Reads bytes from the device register
        /// </summary>
        /// <param name="address">Register address</param>
        /// <param name="buffer">Bytes to be read from the register</param>
        public void Read(Address address, Span<byte> buffer)
        {
            Write(address, Span<byte>.Empty);
            _i2cDevice.Read(buffer);
        }

        /// <summary>
        /// Writes bytes to the device register
        /// </summary>
        /// <param name="address">Register address</param>
        /// <param name="data">Bytes to be written to the register</param>
        public void Write(Address address, Span<byte> data)
        {
            Span<byte> output = stackalloc byte[data.Length + 1];
            output[0] = (byte)address;
            data.CopyTo(output.Slice(1));
            _i2cDevice.Write(output);
        }
    }
}
