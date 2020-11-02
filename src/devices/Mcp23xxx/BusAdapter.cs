// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    public abstract partial class Mcp23xxx
    {
        /// <summary>
        /// Bus adapter
        /// </summary>
        protected abstract class BusAdapter : IDisposable
        {
            /// <summary>
            /// Reads bytes from the device register
            /// </summary>
            /// <param name="registerAddress">Register address</param>
            /// <param name="buffer">Bytes to be read from the register</param>
            public abstract void Read(byte registerAddress, Span<byte> buffer);

            /// <summary>
            /// Writes bytes to the device register
            /// </summary>
            /// <param name="registerAddress">Register address</param>
            /// <param name="data">Bytes to be written to the register</param>
            public abstract void Write(byte registerAddress, Span<byte> data);

            /// <inheritdoc/>
            public abstract void Dispose();
        }
    }
}
