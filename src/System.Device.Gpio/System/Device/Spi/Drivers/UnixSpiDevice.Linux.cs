// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;

namespace System.Device.Spi.Drivers
{
    public class UnixSpiDevice : SpiDevice
    {
        private const string Default_Device_Path = "/dev/spidev";
        private const uint SPI_IOC_MESSAGE_1 = 0x40206b00;
        private int _deviceFileDescriptor = -1;
        private SpiConnectionSettings _settings;
        private static readonly object s_InitializationLock = new object();

        public UnixSpiDevice(SpiConnectionSettings settings)
        {
            _settings = settings;
            DevicePath = Default_Device_Path;
        }

        public string DevicePath { get; set; }

        public override SpiConnectionSettings ConnectionSettings => _settings;

        private unsafe void Initialize()
        {
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }
            lock (s_InitializationLock)
            {
                string deviceFileName = $"{DevicePath}{_settings.BusId}.{_settings.ChipSelectLine}";
                if (_deviceFileDescriptor >= 0)
                {
                    return;
                }
                _deviceFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);
                if (_deviceFileDescriptor < 0)
                {
                    throw new IOException($"Cannot open Spi device file '{deviceFileName}'");
                }

                UnixSpiMode mode = SpiModeToUnixSpiMode(_settings.Mode);
                IntPtr nativePtr = new IntPtr(&mode);

                int result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_MODE, nativePtr);
                if (result == -1)
                {
                    throw new IOException($"Cannot set Spi mode to {_settings.Mode}");
                }

                byte dataLengthInBits = (byte)_settings.DataBitLength;
                nativePtr = new IntPtr(&dataLengthInBits);

                result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_BITS_PER_WORD, nativePtr);
                if (result == -1)
                {
                    throw new IOException($"Cannot set Spi data bit length to {_settings.DataBitLength}");
                }

                int clockFrequency = _settings.ClockFrequency;
                nativePtr = new IntPtr(&clockFrequency);

                result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_MAX_SPEED_HZ, nativePtr);
                if (result == -1)
                {
                    throw new IOException($"Cannot set Spi clock frequency to {_settings.ClockFrequency}");
                }
            }
        }

        private UnixSpiMode SpiModeToUnixSpiMode(SpiMode mode)
        {
            switch (mode)
            {
                case SpiMode.Mode0:
                    return UnixSpiMode.SPI_MODE_0;
                case SpiMode.Mode1:
                    return UnixSpiMode.SPI_MODE_1;
                case SpiMode.Mode2:
                    return UnixSpiMode.SPI_MODE_2;
                case SpiMode.Mode3:
                    return UnixSpiMode.SPI_MODE_3;
                default:
                    throw new ArgumentException("Invalid SpiMode", nameof(mode));
            }
        }

        public override unsafe byte ReadByte()
        {
            Initialize();

            int length = sizeof(byte);
            byte result = 0;
            Transfer(null, &result, length);

            return result;
        }

        public override unsafe void Read(Span<byte> buffer)
        {
            Initialize();

            fixed (byte* bufferPtr = buffer)
            {
                Transfer(null, bufferPtr, buffer.Length);
            }
        }

        public override unsafe void WriteByte(byte data)
        {
            Initialize();

            int length = sizeof(byte);
            Transfer(&data, null, length);
        }

        public override unsafe void Write(Span<byte> data)
        {
            Initialize();

            fixed (byte* dataPtr = data)
            {
                Transfer(dataPtr, null, data.Length);
            }
        }
        public override unsafe void TransferFullDuplex(Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Initialize();

            if (writeBuffer.Length != readBuffer.Length)
            {
                throw new ArgumentException($"Parameters '{nameof(writeBuffer)}' and '{nameof(readBuffer)}' must have the same length");
            }

            fixed (byte* writeBufferPtr = writeBuffer)
            fixed (byte* readBufferPtr = readBuffer)
            {
                Transfer(writeBufferPtr, readBufferPtr, writeBuffer.Length);
            }
        }

        private unsafe void Transfer(byte* writeBufferPtr, byte* readBufferPtr, int buffersLength)
        {
            var tr = new spi_ioc_transfer()
            {
                tx_buf = (ulong)writeBufferPtr,
                rx_buf = (ulong)readBufferPtr,
                len = (uint)buffersLength,
                speed_hz = (uint)_settings.ClockFrequency,
                bits_per_word = (byte)_settings.DataBitLength,
                delay_usecs = 0
            };

            int result = Interop.ioctl(_deviceFileDescriptor, SPI_IOC_MESSAGE_1, new IntPtr(&tr));
            if (result < 1)
            {
                throw new IOException("Error while performing the Spi data transfer.");
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
