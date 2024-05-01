// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.Lps22hb
{
    /// <summary>
    /// LPS22HB - Piezoresistive pressure and thermometer sensor
    /// </summary>
    [Interface("LPS22HB - Piezoresistive pressure and thermometer sensor")]
    public class Lps22hb : IDisposable
    {
        private const byte ReadMask = 0x80;
        private const byte Lps22hID = 0xB1;
        private I2cDevice _i2c;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int DefaultI2cAddress = 0x5C;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lps22hb"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Lps22hb(I2cDevice i2cDevice)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _i2c = i2cDevice;

            if (!TryRead())
            {
                throw new Exception("Device not found");
            }

            // Hight resolution with Normal mode = 0 p43
            byte resolution = Read(Register.RES_CONF);
            resolution &= 0b10;
            WriteByte(Register.RES_CONF, resolution);

            // 7 - 0 - must be set 0
            // 6-4 - output data rate - 0b011 means 25Hz for both sensors
            // 3 - Enable low-pass filter - 0 means disabled
            // 2 - Low-pass configuration register - 0 means disabled
            // 1 - block data update - 1 means update when both MSB and LSB are read
            // 0 - SPI mode - we don't care what value since we use I2c, leave at default (0)
            byte control1 = 0b0011_0010;
            WriteByte(Register.CTRL_REG1, control1);
        }

        private static uint ReadInt24LittleEndian(ReadOnlySpan<byte> buff)
        {
            Debug.Assert(buff.Length == 3, "Buffer must be 3 bytes long");

            byte mostSignificantByte = buff[2];
            Span<byte> b = stackalloc byte[4]
            {
                buff[0],
                buff[1],
                mostSignificantByte,
                (mostSignificantByte >> 7) != 0 ? (byte)0xff : (byte)0x00,
            };

            return BinaryPrimitives.ReadUInt32LittleEndian(b);
        }

        /// <summary>
        /// Temperature
        /// </summary>
        [Telemetry]
        public Temperature Temperature => Temperature.FromDegreesCelsius(ReadInt16(Register.TEMP_OUT_L) / 100f);

        /// <summary>
        /// Pressure
        /// </summary>
        [Telemetry]
        public Pressure Pressure => Pressure.FromHectopascals(ReadInt24(Register.PRESS_OUT_XL) / 4096.0);

        private uint ReadInt24(Register register)
        {
            Span<byte> val = stackalloc byte[3];
            Read(register, val);
            return ReadInt24LittleEndian(val);
        }

        private short ReadInt16(Register register)
        {
            Span<byte> val = stackalloc byte[2];
            Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        private void WriteByte(Register register, byte data)
        {
            Span<byte> buff = stackalloc byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private void Read(Register register, Span<byte> buffer)
        {
            _i2c.WriteByte((byte)((byte)register | ReadMask));
            _i2c.Read(buffer);
        }

        private byte Read(Register register)
        {
            _i2c.WriteByte((byte)((byte)register | ReadMask));
            return _i2c.ReadByte();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }

        /// <summary>
        /// TryRead
        /// </summary>
        /// <returns></returns>
        public bool TryRead()
        {
            byte resolution = Read(Register.WHO_AM_I);

            return resolution == Lps22hID;
        }
    }
}
