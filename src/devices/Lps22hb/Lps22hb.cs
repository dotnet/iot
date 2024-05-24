// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.IO;
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
        /// <param name="outputRate">Output data rate</param>
        /// <param name="lowPassFilter">Use internal low-pass filter</param>
        /// <param name="bdu">Block data update</param>
        public Lps22hb(I2cDevice i2cDevice, OutputRate outputRate = OutputRate.DataRate25Hz, LowPassFilter lowPassFilter = LowPassFilter.Odr9, BduMode bdu = BduMode.BlockDataUpdate)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _i2c = i2cDevice;

            if (!TryRead())
            {
                throw new IOException("Lps22hb not found");
            }

            if (outputRate == OutputRate.PowerDownMode)
            {
                throw new NotImplementedException("One shot mode is not supported");
            }

            ResetDevice();

            // Hight resolution with Normal mode = 0 p43
            byte resolution = Read(Register.RES_CONF);
            resolution &= 0b10;
            WriteByte(Register.RES_CONF, resolution);

            // 7 - 0 - must be set 0
            // 6-4 - output data rate - set by outputRate
            // 3 - Low-pass filter set by lowPassFilter
            // 2 - Low-pass configuration register - set by lowPassFilter
            // 1 - block data update - set by bdu
            // 0 - SPI mode - we don't care what value since we use I2c, leave at default (0)
            var control1 = (byte)(0b0000_0000 | (uint)outputRate << 4 | (uint)lowPassFilter << 2 | (uint)bdu << 1);
            WriteByte(Register.CTRL_REG1, control1);

            if (lowPassFilter != LowPassFilter.Disable)
            {
                Read(Register.LPFP_RES);
            }
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
        /// Reset the device
        /// </summary>
        public void ResetDevice()
        {
            WriteByte(Register.CTRL_REG2, 0b0100);
            int timeoutLoop = 10;
            while (timeoutLoop > 0)
            {
                byte reset = Read(Register.CTRL_REG2);

                if ((reset & 0x04) == 0)
                {
                    break;
                }

                DelayHelper.DelayMilliseconds(10, true);
                timeoutLoop -= 1;
            }
        }

        /// <summary>
        /// Read temperature. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="temperature">Contains the measured temperature on success.</param>
        /// <returns>True on success, false if reading failed.</returns>
        [Telemetry("Temperature")]
        public bool TryReadTemperature(out Temperature temperature)
        {
            try
            {
                temperature = Temperature.FromDegreesCelsius(ReadInt16(Register.TEMP_OUT_L) / 100f);
                return true;
            }
            catch
            {
                temperature = default;
                return false;
            }
        }

        /// <summary>
        /// Reads the pressure. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="pressure">
        /// Contains the measured pressure  on success.
        /// </param>
        /// <returns>True on success, false if reading failed.</returns>
        [Telemetry("Pressure")]
        public bool TryReadPressure(out Pressure pressure)
        {
            try
            {
                pressure = Pressure.FromHectopascals(ReadInt24(Register.PRESS_OUT_XL) / 4096.0);
                return true;
            }
            catch
            {
                pressure = default;
                return false;
            }
        }

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
