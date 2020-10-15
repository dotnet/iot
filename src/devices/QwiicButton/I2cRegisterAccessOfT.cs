// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// Implements low-level access to read and write to the provided I2C device.
    /// Takes as generic parameter an <see cref="Enum"/> where each value represents a register
    /// with its corresponding address, thereby forming a register map.
    /// The underlying type of the <see cref="Enum"/> must be <see cref="byte"/>.
    /// Example:
    /// <example>
    /// <code>
    /// enum RegisterMap : byte
    /// {
    ///     Register1 = 0x00,
    ///     Register2 = 0x01,
    ///     Register3 = 0x03
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// Notice: If the I2C device uses register addresses larger than a <see cref="byte"/>,
    /// it often means that you have to read the data at several individual 8-bit addresses and
    /// assemble this data in the correct order to extract the corresponding 16/32/64-bit word.
    /// For instance, for some devices a register address such as 0x1234 means that you need to
    /// read/write from registers 0x12 and 0x34. Both registers will hold 8-bits of information,
    /// which together form the actual 16-bit word referenced by the hexadecimal 0x1234.
    /// Read through the data sheets/manuals for your specific I2C device for more information
    /// on its register addressing.
    /// </remarks>
    /// </summary>
    public sealed class I2cRegisterAccess<TRegisterMap> : IDisposable
        where TRegisterMap : Enum
    {
        private readonly bool _useLittleEndian;
        private I2cRegisterAccess _registerAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="I2cRegisterAccess{TRegisterMap}"/> class.
        /// </summary>
        /// <param name="device">I2C device to access.</param>
        /// <param name="useLittleEndian"><see langword="true"/> to use little-endian when communicating with the device; <see langword="false"/> to use big-endian.</param>
        public I2cRegisterAccess(I2cDevice device, bool useLittleEndian)
        {
            _registerAccess = new I2cRegisterAccess(device);
            _useLittleEndian = useLittleEndian;
        }

        /// <summary>
        /// Reads a numeric value from the provided register address.
        /// Uses the endianness defined when constructing the instance.
        /// </summary>
        /// <param name="register">Register address in the form an <see cref="Enum"/> value of type <see cref="byte"/>.</param>
        /// <typeparam name="T">Integral numeric type.</typeparam>
        public T ReadRegister<T>(TRegisterMap register)
            where T : struct
        {
            var registerMapUnderlyingType = Enum.GetUnderlyingType(typeof(TRegisterMap));
            if (registerMapUnderlyingType != typeof(byte))
            {
                throw new ArgumentException("Register address must be of type System.Byte");
            }

            return _registerAccess.ReadRegister<T>((byte)(object)register, _useLittleEndian);
        }

        // TODO: Remove when tested
        /*
        /// <summary>
        /// Writes a numeric value to the provided register address.
        /// </summary>
        /// <typeparam name="T">Integral numeric type.</typeparam>
        public void WriteRegister<T>(TRegisterMap register, T data)
            where T : struct
        {
            var bytesToWrite = new List<byte> { (byte)(object)register };

            int typeSizeInBytes = Marshal.SizeOf(default(T));
            for (int index = 0; index < typeSizeInBytes; index++)
            {
                byte valueToWrite = 0;
                var shiftBitsCount = index * 8;

                // Shift operations are only allowed on integral numeric types and not on generic types,
                // thus the need for this switch statement
                switch (data)
                {
                    case sbyte typedData:
                        valueToWrite = (byte)typedData;
                        break;
                    case byte typedData:
                        valueToWrite = typedData;
                        break;
                    case short typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    case ushort typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    case int typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    case uint typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    case long typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    case ulong typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    case char typedData:
                        valueToWrite = index == 0 ? (byte)(typedData & 0xFF) : (byte)(typedData >> shiftBitsCount);
                        break;
                    default:
                        throw new InvalidOperationException($"Type '{data.GetType()}' is not an integral numeric type.");
                }

                bytesToWrite.Add(valueToWrite);
            }

            // _device.Write();
        }
        */

        /// <summary>
        /// Writes a numeric value to the provided register address.
        /// Uses the endianness defined when constructing the instance.
        /// </summary>
        /// <typeparam name="T">Integral numeric type.</typeparam>
        /// <param name="register">Register address in the form an <see cref="Enum"/> value of type <see cref="byte"/>.</param>
        /// <param name="data">Numeric value to write to the register.</param>
        public void WriteRegister<T>(TRegisterMap register, T data)
            where T : struct
        {
            var registerMapUnderlyingType = Enum.GetUnderlyingType(typeof(TRegisterMap));
            if (registerMapUnderlyingType != typeof(byte))
            {
                throw new ArgumentException("Register address must be of type System.Byte");
            }

            _registerAccess.WriteRegister((byte)(object)register, data, _useLittleEndian);
        }

        // TODO: Remove when tested
        /*
        private Span<byte> Write<T>(T data, bool useLittleEndian)
            where T : struct
        {
            int typeSizeInBytes = Marshal.SizeOf(default(T));
            Span<byte> outArray = stackalloc byte[typeSizeInBytes];

            switch (data)
            {
                case sbyte typedData:
                    outArray[0] = (byte)typedData;
                    break;
                case byte typedData:
                    outArray[0] = typedData;
                    break;
                case short typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteInt16LittleEndian(outArray, typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt16BigEndian(outArray, typedData);
                    }

                    break;
                case ushort typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(outArray, typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt16BigEndian(outArray, typedData);
                    }

                    break;
                case int typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(outArray, typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt32BigEndian(outArray, typedData);
                    }

                    break;
                case uint typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(outArray, typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt32BigEndian(outArray, typedData);
                    }

                    break;
                case long typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteInt64LittleEndian(outArray, typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt64BigEndian(outArray, typedData);
                    }

                    break;
                case ulong typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteUInt64LittleEndian(outArray, typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt64BigEndian(outArray, typedData);
                    }

                    break;

                default:
                    throw new InvalidOperationException($"Type '{data.GetType()}' is not a supported integral numeric type.");
            }

            return outArray;
        }
        */

        /// <inheritdoc />
        public void Dispose()
        {
            _registerAccess?.Dispose();
            _registerAccess = null;
        }
    }
}
