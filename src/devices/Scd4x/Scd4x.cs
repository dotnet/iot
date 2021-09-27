// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Scd4x
{
    /// <summary>
    /// CO₂, Humidity, and Temperature sensor SCD4x
    /// </summary>
    [Interface("CO₂, Humidity, and Temperature sensor SCD4x")]
    public sealed class Scd4x : IDisposable
    {
        private const int MeasurementPeriodMs = 5000;

        /// <summary>
        /// The default I²C address of this device.
        /// </summary>
        public const int DefaultI2cAddress = 0x62;

        /// <summary>
        /// The period to wait for each measurement: five seconds.
        /// </summary>
        public static TimeSpan MeasurementPeriod => TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * MeasurementPeriodMs);

        private static ReadOnlySpan<byte> StartPeriodicMeasurementBytes => new byte[] { 0x21, 0xB1 };
        private static ReadOnlySpan<byte> CheckDataReadyStatusBytes => new byte[] { 0xE4, 0xB8 };
        private static ReadOnlySpan<byte> ReadPeriodicMeasurementBytes => new byte[] { 0xEC, 0x05 };
        private static ReadOnlySpan<byte> StopPeriodicMeasurementBytes => new byte[] { 0x3F, 0x86 };
        private static ReadOnlySpan<byte> ReInitBytes => new byte[] { 0x36, 0x46 };

        private readonly I2cDevice _device;
        private VolumeConcentration _lastCo2;
        private RelativeHumidity _lastHum;
        private Temperature _lastTemp;
        private int _nextReadPeriod;
        private bool _started;

        /// <summary>
        /// The most recent CO₂ measurement.
        /// </summary>
        [Telemetry]
        public VolumeConcentration Co2
        {
            get
            {
                RefreshIfInNextPeriod();
                return _lastCo2;
            }
        }

        /// <summary>
        /// The most recent relative humidity measurement.
        /// </summary>
        [Telemetry]
        public RelativeHumidity RelativeHumidity
        {
            get
            {
                RefreshIfInNextPeriod();
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
                RefreshIfInNextPeriod();
                return _lastTemp;
            }
        }

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
        public void Dispose()
        {
            if (_started)
            {
                StopPeriodicMeasurements();
            }

            _device.Dispose();
        }

        private void RefreshIfInNextPeriod()
        {
            if (!_started || _nextReadPeriod - Environment.TickCount <= 0)
            {
                _ = ReadPeriodicMeasurement();
            }
        }

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
        /// </summary>
        /// <param name="pressure">The pressure to use when calibrating the sensor.</param>
        public void SetPressureCalibration(Pressure pressure)
        {
            int delay = SetPressureCalibrationImpl(pressure);
            Thread.Sleep(delay);
        }

        /// <inheritdoc cref="SetPressureCalibration(Pressure)"/>
        public Task SetPressureCalibrationAsync(Pressure pressure)
        {
            try
            {
                int delay = SetPressureCalibrationImpl(pressure);
                return Task.Delay(delay);
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        private int SetPressureCalibrationImpl(Pressure pressure)
        {
            Span<byte> buffer = stackalloc byte[5];

            BinaryPrimitives.WriteUInt16BigEndian(buffer, 0xE000);
            Sensirion.WriteUInt16BigEndianAndCRC8(buffer.Slice(2), (ushort)(Math.Max(0.0, Math.Min(pressure.Pascals, 1.0)) * (1.0 / 100.0)));

            _device.Write(buffer);
            return 1;
        }

        /// <summary>
        /// <para>
        /// Instructs the sensor to start performing periodic measurements.
        /// </para>
        ///
        /// <para>
        /// Every period, the length of which is available in <see cref="MeasurementPeriod"/>, the measurement can then be read via <see cref="ReadPeriodicMeasurement"/>.
        /// </para>
        ///
        /// <para>
        /// Periodic measurement can be stopped with <see cref="StopPeriodicMeasurements"/>.
        /// </para>
        /// </summary>
        public void StartPeriodicMeasurements()
        {
            _device.Write(StartPeriodicMeasurementBytes);
            _nextReadPeriod = Environment.TickCount + MeasurementPeriodMs;
            _started = true;
        }

        /// <summary>
        /// <para>
        /// Reads the next periodic CO₂, humidity, and temperature measurement from the sensor.
        /// </para>
        /// </summary>
        /// <returns>
        /// A tuple of CO₂, humidity, and temperature.
        /// If a CRC check failed for a measurement, it will be <see langword="null"/>.
        /// </returns>
        public (VolumeConcentration? CarbonDioxide, RelativeHumidity? RelativeHumidity, Temperature? Temperature) ReadPeriodicMeasurement()
        {
            var ret = ReadPeriodicMeasurementImpl(CancellationToken.None, async: false);

            Debug.Assert(ret.IsCompleted, "An async=false call should complete synchronously.");
            return ret.Result;
        }

        /// <inheritdoc cref="ReadPeriodicMeasurement"/>
        public ValueTask<(VolumeConcentration? CarbonDioxide, RelativeHumidity? RelativeHumidity, Temperature? Temperature)> ReadPeriodicMeasurementAsync(CancellationToken cancellationToken = default) =>
            ReadPeriodicMeasurementImpl(cancellationToken, async: true);

        private async ValueTask<(VolumeConcentration? CarbonDioxide, RelativeHumidity? RelativeHumidity, Temperature? Temperature)> ReadPeriodicMeasurementImpl(CancellationToken cancellationToken, bool async)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_started)
            {
                StartPeriodicMeasurements();
            }

            // Wait for the next period.
            int delay = _nextReadPeriod - Environment.TickCount;

            if (delay > 0)
            {
                await DelayAsync(Math.Min(delay, MeasurementPeriodMs), cancellationToken, async);
            }

            // Wait for the device to have a measurement.
            // When this loops, it is due to a small desync in device clocks.
            int waitStart = Environment.TickCount;

            while (true)
            {
                BeginCheckDataReady();
                await DelayAsync(1, CancellationToken.None, async);

                if (EndCheckDataReady())
                {
                    break;
                }

                if (Environment.TickCount - waitStart > (MeasurementPeriodMs + 1000))
                {
                    throw new Exception("SCD4x not responding.");
                }

                await DelayAsync(100, cancellationToken, async);
            }

            // Record the next period to expect data ready.
            _nextReadPeriod = Environment.TickCount + 5000;

            // Retrieve the measurements.
            BeginReadPeriodicMeasurement();
            await DelayAsync(2, CancellationToken.None, async);

            return EndReadPeriodicMeasurement();

            static Task DelayAsync(int delay, CancellationToken cancellationToken, bool async)
            {
                if (async)
                {
                    return Task.Delay(delay, cancellationToken);
                }

                Thread.Sleep(delay);
                return Task.CompletedTask;
            }
        }

        private void BeginCheckDataReady() =>
            _device.Write(CheckDataReadyStatusBytes);

        private bool EndCheckDataReady()
        {
            Span<byte> buffer = stackalloc byte[3];
            _device.Read(buffer);

            return Sensirion.ReadUInt16BigEndianAndCRC8(buffer) is ushort response && (response & 0x7FF) != 0;
        }

        private void BeginReadPeriodicMeasurement() =>
            _device.Write(ReadPeriodicMeasurementBytes);

        private (VolumeConcentration? CarbonDioxide, RelativeHumidity? RelativeHumidity, Temperature? Temperature) EndReadPeriodicMeasurement()
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

            if (co2 is not null)
            {
                _lastCo2 = co2.GetValueOrDefault();
            }

            if (temp is not null)
            {
                _lastTemp = temp.GetValueOrDefault();
            }

            if (humidity is not null)
            {
                _lastHum = humidity.GetValueOrDefault();
            }

            return (co2, humidity, temp);
        }

        /// <summary>
        /// Instructs the sensor to stop performing periodic measurements.
        /// </summary>
        public void StopPeriodicMeasurements()
        {
            _device.Write(StopPeriodicMeasurementBytes);
            _started = false;
            Thread.Sleep(500);
        }
    }
}
