// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public abstract partial class Mcp23xxx
    {
        /// <summary>
        /// I2C adapter
        /// </summary>
        protected class I2cAdapter : BusAdapter
        {
            private I2cDevice _device;

            /// <summary>
            /// Constructs I2cAdapter instance
            /// </summary>
            /// <param name="device">I2C device</param>
            public I2cAdapter(I2cDevice device) => _device = device;

            /// <inheritdoc/>
            public override void Dispose() => _device?.Dispose();

            /// <inheritdoc/>
            public override void Read(byte registerAddress, Span<byte> buffer)
            {
                // Set address to register first.
                Write(registerAddress, Span<byte>.Empty);
                _device.Read(buffer);
            }

            /// <inheritdoc/>
            public override void Write(byte registerAddress, Span<byte> data)
            {
                Span<byte> output = stackalloc byte[data.Length + 1];
                output[0] = registerAddress;
                data.CopyTo(output.Slice(1));
                _device.Write(output);
            }
        }
    }
}
