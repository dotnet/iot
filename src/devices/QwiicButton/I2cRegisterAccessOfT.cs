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
    /// Notice: This class only supports 8-bit register addresses.
    /// If the I2C device uses register addresses larger than 8-bit,
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

        /// <inheritdoc />
        public void Dispose()
        {
            _registerAccess?.Dispose();
            _registerAccess = null;
        }
    }
}
