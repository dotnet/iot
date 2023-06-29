// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Humidity and Temperature Sensor SHT4x
    /// </summary>
    [Interface("Humidity and Temperature Sensor SHT4x")]
    public sealed class Sht4x : IDisposable
    {
        /// <summary>
        /// The default I²C address of this device.
        /// </summary>
        public const int DefaultI2cAddress = 0x44;

        private readonly I2cDevice _device;
        private RelativeHumidity _lastHum;
        private Temperature _lastTemp;

        /// <summary>
        /// The level of repeatability to use when measuring relative humidity.
        /// </summary>
        [Property]
        public Sht4xRepeatability Repeatability { get; set; } = Sht4xRepeatability.High;

        /// <summary>
        /// The most recent relative humidity measurement.
        /// </summary>
        [Telemetry]
        public RelativeHumidity RelativeHumidity
        {
            get
            {
                ReadHumidityAndTemperature(Repeatability);
                return _lastHum;
            }
        }

        /// <summary>
        /// The most recent temperature measurement.
        /// </summary>
        [Telemetry]
        public Temperature Temperature
        {
            get
            {
                ReadHumidityAndTemperature(Repeatability);
                return _lastTemp;
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="Sht4x"/>.
        /// </summary>
        /// <param name="device">The I²C device to operate on.</param>
        public Sht4x(I2cDevice device)
        {
            _device = device;
            Reset();
        }

        /// <inheritdoc/>
        public void Dispose() =>
            _device.Dispose();

        /// <summary>
        /// Resets the device.
        /// </summary>
        public void Reset()
        {
            _device.WriteByte(0x94);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Reads the serial number of the device
        /// </summary>
        /// <returns>
        /// A 32 bit integer with the serial number.
        /// If a CRC check failed for a reading, it will be <see langword="null"/>.
        /// </returns>
        public int? ReadSerialNumber()
        {
            Span<byte> buffer = stackalloc byte[6];
            _device.WriteByte((byte)0x89);
            _device.Read(buffer);

            ushort? msb = Sensirion.ReadUInt16BigEndianAndCRC8(buffer);
            ushort? lsb = Sensirion.ReadUInt16BigEndianAndCRC8(buffer.Slice(3, 3));

            return lsb + (msb << 16);
        }

        /// <summary>
        /// Reads relative humidity and temperature.
        /// </summary>
        /// <returns>
        /// A tuple of relative humidity and temperature.
        /// If a CRC check failed for a measurement, it will be <see langword="null"/>.
        /// </returns>
        public (RelativeHumidity? RelativeHumidity, Temperature? Temperature) ReadHumidityAndTemperature(Sht4xRepeatability repeatability = Sht4xRepeatability.High)
        {
            int delay = BeginReadHumidityAndTemperature(repeatability);
            Thread.Sleep(delay);
            return EndReadHumidityAndTemperature();
        }

        /// <inheritdoc cref="ReadHumidityAndTemperature(Sht4xRepeatability)"/>
        public async ValueTask<(RelativeHumidity? RelativeHumidity, Temperature? Temperature)> ReadHumidityAndTemperatureAsync(Sht4xRepeatability repeatability = Sht4xRepeatability.High)
        {
            int delay = BeginReadHumidityAndTemperature(repeatability);
            await Task.Delay(delay);
            return EndReadHumidityAndTemperature();
        }

        private int BeginReadHumidityAndTemperature(Sht4xRepeatability repeatability)
        {
            (byte cmd, int delayInMs) = repeatability switch
            {
                Sht4xRepeatability.Low => ((byte)0xE0, 2),
                Sht4xRepeatability.Medium => ((byte)0xF6, 5),
                Sht4xRepeatability.High => ((byte)0xFD, 9),
                _ => throw new ArgumentOutOfRangeException(nameof(repeatability))
            };

            _device.WriteByte(cmd);
            return delayInMs;
        }

        private (RelativeHumidity? RelativeHumidity, Temperature? Temperature) EndReadHumidityAndTemperature()
        {
            Span<byte> buffer = stackalloc byte[6];
            _device.Read(buffer);

            Temperature? t = Sensirion.ReadUInt16BigEndianAndCRC8(buffer) switch
            {
                ushort deviceTemperature => Temperature.FromDegreesCelsius(deviceTemperature * (35.0 / 13107.0) - 45.0),
                null => (Temperature?)null
            };

            RelativeHumidity? h = Sensirion.ReadUInt16BigEndianAndCRC8(buffer.Slice(3, 3)) switch
            {
                ushort deviceHumidity => RelativeHumidity.FromPercent(Math.Max(0.0, Math.Min(deviceHumidity * (100.0 / 52428.0) - (300.0 / 50.0), 100.0))),
                null => (RelativeHumidity?)null
            };

            if (h is not null)
            {
                _lastHum = h.GetValueOrDefault();
            }

            if (t is not null)
            {
                _lastTemp = t.GetValueOrDefault();
            }

            return (h, t);
        }
    }
}
