// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.I2c
{
    internal class UnixI2cBus : I2cBus
    {
        private const string DefaultDevicePath = "/dev/i2c";
        private static readonly object s_initializationLock = new object();
        public int BusId { get; }
        protected int BusFileDescriptor { get; private set; }
        private HashSet<int>? _usedAddresses = null;

        public static new unsafe UnixI2cBus Create(int busId)
        {
            string deviceFileName = $"{DefaultDevicePath}-{busId}";
            lock (s_initializationLock)
            {
                int busFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);

                if (busFileDescriptor < 0)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()}. Can not open I2C device file '{deviceFileName}'.");
                }

                I2cFunctionalityFlags functionalityFlags;
                int result = Interop.ioctl(busFileDescriptor, (uint)I2cSettings.I2C_FUNCS, new IntPtr(&functionalityFlags));
                if (result < 0)
                {
                    functionalityFlags = 0;
                }

                if ((functionalityFlags & I2cFunctionalityFlags.I2C_FUNC_I2C) != 0)
                {
                    return new UnixI2cBus(busFileDescriptor, busId);
                }
                else
                {
                    return new UnixI2cFileTransferBus(busFileDescriptor, busId);
                }
            }
        }

        public UnixI2cBus(int busFileDescriptor, int busId)
        {
            BusId = busId;
            BusFileDescriptor = busFileDescriptor;
        }

        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (!HasValidFileDescriptor())
            {
                throw new ObjectDisposedException(nameof(UnixI2cBus));
            }

            _usedAddresses ??= new HashSet<int>();
            if (!_usedAddresses.Add(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            return CreateDeviceNoCheck(deviceAddress);
        }

        internal I2cDevice CreateDeviceNoCheck(int deviceAddress)
        {
            return new UnixI2cDevice(this, deviceAddress);
        }

        public override void RemoveDevice(int deviceAddress)
        {
            if (!RemoveDeviceNoCheck(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }
        }

        internal bool RemoveDeviceNoCheck(int deviceAddress)
        {
            return _usedAddresses?.Remove(deviceAddress) ?? false;
        }

        public override unsafe void Read(int deviceAddress, Span<byte> buffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (buffer.Length == 0)
            {
                throw new ArgumentException($"{nameof(buffer)} cannot be empty.", nameof(buffer));
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new ArgumentException($"{nameof(buffer)} length is too long.", nameof(buffer));
            }

            fixed (byte* readBufferPointer = buffer)
            {
                WriteReadCore((ushort)deviceAddress, null, readBufferPointer, 0, (ushort)buffer.Length);
            }
        }

        public override unsafe void Write(int deviceAddress, ReadOnlySpan<byte> buffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new ArgumentException($"{nameof(buffer)} length is too long.", nameof(buffer));
            }

            fixed (byte* writeBufferPointer = buffer)
            {
                WriteReadCore((ushort)deviceAddress, writeBufferPointer, null, (ushort)buffer.Length, 0);
            }
        }

        public override unsafe void WriteRead(int deviceAddress, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (readBuffer.Length == 0)
            {
                throw new ArgumentException($"{nameof(readBuffer)} cannot be empty.", nameof(readBuffer));
            }

            if (writeBuffer.Length > ushort.MaxValue)
            {
                throw new ArgumentException($"{nameof(writeBuffer)} length is too long.", nameof(writeBuffer));
            }

            if (readBuffer.Length > ushort.MaxValue)
            {
                throw new ArgumentException($"{nameof(readBuffer)} length is too long.", nameof(readBuffer));
            }

            fixed (byte* writeBufferPointer = writeBuffer)
            {
                fixed (byte* readBufferPointer = readBuffer)
                {
                    WriteReadCore((ushort)deviceAddress, writeBufferPointer, readBufferPointer, (ushort)writeBuffer.Length, (ushort)readBuffer.Length);
                }
            }
        }

        protected virtual unsafe void WriteReadCore(ushort deviceAddress, byte* writeBuffer, byte* readBuffer, ushort writeBufferLength, ushort readBufferLength)
        {
            // Allocating space for 2 messages in case we want to read and write on the same call.
            i2c_msg* messagesPtr = stackalloc i2c_msg[2];
            int messageCount = 0;

            if (writeBuffer != null)
            {
                messagesPtr[messageCount++] = new i2c_msg()
                {
                    flags = I2cMessageFlags.I2C_M_WR,
                    addr = deviceAddress,
                    len = writeBufferLength,
                    buf = writeBuffer
                };
            }

            if (readBuffer != null)
            {
                messagesPtr[messageCount++] = new i2c_msg()
                {
                    flags = I2cMessageFlags.I2C_M_RD,
                    addr = deviceAddress,
                    len = readBufferLength,
                    buf = readBuffer
                };
            }

            var msgset = new i2c_rdwr_ioctl_data()
            {
                msgs = messagesPtr,
                nmsgs = (uint)messageCount
            };

            int result = Interop.ioctl(BusFileDescriptor, (uint)I2cSettings.I2C_RDWR, new IntPtr(&msgset));
            if (result < 0)
            {
                throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
            }
        }

        private bool HasValidFileDescriptor()
        {
            return BusFileDescriptor >= 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (HasValidFileDescriptor())
            {
                Interop.close(BusFileDescriptor);
                BusFileDescriptor = -1;
            }

            _usedAddresses = null!;
            base.Dispose(disposing);
        }
    }
}
