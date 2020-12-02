// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Adxl357
{
    /// <summary>
    /// I2C Accelerometer ADXL357
    /// </summary>
    public class Adxl357 : IDisposable
    {
        /// <summary>
        /// The default I2C address of ADXL357 device
        /// </summary>
        public const byte DefaultI2CAddress = 0x1d;

        // Default values taken from sample code https://wiki.seeedstudio.com/Grove-3-Axis_Digital_Accelerometer_40g-ADXL357/
        private const int CalibrationBufferLengthDefault = 15;
        private const int CalibrationIntervalDefault = 250;

        private float _factory = 1;
        private I2cDevice _i2CDevice;

        /// <summary>
        /// Constructs a ADXL357 I2C device.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication.</param>
        /// <param name="accelerometerRange">The sensitivity of the accelerometer.</param>
        public Adxl357(I2cDevice i2CDevice, AccelerometerRange accelerometerRange = AccelerometerRange.Range10G)
        {
            _i2CDevice = i2CDevice ?? throw new ArgumentNullException(nameof(i2CDevice));
            Reset();

            AccelerometerRange = accelerometerRange;
            PowerOn();
        }

        /// <summary>
        /// Gets the current acceleration in g.
        /// Range depends on the <see cref="Device.Adxl357.AccelerometerRange"/> passed to the constructor.
        /// </summary>
        public Vector3 Acceleration => GetRawAccelerometer();

        /// <summary>
        /// Gets the current temperature in °C.
        /// Range is from −40°C to +125°C.
        /// </summary>
        public Temperature Temperature => Temperature.FromDegreesCelsius(GetTemperature());

        /// <summary>
        /// Calibrates the accelerometer.
        /// You can override default <paramref name="calibrationBufferLength"/> and <paramref name="calibrationInterval"/> if required.
        /// </summary>
        /// <param name="calibrationBufferLength">The number of times every axis is measured. The average of these measurements is used to calibrate each axis.</param>
        /// <param name="calibrationInterval">The time in milliseconds to wait between each measurement.</param>
        /// <remarks>
        /// Make sure that the sensor is placed horizontally when executing this method.
        /// </remarks>
        public async Task CalibrateAccelerationSensor(int calibrationBufferLength = CalibrationBufferLengthDefault, int calibrationInterval = CalibrationIntervalDefault)
        {
            var caliBuffer = new Vector3[calibrationBufferLength];

            for (int i = 0; i < calibrationBufferLength; i++)
            {
                var acc = GetRawAccelerometer();
                caliBuffer[i].X = acc.X;
                caliBuffer[i].Y = acc.Y;
                caliBuffer[i].Z = acc.Z;

                await Task.Delay(calibrationInterval).ConfigureAwait(false);
            }

            var avgX = caliBuffer.Select(v => v.X).Average();
            var avgY = caliBuffer.Select(v => v.Y).Average();
            var avgZ = caliBuffer.Select(v => v.Z).Average();

            var x = (((avgZ - avgX) + (avgZ - avgY)) / 2);
            x = x == 0 ? float.PositiveInfinity : x;
            _factory = 1.0F / x;
        }

        /// <summary>
        /// Gets or sets the sensitivity of the accelerometer.
        /// </summary>
        public AccelerometerRange AccelerometerRange
        {
            get => (AccelerometerRange)ReadByte(Register.SET_RANGE_REG_ADDR);
            set
            {
                var currentValue = ReadByte(Register.SET_RANGE_REG_ADDR);
                var newValue = currentValue | (byte)value;
                WriteRegister(Register.SET_RANGE_REG_ADDR, (byte)newValue);
            }
        }

        private double GetTemperature()
        {
            Span<byte> data = stackalloc byte[2] { 0, 0 };

            ReadBytes(Register.TEMPERATURE_REG_ADDR, data);

            double value = BinaryPrimitives.ReadUInt16BigEndian(data);

            return 25 + (value - 1852) / -9.05;
        }

        private void Reset()
        {
            WriteRegister(Register.RESET_REG_ADDR, 0x52);
            Thread.Sleep(100);
        }

        private void PowerOn()
        {
            WriteRegister(Register.POWER_CTR_REG_ADDR, 0x00);
            Thread.Sleep(100);
        }

        private Vector3 GetRawAccelerometer()
        {
            Span<byte> data = stackalloc byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            var ace = new Vector3();

            if (CheckDataReady())
            {
                ReadBytes(Register.FIFO_DATA_REG_ADDR, data);

                ace.X = GetValueForOneAxis(data[0], data[1], data[2]);
                ace.Y = GetValueForOneAxis(data[3], data[4], data[5]);
                ace.Z = GetValueForOneAxis(data[6], data[7], data[8]);
            }

            return ace;
        }

        private float GetValueForOneAxis(byte firstByte, byte secondByte, byte thirdByte)
        {
            uint value = ((uint)firstByte << 12) | ((uint)secondByte << 4) | ((uint)thirdByte >> 4);

            if ((value & 0x80000) == 0x80000)
            {
                value = (value & 0x7ffff) - 0x80000;
            }

            return value * _factory;
        }

        private bool CheckDataReady()
        {
            var status = GetAdxl357Status();
            return (status & 0x01) == 0x01;
        }

        private byte GetAdxl357Status() => ReadByte(Register.STATUS_REG_ADDR);

        private void WriteRegister(Register register, byte data)
        {
            Span<byte> dataout = stackalloc byte[]
            {
                (byte)register, data
            };

            _i2CDevice.Write(dataout);
        }

        private byte ReadByte(Register register)
        {
            _i2CDevice.WriteByte((byte)register);
            return _i2CDevice.ReadByte();
        }

        private void ReadBytes(Register register, Span<byte> readBytes)
        {
            _i2CDevice.WriteByte((byte)register);
            _i2CDevice.Read(readBytes);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _i2CDevice.Dispose();
            _i2CDevice = null!;
        }
    }
}
