//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// Encapsulates low-level access to read from and write to the provided
    /// Qwiic Button device.
    /// </summary>
    internal class I2cBusAccess : IDisposable
    {
        private I2cDevice _device;

        public I2cBusAccess(I2cDevice device)
        {
            _device = device;
        }

        internal byte ReadSingleRegister(Register register)
        {
            Span<byte> readBuffer = new byte[1];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<byte>(readBuffer);
        }

        internal ushort ReadDoubleRegister(Register register)
        {
            Span<byte> readBuffer = new byte[2];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<ushort>(readBuffer);
        }

        internal uint ReadQuadRegister(Register register)
        {
            Span<byte> readBuffer = new byte[4];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<uint>(readBuffer);
        }

        internal bool WriteSingleRegister(Register register, byte data)
        {
            _device.Write(new[] { (byte)register, data });
            return true;
        }

        internal bool WriteDoubleRegister(Register register, ushort data)
        {
            byte lower = (byte)(data & 0xff);
            byte upper = (byte)(data >> 8);
            _device.Write(new ReadOnlySpan<byte>(new[]
            {
               (byte)register,
               lower,
               upper
            }));

            return true;
        }

        internal byte WriteSingleRegisterWithReadback(Register register, byte data)
        {
            if (WriteSingleRegister(register, data))
            {
                return 1;
            }

            if (ReadSingleRegister(register) != data)
            {
                return 2;
            }

            return 0;
        }

        internal ushort WriteDoubleRegisterWithReadback(Register register, ushort data)
        {
            if (WriteDoubleRegister(register, data))
            {
                return 1;
            }

            if (ReadDoubleRegister(register) != data)
            {
                return 2;
            }

            return 0;
        }

        private static ReadOnlySpan<byte> ToReadOnlySpan(Register registerValue)
        {
            return new ReadOnlySpan<byte>(new[] { (byte)registerValue });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }
    }
}
