// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Infrastructure
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
        /// <param name="commandCode">Register access command code</param>
        /// <param name="buffer">Bytes to be read from the register</param>
        public void Read(CommandCode commandCode, Span<byte> buffer)
        {
            Write(commandCode, Span<byte>.Empty);
            _i2cDevice.Read(buffer);
            Console.WriteLine($"I2cInterface.Read: {buffer[0]} {buffer[1]}");
        }

        /// <summary>
        /// Writes bytes to the device register
        /// </summary>
        /// <param name="commandCode">Register access command code</param>
        /// <param name="data">Bytes to be written to the register</param>
        public void Write(CommandCode commandCode, Span<byte> data)
        {
            Span<byte> output = stackalloc byte[data.Length + 1];
            output[0] = (byte)commandCode;
            data.CopyTo(output.Slice(1));
            _i2cDevice.Write(output);

            Console.Write("Writing:");
            foreach (byte b in output)
            {
                Console.Write($" {b:X}");
            }

            Console.WriteLine();
        }
    }
}
