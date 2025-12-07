// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

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
    private Dictionary<byte, RegisterBase> _registerMap;
    private byte _currentRegister;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The connection settings for this device.</param>
    public I2cSimulatedDeviceBase(I2cConnectionSettings settings)
    {
        ConnectionSettings = settings;
        _registerMap = new Dictionary<byte, RegisterBase>();
        _disposed = false;
        _currentRegister = 0;
    }

    /// <summary>
    /// The registermap of this device.
    /// This should only be accessed from a derived class, except for test purposes.
    /// </summary>
    public Dictionary<byte, RegisterBase> RegisterMap => _registerMap;

    /// <summary>
    /// The active connection settings
    /// </summary>
    public override I2cConnectionSettings ConnectionSettings { get; }

    /// <summary>
    /// The active register.
    /// Can be set to mimic some non-standard behavior of setting a register (or if reading increases
    /// the register pointer, which is the case on some chips)
    /// </summary>
    protected byte CurrentRegister
    {
        get
        {
            return _currentRegister;
        }
        set
        {
            _currentRegister = value;
        }
    }

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
    /// This method implements the read operation from the device.
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
            return;
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

    /// <summary>
    /// Base class for generic register access
    /// </summary>
    public abstract record class RegisterBase : IComparable
    {
        /// <summary>
        /// Writes the register, regardless of its actual type
        /// </summary>
        /// <param name="value">The value to write</param>
        protected abstract void WriteRegister(int value);

        /// <summary>
        /// Reads the register value regardless of its actual type
        /// </summary>
        /// <returns>The register value, sign-extended to int</returns>
        protected abstract int ReadRegister();

        /// <inheritdoc />
        public abstract int CompareTo(object? obj);

        /// <summary>
        /// Gets the value as stored in the register
        /// </summary>
        /// <returns></returns>
        public abstract int GetValue();
    }

    /// <summary>
    /// Represents a register value
    /// </summary>
    /// <typeparam name="T">Size of the register, usually byte or int</typeparam>
    public record class Register<T> : RegisterBase
        where T : struct, IEquatable<T>, INumber<T>, IComparable
    {
        /// <summary>
        /// Event that is raised when the register is written
        /// </summary>
        private readonly Func<T, T>? _registerUpdateHandler;

        /// <summary>
        /// Event that is raised to read the register. Gets the internal value of the register
        /// and returns the value the client should see (e.g a random measurement value)
        /// </summary>
        private readonly Func<T, T>? _registerReadHandler;

        private T _value;

        /// <summary>
        /// Create a new register
        /// </summary>
        public Register()
            : this(default(T))
        {
        }

        /// <summary>
        /// Creates a new register
        /// </summary>
        /// <param name="initialValue">The initial (power-on-reset) value of the register</param>
        public Register(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Creates a new register with handlers
        /// </summary>
        /// <param name="initialValue">The initial value of the register at power-up</param>
        /// <param name="updateHandler">A handler for a register write. Can be null.</param>
        /// <param name="readHandler">A handler for a register read. Can be null.</param>
        public Register(T initialValue, Func<T, T>? updateHandler, Func<T, T>? readHandler)
        {
            _value = initialValue;
            _registerUpdateHandler = updateHandler;
            _registerReadHandler = readHandler;
        }

        /// <summary>
        /// The current value of the register
        /// </summary>
        public T Value
        {
            get
            {
                if (_registerReadHandler != null)
                {
                    return _registerReadHandler(_value);
                }

                return _value;
            }
            set
            {
                if (_registerUpdateHandler != null)
                {
                    _value = _registerUpdateHandler(value);
                    return;
                }

                _value = value;
            }
        }

        /// <inheritdoc />
        protected override void WriteRegister(int value)
        {
            if (Marshal.SizeOf<T>() == 2 && BitConverter.IsLittleEndian)
            {
                // The bus runs in big-endian mode
                int r = (value & 0xFF) << 8 | value >> 8;
                value = r;
            }

            Value = T.CreateChecked(value);
        }

        /// <inheritdoc />
        protected override int ReadRegister()
        {
            int ret = int.CreateChecked(Value);
            if (Marshal.SizeOf<T>() == 2 && BitConverter.IsLittleEndian)
            {
                // The bus runs in big-endian mode
                int r = (ret & 0xFF) << 8 | ret >> 8;
                ret = r;
            }

            return ret;
        }

        /// <summary>
        /// The value, for external access
        /// </summary>
        /// <returns>The value, sign-extended to int</returns>
        public override int GetValue()
        {
            return int.CreateChecked(Value);
        }

        /// <inheritdoc />
        public override int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is Register<T> t1)
            {
                return _value.CompareTo(t1._value);
            }

            throw new ArgumentException("These types can't be compared");
        }
    }
}
