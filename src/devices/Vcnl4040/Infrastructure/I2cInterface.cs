// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Vncl4040.Infrastructure
{
    /// <summary>
    /// Represents an convinient interface to the I2C bus in the context of this binding.
    /// It provides aids to simplify read and write operations.
    /// </summary>
    public class I2cInterface
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
        /// <param name="registerAddress">Register address</param>
        /// <param name="buffer">Bytes to be read from the register</param>
        public void Read(byte registerAddress, Span<byte> buffer)
        {
            Write(registerAddress, Span<byte>.Empty);
            _i2cDevice.Read(buffer);
        }

        /// <summary>
        /// Reads a byte from a device register
        /// </summary>
        /// <param name="registerAddress">Register address</param>
        public byte Read(byte registerAddress)
        {
            Write(registerAddress, Span<byte>.Empty);
            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Reads a byte from a device register
        /// </summary>
        /// <param name="registerAddress">Register address</param>
        public byte Read(int registerAddress)
        {
            Write((byte)registerAddress, Span<byte>.Empty);
            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Writes bytes to the device register
        /// </summary>
        /// <param name="registerAddress">Register address</param>
        /// <param name="data">Bytes to be written to the register</param>
        public void Write(byte registerAddress, Span<byte> data)
        {
            Span<byte> output = stackalloc byte[data.Length + 1];
            output[0] = registerAddress;
            data.CopyTo(output.Slice(1));
            _i2cDevice.Write(output);
        }

        /// <summary>
        /// Writes a single byte to the device register
        /// </summary>
        /// <param name="registerAddress">Register address</param>
        /// <param name="data">Byte to be written to the register</param>
        public void Write(byte registerAddress, byte data)
        {
            Span<byte> output = stackalloc byte[2];
            output[0] = registerAddress;
            output[1] = data;
            _i2cDevice.Write(output);
        }
    }
}
