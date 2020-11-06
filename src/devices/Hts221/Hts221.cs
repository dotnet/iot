// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Hts221
{
    /// <summary>
    /// HTS221 - Capacitive digital sensor for relative humidity and temperature
    /// </summary>
    public class Hts221 : IDisposable
    {
        private const byte ReadMask = 0x80;
        private I2cDevice _i2c;

        /// <summary>
        /// Hts221 - Temperature and humidity sensor
        /// </summary>
        public Hts221(I2cDevice i2cDevice)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _i2c = i2cDevice;

            // Highest resolution for both temperature and humidity sensor:
            // 0.007 DegreesCelsius and 0.03 percentage of relative humidity respectively
            byte resolution = 0b0011_1111;
            WriteByte(Register.ResolutionMode, resolution);

            byte control1orig = Read(Register.Control1);
            // 7 - PD - power down control - 1 means active
            // 6-3 - reserved - keep original
            // 2 - BDU - block data update - 1 is recommended by datasheet and means that output registers
            //                               won't be updated until both LSB and MSB are read
            // 0-1 - output data rate - 11 means 12.5Hz
            byte control1 = (byte)(0b1000_0111 | (control1orig & 0b0111_1000));
            WriteByte(Register.Control1, control1);
        }

        /// <summary>
        /// Temperature
        /// </summary>
        public Temperature Temperature => Temperature.FromDegreesCelsius(GetActualTemperature(ReadInt16(Register.Temperature)));

        /// <summary>
        /// Humidity in %rH (percentage relative humidity)
        /// </summary>
        public Ratio Humidity => GetActualHumidity(ReadInt16(Register.Humidity));

        private void WriteByte(Register register, byte data)
        {
            Span<byte> buff = stackalloc byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private short ReadInt16(Register register)
        {
            Span<byte> val = stackalloc byte[2];
            Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
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

        private float GetActualTemperature(short temperatureRaw)
        {
            // datasheet does not specify if calibration points are not changing
            // since this is almost no-op and max output data rate is 12.5Hz let's do it every time
            (short t0raw, short t1raw) = GetTemperatureCalibrationPointsRaw();
            (ushort t0x8C, ushort t1x8C) = GetTemperatureCalibrationPointsCelsius();
            return Lerp(temperatureRaw, t0raw, t1raw, t0x8C / 8.0f, t1x8C / 8.0f);
        }

        private Ratio GetActualHumidity(short humidityRaw)
        {
            // datasheet does not specify if calibration points are not changing
            // since this is almost no-op and max output data rate is 12.5Hz let's do it every time
            (short h0raw, short h1raw) = GetHumidityCalibrationPointsRaw();
            (byte h0x2rH, byte h1x2rH) = GetHumidityCalibrationPointsRH();
            return Ratio.FromPercent(Lerp(humidityRaw, h0raw, h1raw, h0x2rH / 2.0f, h1x2rH / 2.0f));
        }

        private static float Lerp(float x, float x0, float x1, float y0, float y1)
        {
            float xrange = x1 - x0;
            float yrange = y1 - y0;
            return y0 + (x - x0) * yrange / xrange;
        }

        private (ushort t0x8, ushort t1x8) GetTemperatureCalibrationPointsCelsius()
        {
            Span<byte> t0t1Lsb = stackalloc byte[2];
            Read(Register.Temperature0LsbDegCx8, t0t1Lsb);
            byte t0t1Msb = Read(Register.Temperature0And1MsbDegCx8);

            ushort t0 = (ushort)(t0t1Lsb[0] | ((t0t1Msb & 0b11) << 8));
            ushort t1 = (ushort)(t0t1Lsb[1] | ((t0t1Msb & 0b1100) << 6));
            return (t0, t1);
        }

        private (short t0, short t1) GetTemperatureCalibrationPointsRaw()
        {
            Span<byte> t0t1 = stackalloc byte[4];
            Read(Register.Temperature0Raw, t0t1);
            short t0 = BinaryPrimitives.ReadInt16LittleEndian(t0t1.Slice(0, 2));
            short t1 = BinaryPrimitives.ReadInt16LittleEndian(t0t1.Slice(2, 2));
            return (t0, t1);
        }

        private (byte h0, byte h1) GetHumidityCalibrationPointsRH()
        {
            Span<byte> h0h1 = stackalloc byte[2];
            Read(Register.Humidity0rHx2, h0h1);
            return (h0h1[0], h0h1[1]);
        }

        private (short h0, short h1) GetHumidityCalibrationPointsRaw()
        {
            // space in addressing between both registers therefore do 2 reads
            short h0 = ReadInt16(Register.Humidity0Raw);
            short h1 = ReadInt16(Register.Humidity1Raw);
            return (h0, h1);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }
    }
}
