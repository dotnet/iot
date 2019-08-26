// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.I2c
{
    /// <summary>
    /// Represents an I2C communication channel running on Unix.
    /// </summary>
    internal class UnixI2cDevice : I2cDevice
    {
        private readonly I2cConnectionSettings _settings;
        private const string DefaultDevicePath = "/dev/i2c";
        private int _deviceFileDescriptor = -1;
        private I2cFunctionalityFlags _functionalities;
        private static readonly object s_initializationLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixI2cDevice"/> class that will use the specified settings to communicate with the I2C device.
        /// </summary>
        /// <param name="settings">
        /// The connection settings of a device on an I2C bus.
        /// </param>
        public UnixI2cDevice(I2cConnectionSettings settings)
        {
            _settings = settings;
            DevicePath = DefaultDevicePath;
        }

        /// <summary>
        /// Path to I2C resources located on the platform.
        /// </summary>
        public string DevicePath { get; set; }

        /// <summary>
        /// The connection settings of a device on an I2C bus. The connection settings are immutable after the device is created
        /// so the object returned will be a clone of the settings object.
        /// </summary>
        public override I2cConnectionSettings ConnectionSettings => new I2cConnectionSettings(_settings);

        private unsafe void Initialize()
        {
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }

            string deviceFileName = $"{DevicePath}-{_settings.BusId}";
            lock (s_initializationLock)
            {
                if (_deviceFileDescriptor >= 0)
                {
                    return;
                }
                _deviceFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);

                if (_deviceFileDescriptor < 0)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()}. Can not open I2C device file '{deviceFileName}'.");
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
            if ((_functionalities & I2cFunctionalityFlags.I2C_FUNC_I2C) != 0)
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

            var msgset = new i2c_rdwr_ioctl_data()
            {
                msgs = messagesPtr,
                nmsgs = (uint)messageCount
            };

            int result = Interop.ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_RDWR, new IntPtr(&msgset));
            if (result < 0)
            {
                throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
            }
        }

        private unsafe void FileInterfaceTransfer(byte* writeBuffer, byte* readBuffer, int writeBufferLength, int readBufferLength)
        {
            int result = Interop.ioctl(_deviceFileDescriptor, (uint)I2cSettings.I2C_SLAVE_FORCE, (ulong)_settings.DeviceAddress);
            if (result < 0)
            {
                throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
            }

            if (writeBuffer != null)
            {
                result = Interop.write(_deviceFileDescriptor, new IntPtr(writeBuffer), writeBufferLength);
                if (result < 0)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
                }
            }

            if (readBuffer != null)
            {
                result = Interop.read(_deviceFileDescriptor, new IntPtr(readBuffer), readBufferLength);
                if (result < 0)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} performing I2C data transfer.");
                }
            }
        }

        /// <summary>
        /// Reads a byte from the I2C device.
        /// </summary>
        /// <returns>A byte read from the I2C device.</returns>
        public override unsafe byte ReadByte()
        {
            Initialize();

            int length = sizeof(byte);
            byte result = 0;
            Transfer(null, &result, 0, length);
            return result;
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public override unsafe void Read(Span<byte> buffer)
        {
            if (buffer.Length == 0)
                throw new ArgumentException($"{nameof(buffer)} cannot be empty.");

            Initialize();

            fixed (byte* bufferPointer = buffer)
            {
                Transfer(null, bufferPointer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Writes a byte to the I2C device.
        /// </summary>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public override unsafe void WriteByte(byte value)
        {
            Initialize();

            int length = sizeof(byte);
            Transfer(&value, null, length, 0);
        }

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="value">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public override unsafe void Write(ReadOnlySpan<byte> value)
        {
            Initialize();

            fixed (byte* dataPointer = value)
            {
                Transfer(dataPointer, null, value.Length, 0);
            }
        }

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the I2C bus on which the device is connected, 
        /// and sends a restart condition between the write and read operations.
        /// </summary>
        /// <param name="writeBuffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.</param>
        /// <param name="readBuffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public override unsafe void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (readBuffer.Length == 0)
                throw new ArgumentException($"{nameof(readBuffer)} cannot be empty.");

            Initialize();

            fixed (byte* writeBufferPointer = writeBuffer)
            {
                fixed (byte* readBufferPointer = readBuffer)
                {
                    Transfer(writeBufferPointer, readBufferPointer, writeBuffer.Length, readBuffer.Length);
                }
            }
        }

        protected override void Dispose(bool disposing)
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
