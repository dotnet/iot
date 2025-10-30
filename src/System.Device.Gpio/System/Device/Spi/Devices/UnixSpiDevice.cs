﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.Spi;

/// <summary>
/// Represents a SPI communication channel running on Unix.
/// </summary>
internal class UnixSpiDevice : SpiDevice
{
    private const string DefaultDevicePath = "/dev/spidev";
    private const uint SPI_IOC_MESSAGE_1 = 0x40206b00;
    private const int MaxStackallocBuffer = 256;
    private static readonly object s_initializationLock = new object();
    private readonly SpiConnectionSettings _settings;
    private int _deviceFileDescriptor = -1;
    private bool _isInverted = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnixSpiDevice"/> class that will use the specified settings to communicate with the SPI device.
    /// </summary>
    /// <param name="settings">
    /// The connection settings of a device on a SPI bus.
    /// </param>
    public UnixSpiDevice(SpiConnectionSettings settings)
    {
        _settings = settings;
        DevicePath = DefaultDevicePath;
    }

    /// <summary>
    /// Path to SPI resources located on the platform.
    /// </summary>
    public string DevicePath { get; set; }

    /// <summary>
    /// The connection settings of a device on a SPI bus. The connection settings are immutable after the device is created
    /// so the object returned will be a clone of the settings object.
    /// </summary>
    public override SpiConnectionSettings ConnectionSettings => new SpiConnectionSettings(_settings);

    private unsafe void Initialize()
    {
        if (_deviceFileDescriptor >= 0)
        {
            return;
        }

        lock (s_initializationLock)
        {
            // -1 means ignore Chip Select Line
            int chipSelectLine = _settings.ChipSelectLine == -1 ? 0 : _settings.ChipSelectLine;

            string deviceFileName = $"{DevicePath}{_settings.BusId}.{chipSelectLine}";
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }

            _deviceFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);
            if (_deviceFileDescriptor < 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                string errorMessage = Marshal.GetLastPInvokeErrorMessage();
                string error = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
                throw new IOException($"Error {error}. Can not open SPI device file '{deviceFileName}'.");
            }

            UnixSpiMode mode = SpiSettingsToUnixSpiMode();
            IntPtr nativePtr = new IntPtr(&mode);

            int result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_MODE, nativePtr);
            if (result == -1)
            {
                // We should try the other mode and invert the buffers manually
                // RPI can't open Mode0 in LSB for example
                if (_settings.DataFlow == DataFlow.LsbFirst)
                {
                    mode &= ~UnixSpiMode.SPI_LSB_FIRST;
                }
                else
                {
                    mode |= UnixSpiMode.SPI_LSB_FIRST;
                }

                nativePtr = new IntPtr(&mode);
                result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_MODE, nativePtr);
                if (result == -1)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    string errorMessage = Marshal.GetLastPInvokeErrorMessage();
                    string error = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
                    throw new IOException($"Error {error}. Can not set SPI mode to {_settings.Mode}.");
                }

                _isInverted = true;
            }

            byte dataLengthInBits = (byte)_settings.DataBitLength;
            nativePtr = new IntPtr(&dataLengthInBits);

            result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_BITS_PER_WORD, nativePtr);
            if (result == -1)
            {
                int errorCode = Marshal.GetLastWin32Error();
                string errorMessage = Marshal.GetLastPInvokeErrorMessage();
                string error = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
                throw new IOException($"Error {error}. Can not set SPI data bit length to {_settings.DataBitLength}.");
            }

            int clockFrequency = _settings.ClockFrequency;
            nativePtr = new IntPtr(&clockFrequency);

            result = Interop.ioctl(_deviceFileDescriptor, (uint)SpiSettings.SPI_IOC_WR_MAX_SPEED_HZ, nativePtr);
            if (result == -1)
            {
                int errorCode = Marshal.GetLastWin32Error();
                string errorMessage = Marshal.GetLastPInvokeErrorMessage();
                string error = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
                throw new IOException($"Error {error}. Can not set SPI clock frequency to {_settings.ClockFrequency}.");
            }
        }
    }

    private UnixSpiMode SpiSettingsToUnixSpiMode()
    {
        UnixSpiMode mode = SpiModeToUnixSpiMode(_settings.Mode);

        if (_settings.ChipSelectLine == -1)
        {
            mode |= UnixSpiMode.SPI_NO_CS;
        }
        else if (_settings.ChipSelectLineActiveState == PinValue.High)
        {
            mode |= UnixSpiMode.SPI_CS_HIGH;
        }

        if (_settings.DataFlow == DataFlow.LsbFirst)
        {
            mode |= UnixSpiMode.SPI_LSB_FIRST;
        }

        return mode;
    }

    private UnixSpiMode SpiModeToUnixSpiMode(SpiMode mode)
    {
        return mode switch
        {
            SpiMode.Mode0 => UnixSpiMode.SPI_MODE_0,
            SpiMode.Mode1 => UnixSpiMode.SPI_MODE_1,
            SpiMode.Mode2 => UnixSpiMode.SPI_MODE_2,
            SpiMode.Mode3 => UnixSpiMode.SPI_MODE_3,
            _ => throw new ArgumentException("Invalid SPI mode.", nameof(mode))
        };
    }

    /// <summary>
    /// Reads a byte from the SPI device.
    /// </summary>
    /// <returns>A byte read from the SPI device.</returns>
    public override unsafe byte ReadByte()
    {
        Initialize();

        int length = sizeof(byte);
        byte result = 0;
        Transfer(null, &result, length);

        return _isInverted ? ReverseByte(result) : result;
    }

    /// <summary>
    /// Reads data from the SPI device.
    /// </summary>
    /// <param name="buffer">
    /// The buffer to read the data from the SPI device.
    /// The length of the buffer determines how much data to read from the SPI device.
    /// </param>
    public override unsafe void Read(Span<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            throw new ArgumentException($"{nameof(buffer)} cannot be empty.");
        }

        Initialize();

        fixed (byte* bufferPtr = buffer)
        {
            Transfer(null, bufferPtr, buffer.Length);
        }

        if (_isInverted)
        {
            ReverseByte(buffer);
        }
    }

    /// <summary>
    /// Writes a byte to the SPI device.
    /// </summary>
    /// <param name="value">The byte to be written to the SPI device.</param>
    public override unsafe void WriteByte(byte value)
    {
        Initialize();

        if (_isInverted)
        {
            value = ReverseByte(value);
        }

        int length = sizeof(byte);
        Transfer(&value, null, length);
    }

    /// <summary>
    /// Writes data to the SPI device.
    /// </summary>
    /// <param name="buffer">
    /// The buffer that contains the data to be written to the SPI device.
    /// </param>
    public override unsafe void Write(ReadOnlySpan<byte> buffer)
    {
        Initialize();

        if (_isInverted)
        {
            Span<byte> toSend = buffer.Length > MaxStackallocBuffer ? new byte[buffer.Length] : stackalloc byte[buffer.Length];
            buffer.CopyTo(toSend);
            ReverseByte(toSend);
            fixed (byte* dataPtr = toSend)
            {
                Transfer(dataPtr, null, buffer.Length);
            }
        }
        else
        {
            fixed (byte* dataPtr = buffer)
            {
                Transfer(dataPtr, null, buffer.Length);
            }
        }
    }

    /// <summary>
    /// Writes and reads data from the SPI device.
    /// </summary>
    /// <param name="writeBuffer">The buffer that contains the data to be written to the SPI device.</param>
    /// <param name="readBuffer">The buffer to read the data from the SPI device.</param>
    public override unsafe void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
    {
        Initialize();

        if (writeBuffer.Length != readBuffer.Length)
        {
            throw new ArgumentException($"Parameters '{nameof(writeBuffer)}' and '{nameof(readBuffer)}' must have the same length.");
        }

        if (_isInverted)
        {
            Span<byte> toSend = writeBuffer.Length > MaxStackallocBuffer ? new byte[writeBuffer.Length] : stackalloc byte[writeBuffer.Length];
            writeBuffer.CopyTo(toSend);
            ReverseByte(toSend);
            fixed (byte* writeBufferPtr = toSend)
            {
                fixed (byte* readBufferPtr = readBuffer)
                {
                    Transfer(writeBufferPtr, readBufferPtr, writeBuffer.Length);
                }
            }

            ReverseByte(readBuffer);
        }
        else
        {
            fixed (byte* writeBufferPtr = writeBuffer)
            {
                fixed (byte* readBufferPtr = readBuffer)
                {
                    Transfer(writeBufferPtr, readBufferPtr, writeBuffer.Length);
                }
            }
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
            int errorCode = Marshal.GetLastWin32Error();
            string errorMessage = Marshal.GetLastPInvokeErrorMessage();
            string error = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
            throw new IOException($"Error {error} performing SPI data transfer.");
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
