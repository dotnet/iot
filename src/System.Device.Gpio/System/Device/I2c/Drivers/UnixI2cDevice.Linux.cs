// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.I2c.Drivers
{
    public class UnixI2cDevice : I2cDevice
    {
        private I2cConnectionSettings _settings;
        private const string DefaultDevicePath = "/dev/i2c";
        private int _deviceFileDescriptor = -1;
        private I2cFunctionalityFlags _functionalities;
        private static readonly object s_InitializationLock = new object();

        public UnixI2cDevice(I2cConnectionSettings settings)
        {
            _settings = settings;
            DevicePath = DefaultDevicePath;
        }

        public string DevicePath { get; set; }

        public override I2cConnectionSettings ConnectionSettings => _settings;

        private unsafe void Initialize()
        {
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }

            string deviceFileName = $"{DevicePath}-{_settings.BusId}";
            lock (s_InitializationLock)
            {
                if (_deviceFileDescriptor >= 0)
                {
                    return;
                }
                _deviceFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);

                if (_deviceFileDescriptor < 0)
                {
                    throw new IOException($"Cannot open I2c device file '{deviceFileName}'");
                }

                I2cFunctionalityFlags tempFlags;
                int result = Interop.ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_FUNCS, new IntPtr(&tempFlags));
                if (result < 0)
                {
                    _functionalities = 0;
                }
                _functionalities = tempFlags;
            }
        }

        private unsafe void Transfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            if (_functionalities.HasFlag(I2cFunctionalityFlags.I2C_FUNC_I2C))
            {
                ReadWriteInterfaceTransfer(writeBuffer, readBuffer, writeBufferLength, readBufferLength);
            }
            else
            {
                FileInterfaceTransfer(writeBuffer, readBuffer, writeBufferLength, readBufferLength);
            }
        }

        private unsafe void ReadWriteInterfaceTransfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            // Allocating space for 2 messages in case we want to read and write on the same call.
            i2c_msg* messagesPtr = stackalloc i2c_msg[2];
            int messageCount = 0;

            if (writeBuffer != null)
            {
                messagesPtr[messageCount++] = new i2c_msg()
                {
                    flags = I2cMessageFlags.I2C_M_WR,
                    addr = (ushort)_settings.DeviceAddress,
                    len = (ushort)writeBufferLength,
                    buf = writeBuffer
                };
            }

            if (readBuffer != null)
            {
                messagesPtr[messageCount++] = new i2c_msg()
                {
                    flags = I2cMessageFlags.I2C_M_RD,
                    addr = (ushort)_settings.DeviceAddress,
                    len = (ushort)readBufferLength,
                    buf = readBuffer
                };
            }

            var tr = new i2c_rdwr_ioctl_data()
            {
                msgs = messagesPtr,
                nmsgs = (uint)messageCount
            };

            int result = Interop.ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_RDWR, new IntPtr(&tr));
            if (result < 0)
            {
                throw new IOException("Error when attempting to perform the I2c data transfer.");
            }
        }

        private unsafe void FileInterfaceTransfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            int result = Interop.ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_SLAVE_FORCE, (ulong)_settings.DeviceAddress);
            if (result < 0)
            {
                throw new IOException("Error performing I2c data transfer");
            }

            if (writeBuffer != null)
            {
                result = Interop.write(_deviceFileDescriptor, new IntPtr(writeBuffer), writeBufferLength);
                if (result < 0)
                {
                    throw new IOException("Error performing I2c data transfer");
                }
            }

            if (readBuffer != null)
            {
                result = Interop.read(_deviceFileDescriptor, new IntPtr(readBuffer), readBufferLength);
                if (result < 0)
                {
                    throw new IOException("Error performing I2c data transfer");
                }
            }
        }

        public override unsafe byte ReadByte()
        {
            Initialize();

            int length = sizeof(byte);
            byte result = 0;
            Transfer(null, &result, 0, length);
            return result;
        }

        public override unsafe void Read(Span<byte> buffer)
        {
            Initialize();

            fixed (byte* bufferPointer = buffer)
            {
                Transfer(null, bufferPointer, 0, buffer.Length);
            }
        }

        public override unsafe void WriteByte(byte data)
        {
            Initialize();

            int length = sizeof(byte);
            Transfer(&data, null, length, 0);
        }

        public override unsafe void Write(Span<byte> data)
        {
            Initialize();

            fixed (byte* dataPointer = data)
            {
                Transfer(dataPointer, null, data.Length, 0);
            }
        }

        public override void Dispose(bool disposing)
        {
            if (_deviceFileDescriptor >= 0)
            {
                Interop.close(_deviceFileDescriptor);
                _deviceFileDescriptor = -1;
            }
            base.Dispose(disposing);
        }
    }
}
