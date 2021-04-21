// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.I2c
{
    internal class UnixI2cFileTransferBus : UnixI2cBus
    {
        public UnixI2cFileTransferBus(int busFileDescriptor, int busId)
            : base(busFileDescriptor, busId)
        {
        }

        protected override unsafe bool WriteReadCore(ushort deviceAddress, byte* writeBuffer, byte* readBuffer, ushort writeBufferLength, ushort readBufferLength, bool throwExceptionOnIOError)
        {
            int result = Interop.ioctl(BusFileDescriptor, (uint)I2cSettings.I2C_SLAVE_FORCE, deviceAddress);
            if (result < 0)
            {
                if (throwExceptionOnIOError)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
                }
                else
                {
                    return false;
                }
            }

            if (writeBuffer != null)
            {
                result = Interop.write(BusFileDescriptor, new IntPtr(writeBuffer), writeBufferLength);
                if (result < 0)
                {
                    if (throwExceptionOnIOError)
                    {
                        throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (readBuffer != null)
            {
                result = Interop.read(BusFileDescriptor, new IntPtr(readBuffer), readBufferLength);
                if (result < 0)
                {
                    if (throwExceptionOnIOError)
                    {
                        throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
