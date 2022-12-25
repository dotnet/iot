// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Am2320
{
    /// <summary>
    /// AM2320 - Temperature and Humidity sensor.
    /// </summary>
    [Interface("AM2320 - Temperature and Humidity sensor")]
    public class Am2320 : IDisposable
    {
        private readonly I2cDevice _i2c;
        private readonly byte[] _readBuff = new byte[8];

        /// <summary>
        /// AM3220 default I2C address.
        /// </summary>
        public const int DefaultI2cAddress = 0x5C;

        /// <summary>
        /// The minimum read period is 1.5 seconds. Do not read the sensor more often.
        /// </summary>
        public static readonly TimeSpan MinimumReadPeriod = new TimeSpan(0, 0, 0, 1, 500);

        private DateTime _lastMeasurement = DateTime.UtcNow.Subtract(MinimumReadPeriod);

        /// <summary>
        /// Gets a value indicating whether last read went, <c>true</c> for success, <c>false</c> for failure.
        /// </summary>
        public bool IsLastReadSuccessful { get; internal set; }

        /// <summary>
        /// Gets the last read temperature.
        /// </summary>
        /// <param name="temperature">[Out] The current temperature on success.</param>
        /// <returns>True on success, false if reading failed.</returns>
        [Telemetry("Temperature")]
        public bool TryReadTemperature(
                out Temperature temperature)
        {
            temperature = default;
            if (IsOutDated())
            {
                ReadData();
            }

            if (IsLastReadSuccessful)
            {
                temperature = GetTemperature();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the last read of relative humidity in percentage.
        /// </summary>
        /// <param name="humidity">[Out] The current relative humidity on success.</param>
        /// <returns>True on success, false if reading failed.</returns>
        [Telemetry("Humidity")]
        public bool TryReadHumidity(
                    out RelativeHumidity humidity)
        {
            humidity = default;
            if (IsOutDated())
            {
                ReadData();
            }

            if (IsLastReadSuccessful)
            {
                humidity = GetHumidity();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the device information.
        /// </summary>
        /// <param name="deviceInformation">[Out] The Device Information on success.</param>
        /// <returns>True on success, false if reading failed.</returns>
        [Property("DeviceInformation")]
        public bool TryGetDeviceInformation(out DeviceInformation? deviceInformation)
        {
            deviceInformation = null;
            Span<byte> buff = stackalloc byte[11];

            // 32 bit device ID
            // Forces the sensor to wake up and wait 10 ms according to documentation
            _i2c.WriteByte(0x00);
            Thread.Sleep(10);

            // Sending functin code 0x03, start regster 0x08 and 7 registers
            _i2c.Write(stackalloc byte[] { 0x03, 0x08, 0x07 });

            // Wait at least 30 micro seconds
            Thread.Sleep(1);
            _i2c.Read(buff);

            // Check if it is valid
            if (!IsValidReadBuffer(buff, 0x07))
            {
                return false;
            }

            if (!IsCrcValid(buff))
            {
                return false;
            }

            deviceInformation = new DeviceInformation()
            {
                Model = BinaryPrimitives.ReadUInt16BigEndian(buff.Slice(2)),
                Version = buff[4],
                DeviceId = BinaryPrimitives.ReadUInt32BigEndian(buff.Slice(5)),
            };

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Am2320" /> class.
        /// </summary>
        /// <param name="i2c">The <see cref="I2cDevice"/>.</param>
        /// <remarks>This sensor only works on Standard Mode speed.</remarks>
        public Am2320(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException(nameof(i2c));
        }

        private bool IsOutDated() => !(_lastMeasurement.Add(MinimumReadPeriod) > DateTime.UtcNow);

        private void ReadData()
        {
            // Forces the sensor to wake up and wait 10 ms according to documentation
            _i2c.WriteByte(0x00);
            Thread.Sleep(10);

            // Sending functin code 0x03, start regster 0x00 and 4 registers
            _i2c.Write(stackalloc byte[] { 0x03, 0x00, 0x04 });

            // Wait at least 30 micro seconds
            Thread.Sleep(1);
            _i2c.Read(_readBuff);
            _lastMeasurement = DateTime.UtcNow;

            // Check if sucessfull read
            if (!IsValidReadBuffer(_readBuff, 0x04))
            {
                IsLastReadSuccessful = false;
                return;
            }

            if (!IsCrcValid(_readBuff))
            {
                IsLastReadSuccessful = false;
                return;
            }

            IsLastReadSuccessful = true;
        }

        private bool IsValidReadBuffer(Span<byte> buff, byte expected) => (buff[0] == 0x03) && (buff[1] == expected);

        private bool IsCrcValid(Span<byte> buff)
        {
            var crc = Crc16(buff.Slice(0, buff.Length - 2));
            return (crc >> 8 == buff[buff.Length - 1]) && ((crc & 0xFF) == buff[buff.Length - 2]);
        }

        private Temperature GetTemperature()
        {
            short temp = BinaryPrimitives.ReadInt16BigEndian((new Span<byte>(_readBuff)).Slice(4));
            return Temperature.FromDegreesCelsius(temp / 10.0);
        }

        private RelativeHumidity GetHumidity()
        {
            short hum = BinaryPrimitives.ReadInt16BigEndian((new Span<byte>(_readBuff)).Slice(2));
            return RelativeHumidity.FromPercent(hum / 10.0);
        }

        private ushort Crc16(Span<byte> ptr)
        {
            ushort crc = 0xFFFF;
            byte i;
            byte inc = 0;
            while (inc < ptr.Length)
            {
                crc ^= ptr[inc++];
                for (i = 0; i < 8; i++)
                {
                    if ((crc & 0x01) == 0x01)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
        }
    }
}
