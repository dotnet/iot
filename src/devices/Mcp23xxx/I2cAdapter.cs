// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public abstract partial class Mcp23xxx
    {
        protected class I2cAdapter : BusAdapter
        {
            private I2cDevice _device;

            public I2cAdapter(I2cDevice device) => _device = device;

            public override void Dispose() => _device?.Dispose();

            public override void Read(byte registerAddress, Span<byte> buffer)
            {
                // Set address to register first.
                Write(registerAddress, Span<byte>.Empty);
                _device.Read(buffer);
            }

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
