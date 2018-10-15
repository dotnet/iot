// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Runtime.InteropServices;

namespace System.Devices.I2c
{
    public class UnixI2cDevice : I2cDevice
    {
        #region Interop

        private const string LibraryName = "libc";

        [Flags]
        private enum FileOpenFlags
        {
            O_RDONLY = 0x00,
            O_NONBLOCK = 0x800,
            O_RDWR = 0x02,
            O_SYNC = 0x101000
        }

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, FileOpenFlags flags);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int close(int fd);

        private enum I2cSettings : uint
        {
            /// <summary>Combined R/W transfer (one STOP only)</summary>
            I2C_RDWR = 0x0707,
            /// <summary>Smbus transfer</summary>
            I2C_SMBUS = 0x0720,
            /// <summary>Get the adapter functionality mask</summary>
            I2C_FUNCS = 0x0705,
            /// <summary>Use this slave address, even if it is already in use by a driver</summary>
            I2C_SLAVE_FORCE = 0x0706
        }

        /// To determine what functionality is supported
        [Flags]
        private enum I2cFunctionalityFlags : ulong
        {
            I2C_FUNC_I2C = 0x00000001,
            I2C_FUNC_SMBUS_BLOCK_DATA = 0x03000000
        }

        [Flags]
        private enum I2cMessageFlags : ushort
        {
            /// <summary>Write data to slave</summary>
            I2C_M_WR = 0x0000,
            /// <summary>Read data from slave</summary>
            I2C_M_RD = 0x0001
        }

        private unsafe struct i2c_msg
        {
            public ushort addr;
            public I2cMessageFlags flags;
            public ushort len;
            public byte* buf;
        };

        /// <summary>Used in the <see cref="I2C_RDWR"/> <see cref="ioctl"/> call</summary>
        private unsafe struct i2c_rdwr_ioctl_data
        {
            public i2c_msg* msgs;
            public uint nmsgs;
        };

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int ioctl(int fd, uint request, IntPtr argp);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int ioctl(int fd, uint request, ulong argp);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int read(int fd, IntPtr buf, int count);

        [DllImport(LibraryName, SetLastError = true)]
        private static extern int write(int fd, IntPtr buf, int count);

        #endregion

        private const string DefaultDevicePath = "/dev/i2c";

        private int _deviceFileDescriptor = -1;
        private I2cFunctionalityFlags _functionalities;

        public UnixI2cDevice(I2cConnectionSettings settings)
            : base(settings)
        {
            DevicePath = DefaultDevicePath;
        }

        public override void Dispose()
        {
            if (_deviceFileDescriptor >= 0)
            {
                close(_deviceFileDescriptor);
                _deviceFileDescriptor = -1;
            }
        }

        public string DevicePath { get; set; }

        private unsafe void Initialize()
        {
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }

            string deviceFileName = $"{DevicePath}-{_settings.BusId}";
            _deviceFileDescriptor = open(deviceFileName, FileOpenFlags.O_RDWR);

            if (_deviceFileDescriptor < 0)
            {
                throw Utils.CreateIOException($"Cannot open I2c device file '{deviceFileName}'", _deviceFileDescriptor);
            }

            fixed (I2cFunctionalityFlags* functionalitiesPtr = &_functionalities)
            {
                int ret = ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_FUNCS, new IntPtr(functionalitiesPtr));
                if (ret < 0)
                {
                    _functionalities = 0;
                }
            }
        }

        public override unsafe void Read(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            Initialize();

            fixed (byte* rxPtr = buffer)
            {
                Transfer(null, rxPtr, 0, buffer.Length);
            }
        }

        public override unsafe byte Read8()
        {
            Initialize();

            int length = sizeof(byte);
            byte result = 0;
            Transfer(null, &result, 0, length);
            return result;
        }

        public override unsafe ushort Read16()
        {
            Initialize();

            int length = sizeof(ushort);
            ushort result = 0;
            Transfer(null, (byte*)&result, 0, length);

            result = Utils.SwapBytes(result);
            return result;
        }

        public override unsafe uint Read24()
        {
            Initialize();

            const int length = 3;
            uint result = 0;
            Transfer(null, (byte*)&result, 0, length);

            result = result << 8;
            result = Utils.SwapBytes(result);
            return result;
        }

        public override unsafe uint Read32()
        {
            Initialize();

            int length = sizeof(uint);
            uint result = 0;
            Transfer(null, (byte*)&result, 0, length);

            result = Utils.SwapBytes(result);
            return result;
        }

        public override unsafe ulong Read64()
        {
            Initialize();

            int length = sizeof(ulong);
            ulong result = 0;
            Transfer(null, (byte*)&result, 0, length);

            result = Utils.SwapBytes(result);
            return result;
        }

        public override unsafe void Write(params byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            Initialize();

            fixed (byte* txPtr = buffer)
            {
                Transfer(txPtr, null, buffer.Length, 0);
            }
        }

        public override unsafe void Write8(byte value)
        {
            Initialize();

            int length = sizeof(byte);
            Transfer(&value, null, length, 0);
        }

        public override unsafe void Write16(ushort value)
        {
            Initialize();

            int length = sizeof(ushort);
            Transfer((byte*)&value, null, length, 0);
        }

        public override unsafe void Write24(uint value)
        {
            Initialize();

            value = value & 0xFFFFFF;
            const int length = 3;
            Transfer((byte*)&value, null, length, 0);
        }

        public override unsafe void Write32(uint value)
        {
            Initialize();

            int length = sizeof(uint);
            Transfer((byte*)&value, null, length, 0);
        }

        public override unsafe void Write64(ulong value)
        {
            Initialize();

            int length = sizeof(ulong);
            Transfer((byte*)&value, null, length, 0);
        }

        public override unsafe void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            if (writeBuffer == null)
            {
                throw new ArgumentNullException(nameof(writeBuffer));
            }

            if (readBuffer == null)
            {
                throw new ArgumentNullException(nameof(readBuffer));
            }

            Initialize();

            fixed (byte* txPtr = writeBuffer, rxPtr = readBuffer)
            {
                Transfer(txPtr, rxPtr, writeBuffer.Length, readBuffer.Length);
            }
        }

        private unsafe void Transfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            if (_functionalities.HasFlag(I2cFunctionalityFlags.I2C_FUNC_I2C))
            {
                //Console.WriteLine("Using I2c RdWr interface");

                RdWrInterfaceTransfer(writeBuffer, readBuffer, writeBufferLength, readBufferLength);
            }
            else
            {
                //Console.WriteLine("Using I2c file interface");

                FileInterfaceTransfer(writeBuffer, readBuffer, writeBufferLength, readBufferLength);
            }
        }

        private unsafe void RdWrInterfaceTransfer(byte* writeBufferPtr, byte* readBufferPtr, int writeBufferLength, int readBufferLength)
        {
            int messageCount = 0;

            if (writeBufferPtr != null)
            {
                messageCount++;
            }

            if (readBufferPtr != null)
            {
                messageCount++;
            }

            i2c_msg* messagesPtr = stackalloc i2c_msg[messageCount];
            messageCount = 0;

            if (writeBufferPtr != null)
            {
                messagesPtr[messageCount++] = new i2c_msg()
                {
                    flags = I2cMessageFlags.I2C_M_WR,
                    addr = (ushort)_settings.DeviceAddress,
                    len = (ushort)writeBufferLength,
                    buf = writeBufferPtr
                };
            }

            if (readBufferPtr != null)
            {
                messagesPtr[messageCount++] = new i2c_msg()
                {
                    flags = I2cMessageFlags.I2C_M_RD,
                    addr = (ushort)_settings.DeviceAddress,
                    len = (ushort)readBufferLength,
                    buf = readBufferPtr
                };
            }

            var tr = new i2c_rdwr_ioctl_data()
            {
                msgs = messagesPtr,
                nmsgs = (uint)messageCount
            };

            int ret = ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_RDWR, new IntPtr(&tr));
            if (ret < 0)
            {
                throw Utils.CreateIOException("Error performing I2c data transfer", ret);
            }
        }

        private unsafe void FileInterfaceTransfer(byte* writeBufferPtr, byte* readBufferPtr, int writeBufferLength, int readBufferLength)
        {
            int ret = ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_SLAVE_FORCE, (ulong)_settings.DeviceAddress);
            if (ret < 0)
            {
                throw Utils.CreateIOException("Error performing I2c data transfer", ret);
            }

            if (writeBufferPtr != null)
            {
                ret = write(_deviceFileDescriptor, new IntPtr(writeBufferPtr), writeBufferLength);

                if (ret < 0)
                {
                    throw Utils.CreateIOException("Error performing I2c data transfer", ret);
                }
            }

            if (readBufferPtr != null)
            {
                ret = read(_deviceFileDescriptor, new IntPtr(readBufferPtr), readBufferLength);

                if (ret < 0)
                {
                    throw Utils.CreateIOException("Error performing I2c data transfer", ret);
                }
            }
        }
    }
}
