//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.Common
{
    /// <summary>
    /// Implements low-level access to read and write to the provided I2C device.
    /// Takes as generic parameter an <see cref="Enum"/> where each value represents a register
    /// with its corresponding address, thereby forming a register map. Example:
    /// <example>
    /// <code>
    /// enum RegisterMap : byte
    /// {
    ///     Register1 = 0x00,
    ///     Register2 = 0x01,
    ///     Register3 = 0x03
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public sealed class I2cRegisterAccess<TRegisterMap> : IDisposable
        where TRegisterMap : Enum
    {
        private I2cDevice _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="I2cRegisterAccess{TRegisterMap}"/> class.
        /// </summary>
        /// <param name="device">I2C device to access.</param>
        public I2cRegisterAccess(I2cDevice device)
        {
            _device = device;
        }

        /// <summary>
        /// Reads an 8-bit integer from the provided register address.
        /// </summary>
        public byte ReadSingleRegister(TRegisterMap register)
        {
            Span<byte> readBuffer = new byte[1];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<byte>(readBuffer);
        }

        /// <summary>
        /// Reads a 16-bit integer from the provided register address.
        /// </summary>
        public ushort ReadDoubleRegister(TRegisterMap register)
        {
            Span<byte> readBuffer = new byte[2];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<ushort>(readBuffer);
        }

        /// <summary>
        /// Reads a 32-bit integer from the provided register address.
        /// </summary>
        public uint ReadQuadRegister(TRegisterMap register)
        {
            Span<byte> readBuffer = new byte[4];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<uint>(readBuffer);
        }

        /// <summary>
        /// Writes an 8-bit integer to the provided register address.
        /// </summary>
        public void WriteSingleRegister(TRegisterMap register, byte data)
        {
            _device.Write(new[] { (byte)(object)register, data });
        }

        /// <summary>
        /// Writes a 16-bit integer to the provided register address.
        /// </summary>
        public void WriteDoubleRegister(TRegisterMap register, ushort data)
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

        private static ReadOnlySpan<byte> ToReadOnlySpan(TRegisterMap registerValue)
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
