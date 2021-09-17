// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Scd4x
{
    /// <summary>
    /// CO₂, Humidity, and Temperature sensor SCD4x
    /// </summary>
    public sealed class Scd4x : IDisposable
    {
        /// <summary>
        /// The default I²C address of this device.
        /// </summary>
        public const int DefaultI2cAddress = 0x62;

        /// <summary>
        /// The period to wait for a measurement.
        /// </summary>
        public static TimeSpan MeasurementPeriod => TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 5);

        private static ReadOnlySpan<byte> StartPeriodicMeasurementBytes => new byte[] { 0x21, 0xB1 };
        private static ReadOnlySpan<byte> CheckDataReadyStatusBytes => new byte[] { 0xE4, 0xB8 };
        private static ReadOnlySpan<byte> ReadPeriodicMeasurementBytes => new byte[] { 0xEC, 0x05 };
        private static ReadOnlySpan<byte> StopPeriodicMeasurementBytes => new byte[] { 0x3F, 0x86 };
        private static ReadOnlySpan<byte> ReInitBytes => new byte[] { 0x36, 0x46 };

        private readonly I2cDevice _device;

        /// <summary>
        /// Instantiates a new <see cref="Scd4x"/>.
        /// </summary>
        /// <param name="device">The I²C device to operate on.</param>
        public Scd4x(I2cDevice device)
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
            StopPeriodicMeasurements();

            _device.Write(ReInitBytes);
            Thread.Sleep(20);
        }

        /// <summary>
        /// Calibrates the sensor to operate at a specific barometric pressure.
        /// Doing so will make measurements more accurate.
        /// </summary>
        /// <param name="pressure">The pressure to use when calibrating the sensor.</param>
        public void SetPressureCalibration(Pressure pressure)
        {
            TimeSpan delay = BeginSetPressureCalibration(pressure);
            Thread.Sleep(delay);
        }

        /// <summary>
        /// Begins calibrating the sensor to operate at a specific barometric pressure.
        /// Doing so will make measurements more accurate.
        /// </summary>
        /// <param name="pressure">The pressure to use when calibrating the sensor.</param>
        /// <returns>
        /// The time that the caller must wait before performing any further operations on the device.
        /// </returns>
        public TimeSpan BeginSetPressureCalibration(Pressure pressure)
        {
            Span<byte> buffer = stackalloc byte[5];

            BinaryPrimitives.WriteUInt16BigEndian(buffer, 0xE000);
            Sensirion.WriteUInt16BigEndianAndCRC8(buffer.Slice(2), (ushort)(Math.Max(0.0, Math.Min(pressure.Pascals, 1.0)) * (1.0 / 100.0)));

            _device.Write(buffer);
            return TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// <para>
        /// Instructs the sensor to start performing periodic measurements.
        /// </para>
        ///
        /// <para>
        /// Every period, the length of which is available in <see cref="MeasurementPeriod"/>, <see cref="CheckDataReady"/> can be called to see if a measurement is available,
        /// and the measurement can then be read via <see cref="ReadPeriodicMeasurement"/>.
        /// </para>
        ///
        /// <para>
        /// <see cref="StopPeriodicMeasurements"/> must be called to stop periodic measurements.
        /// </para>
        /// </summary>
        public void StartPeriodicMeasurements() =>
            _device.Write(StartPeriodicMeasurementBytes);

        /// <summary>
        /// <para>
        /// Checks if a periodic measurement is available.
        /// Once available, the measurement can be read via <see cref="ReadPeriodicMeasurement"/>.
        /// </para>
        ///
        /// <para>
        /// <see cref="StartPeriodicMeasurements"/> must be called first.
        /// </para>
        /// </summary>
        /// <returns>If a measurement is available, <see langword="true"/>. Otherwise, <see langword="false"/>.</returns>
        public bool CheckDataReady()
        {
            TimeSpan delay = BeginCheckDataReady();
            Thread.Sleep(delay);
            return EndCheckDataReady();
        }

        /// <summary>
        /// <para>
        /// Begins checking if a periodic measurement is available.
        /// Once available, the measurement can be read via <see cref="ReadPeriodicMeasurement"/>.
        /// </para>
        ///
        /// <para>
        /// <see cref="StartPeriodicMeasurements"/> must be called first.
        /// </para>
        /// </summary>
        /// <returns>
        /// The time that the caller must wait before calling <see cref="EndCheckDataReady"/> to retreive the current data ready status.
        /// </returns>
        public TimeSpan BeginCheckDataReady()
        {
            _device.Write(CheckDataReadyStatusBytes);
            return TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// Completes checking if a periodic measurement is available.
        /// <see cref="BeginCheckDataReady"/> must have been called first.
        /// </summary>
        /// <returns>If a measurement is available, <see langword="true"/>. Otherwise, <see langword="false"/>.</returns>
        public bool EndCheckDataReady()
        {
            Span<byte> buffer = stackalloc byte[3];
            _device.Read(buffer);

            return Sensirion.ReadUInt16BigEndianAndCRC8(buffer) is ushort response && (response & 0x7FF) != 0;
        }

        /// <summary>
        /// <para>
        /// Reads a periodic CO₂, humidity, and temperature measurement from the sensor.
        /// </para>
        ///
        /// <para>
        /// <see cref="CheckDataReady"/> should be called first to ensure a measurement is available. Once read, data will no longer be ready.
        /// </para>
        /// </summary>
        /// <returns>A tuple of CO₂, humidity, and temperature.</returns>
        public (VolumeConcentration? CarbonDioxide, RelativeHumidity? RelativeHumidity, Temperature? Temperature) ReadPeriodicMeasurement()
        {
            TimeSpan delay = BeginReadPeriodicMeasurement();
            Thread.Sleep(delay);
            return EndReadPeriodicMeasurement();
        }

        /// <summary>
        /// <para>
        /// Begins reading a periodic CO₂, humidity, and temperature measurement from the sensor.
        /// </para>
        ///
        /// <para>
        /// <see cref="CheckDataReady"/> should be called first to ensure a measurement is available. Once read, data will no longer be ready.
        /// </para>
        /// </summary>
        /// <returns>
        /// The time that the caller must wait before calling <see cref="EndReadPeriodicMeasurement"/> to retreive the measurement.
        /// </returns>
        public TimeSpan BeginReadPeriodicMeasurement()
        {
            _device.Write(ReadPeriodicMeasurementBytes);
            return TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 2);
        }

        /// <summary>
        /// <para>
        /// Completes reading a periodic CO₂, humidity, and temperature measurement from the sensor.
        /// </para>
        ///
        /// <para>
        /// <see cref="BeginReadPeriodicMeasurement"/> must have been called first.
        /// </para>
        /// </summary>
        /// <returns>
        /// A tuple of CO₂, relative humidity, and temperature.
        /// If a CRC check failed for a measurement, it will be <see langword="null"/>.
        /// </returns>
        public (VolumeConcentration? CarbonDioxide, RelativeHumidity? RelativeHumidity, Temperature? Temperature) EndReadPeriodicMeasurement()
        {
            Span<byte> buffer = stackalloc byte[9];
            _device.Read(buffer);

            VolumeConcentration? co2 = Sensirion.ReadUInt16BigEndianAndCRC8(buffer) switch
            {
                ushort sco2 => VolumeConcentration.FromPartsPerMillion(sco2),
                null => (VolumeConcentration?)null
            };

            Temperature? temp = Sensirion.ReadUInt16BigEndianAndCRC8(buffer.Slice(3, 3)) switch
            {
                ushort st => Temperature.FromDegreesCelsius(st * (35.0 / 13107.0) - 45.0),
                null => (Temperature?)null
            };

            RelativeHumidity? humidity = Sensirion.ReadUInt16BigEndianAndCRC8(buffer.Slice(6, 3)) switch
            {
                ushort srh => RelativeHumidity.FromPercent(srh * (100.0 / 65535.0)),
                null => (RelativeHumidity?)null
            };

            return (co2, humidity, temp);
        }

        /// <summary>
        /// Instructs the sensor to stop performing periodic measurements.
        /// </summary>
        public void StopPeriodicMeasurements()
        {
            _device.Write(StopPeriodicMeasurementBytes);
            Thread.Sleep(500);
        }
    }
}
