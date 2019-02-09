// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    internal class I2cAdapter : IBusDevice
    {
        private I2cDevice _device;

        public I2cAdapter(I2cDevice device) => _device = device;

        public void Dispose() => _device?.Dispose();

        public void Read(byte registerAddress, Span<byte> buffer)
        {
            // Set address to register first.
            Write(registerAddress, Span<byte>.Empty);
            _device.Read(buffer);
        }

        public void Write(byte registerAddress, Span<byte> data)
        {
            Span<byte> output = stackalloc byte[data.Length + 1];
            output[0] = registerAddress;
            data.CopyTo(output.Slice(1));
            _device.Write(output);
        }
    }
}
