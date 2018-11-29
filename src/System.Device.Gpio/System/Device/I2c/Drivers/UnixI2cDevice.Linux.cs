// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;

namespace System.Device.I2c.Drivers
{
    public class UnixI2cDevice : I2cDevice
    {
        private I2cConnectionSettings _settings;
        private const string DefaultDevicePath = "/dev/i2c";
        private int _deviceFileDescriptor = -1;
        private I2cFunctionalityFlags _functionalities;

        public UnixI2cDevice(I2cConnectionSettings settings)
        {
            _settings = settings;
            DevicePath = DefaultDevicePath;
        }

        public string DevicePath { get; set; }

        public override I2cConnectionSettings ConnectionSettings => throw new NotImplementedException();

        private unsafe void Initialize()
        {
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }

            string deviceFileName = $"{DevicePath}-{_settings.BusId}";
            _deviceFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);

            if (_deviceFileDescriptor < 0)
            {
                throw new IOException($"Cannot open I2c device file '{deviceFileName}'");
            }

            fixed (I2cFunctionalityFlags* functionalitiesPtr = &_functionalities)
            {
                int result = Interop.ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_FUNCS, new IntPtr(functionalitiesPtr));
                if (result < 0)
                {
                    _functionalities = 0;
                }
            }
        }

        private unsafe void Transfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            if (_functionalities.HasFlag(I2cFunctionalityFlags.I2C_FUNC_I2C))
            {
                RdWrInterfaceTransfer(writeBuffer, readBuffer, writeBufferLength, readBufferLength);
            }
            else
            {
                FileInterfaceTransfer(writeBuffer, readBuffer, writeBufferLength, readBufferLength);
            }
        }

        private unsafe void RdWrInterfaceTransfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            int messageCount = 0;

            if (writeBuffer != null)
            {
                messageCount++;
            }
            if (readBuffer != null)
            {
                messageCount++;
            }

            i2c_msg* messagesPtr = stackalloc i2c_msg[messageCount];
            messageCount = 0;

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

    internal unsafe struct i2c_msg
    {
        public ushort addr;
        public I2cMessageFlags flags;
        public ushort len;
        public byte* buf;
    }

    [Flags]
    internal enum I2cMessageFlags : ushort
    {
        /// <summary>Write data to slave</summary>
        I2C_M_WR = 0x0000,
        /// <summary>Read data from slave</summary>
        I2C_M_RD = 0x0001
    }

    internal unsafe struct i2c_rdwr_ioctl_data
    {
        public i2c_msg* msgs;
        public uint nmsgs;
    };
}
