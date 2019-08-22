// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    public abstract partial class Mcp23xxx
    {
        protected abstract class BusAdapter : IDisposable
        {
            public abstract void Read(byte registerAddress, Span<byte> buffer);
            public abstract void Write(byte registerAddress, Span<byte> data);

            public abstract void Dispose();
        }
    }
}
