//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// Implements low-level access to read from and write to the provided I2C device.
    /// Takes as generic parameter an <see cref="Enum"/> where each value represents a register
    /// and is assigned an integer address, thereby forming a register map.
    /// </summary>
    internal sealed class I2cRegisterAccess<T> : IDisposable
        where T : Enum
    {
        private I2cDevice _device;

        public I2cRegisterAccess(I2cDevice device)
        {
            _device = device;
        }

        internal byte ReadSingleRegister(T register)
        {
            Span<byte> readBuffer = new byte[1];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<byte>(readBuffer);
        }

        internal ushort ReadDoubleRegister(T register)
        {
            Span<byte> readBuffer = new byte[2];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<ushort>(readBuffer);
        }

        internal uint ReadQuadRegister(T register)
        {
            Span<byte> readBuffer = new byte[4];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<uint>(readBuffer);
        }

        internal void WriteSingleRegister(T register, byte data)
        {
            _device.Write(new[] { (byte)(object)register, data });
        }

        internal void WriteDoubleRegister(T register, ushort data)
        {
            byte lower = (byte)(data & 0xff);
            byte upper = (byte)(data >> 8);
            _device.Write(new ReadOnlySpan<byte>(new[]
            {
               (byte)(object)register,
               lower,
               upper
            }));
        }

        private static ReadOnlySpan<byte> ToReadOnlySpan(T registerValue)
        {
            return new ReadOnlySpan<byte>(new[] { (byte)(object)registerValue });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }
    }
}
