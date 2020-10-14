// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// Implements low-level access to read and write to the provided I2C device.
    /// </summary>
    public sealed class I2cRegisterAccess : IDisposable
    {
        private I2cDevice _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="I2cRegisterAccess"/> class.
        /// </summary>
        /// <param name="device">I2C device to access.</param>
        public I2cRegisterAccess(I2cDevice device)
        {
            _device = device;
        }

        /// <summary>
        /// Reads a numeric value from the provided register address.
        /// </summary>
        /// <typeparam name="T">Integral numeric type.</typeparam>
        /// <param name="registerAddress">Register address.</param>
        /// <param name="useLittleEndian"><see langword="true"/> to use little-endian when reading from the register; <see langword="false"/> to use big-endian.</param>
        public T ReadRegister<T>(byte registerAddress, bool useLittleEndian)
            where T : struct
        {
            int typeSizeInBytes = Marshal.SizeOf(default(T));
            Span<byte> writeBuffer = stackalloc byte[]
            {
                registerAddress
            };
            Span<byte> readBuffer = stackalloc byte[typeSizeInBytes];
            _device.WriteRead(writeBuffer, readBuffer);

            switch (default(T))
            {
                case byte byteType:
                    return MemoryMarshal.Read<T>(readBuffer);
                case short shortType:
                    return useLittleEndian
                        ? (T)(object)BinaryPrimitives.ReadInt16LittleEndian(readBuffer)
                        : (T)(object)BinaryPrimitives.ReadInt16BigEndian(readBuffer);
                case ushort ushortType:
                    return useLittleEndian
                        ? (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(readBuffer)
                        : (T)(object)BinaryPrimitives.ReadUInt16BigEndian(readBuffer);
                case int intType:
                    return useLittleEndian
                        ? (T)(object)BinaryPrimitives.ReadInt32LittleEndian(readBuffer)
                        : (T)(object)BinaryPrimitives.ReadInt32BigEndian(readBuffer);
                case uint uintType:
                    return useLittleEndian
                        ? (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(readBuffer)
                        : (T)(object)BinaryPrimitives.ReadUInt32BigEndian(readBuffer);
                case long longType:
                    return useLittleEndian
                        ? (T)(object)BinaryPrimitives.ReadInt64LittleEndian(readBuffer)
                        : (T)(object)BinaryPrimitives.ReadInt64BigEndian(readBuffer);
                case ulong ulongType:
                    return useLittleEndian
                        ? (T)(object)BinaryPrimitives.ReadUInt64LittleEndian(readBuffer)
                        : (T)(object)BinaryPrimitives.ReadUInt64BigEndian(readBuffer);
                default:
                    throw new InvalidOperationException($"Type '{default(T).GetType()}' is not a supported integral numeric type.");
            }
        }

        /// <summary>
        /// Writes a numeric value to the provided register address.
        /// </summary>
        /// <typeparam name="T">Integral numeric type.</typeparam>
        /// <param name="registerAddress">Register address.</param>
        /// <param name="data">Numeric value to write to the register.</param>
        /// <param name="useLittleEndian"><see langword="true"/> to use little-endian when writing to the register; <see langword="false"/> to use big-endian.</param>
        public void WriteRegister<T>(byte registerAddress, T data, bool useLittleEndian)
            where T : struct
        {
            int typeSizeInBytes = Marshal.SizeOf(default(T));
            Span<byte> outArray = stackalloc byte[typeSizeInBytes + 1]; // Allocate enough space for the register address + data
            outArray[0] = registerAddress;

            switch (data)
            {
                case sbyte typedData:
                    outArray[1] = (byte)typedData;
                    break;
                case byte typedData:
                    outArray[1] = typedData;
                    break;
                case short typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt16BigEndian(outArray.Slice(1), typedData);
                    }

                    break;
                case ushort typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(outArray.Slice(1), typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt16BigEndian(outArray.Slice(1), typedData);
                    }

                    break;
                case int typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(outArray.Slice(1), typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt32BigEndian(outArray.Slice(1), typedData);
                    }

                    break;
                case uint typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(outArray.Slice(1), typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt32BigEndian(outArray.Slice(1), typedData);
                    }

                    break;
                case long typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteInt64LittleEndian(outArray.Slice(1), typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt64BigEndian(outArray.Slice(1), typedData);
                    }

                    break;
                case ulong typedData:
                    if (useLittleEndian)
                    {
                        BinaryPrimitives.WriteUInt64LittleEndian(outArray.Slice(1), typedData);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt64BigEndian(outArray.Slice(1), typedData);
                    }

                    break;

                default:
                    throw new InvalidOperationException($"Type '{data.GetType()}' is not a supported integral numeric type.");
            }

            _device.Write(outArray);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }
    }
}
