// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Device.I2c;

/// <summary>
/// This class can be used to create a simulated I2C device.
/// Derive from it and implement the <see cref="Write"/> and <see cref="Read"/> commands
/// to behave as expected.
/// Can also serve as base for a testing mock.
/// </summary>
public abstract class I2cSimulatedDeviceBase : I2cDevice
{
    private bool _disposed;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The connection settings for this device.</param>
    public I2cSimulatedDeviceBase(I2cConnectionSettings settings)
    {
        ConnectionSettings = settings;
        _disposed = false;
    }

    /// <summary>
    /// The active connection settings
    /// </summary>
    public override I2cConnectionSettings ConnectionSettings { get; }

    /// <summary>
    /// Reads a byte from the bus
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException">The instance is disposed already</exception>
    /// <exception cref="IOException"></exception>
    public override byte ReadByte()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("This instance is disposed");
        }

        byte[] buffer = new byte[1];
        if (WriteRead([], buffer) == 1)
        {
            return buffer[0];
        }

        throw new IOException("Unable to read a byte from the device");
    }

    /// <summary>
    /// This method should implement the read operation from the device.
    /// </summary>
    /// <param name="inputBuffer">Buffer with input data to the device, buffer[0] is usually the command byte</param>
    /// <param name="outputBuffer">The return data from the device</param>
    /// <returns>How many bytes where read. Should usually match the length of the output buffer</returns>
    /// <remarks>This doesn't use <see cref="Span{T}"/> as argument type to be mockable</remarks>
    protected abstract int WriteRead(byte[] inputBuffer, byte[] outputBuffer);

    /// <inheritdoc />
    public override void Read(Span<byte> buffer)
    {
        byte[] buffer2 = buffer.ToArray();
        if (WriteRead([], buffer2) == buffer.Length)
        {
            buffer2.CopyTo(buffer);
        }

        throw new IOException($"Unable to read {buffer.Length} bytes from the device");
    }

    /// <inheritdoc />
    public override void WriteByte(byte value)
    {
        if (WriteRead([value], []) == 1)
        {
            return;
        }

        throw new IOException("Unable to write a byte to the device");
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        WriteRead(buffer.ToArray(), []);
    }

    /// <inheritdoc />
    public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
    {
        byte[] outBuffer = new byte[readBuffer.Length];
        if (WriteRead(writeBuffer.ToArray(), outBuffer) != readBuffer.Length)
        {
            throw new IOException($"Unable to read {readBuffer.Length} bytes from the device");
        }

        outBuffer.CopyTo(readBuffer);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _disposed = true;
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        var self = new ComponentInformation(this, "Simulated I2C Device");
        self.Properties["BusNo"] = ConnectionSettings.BusId.ToString(CultureInfo.InvariantCulture);
        self.Properties["DeviceAddress"] = $"0x{ConnectionSettings.DeviceAddress:x2}";
        return self;
    }
}
