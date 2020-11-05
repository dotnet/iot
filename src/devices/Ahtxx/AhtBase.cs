// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Ahtxx
{
    /// <summary>
    ///  Base class for common functions of the AHT10/15 and AHT20 sensors.
    /// </summary>
    public abstract class AhtBase : IDisposable
    {
        /// <summary>
        /// Address of AHT10/15/20 device (0x38). This address is fix and cannot be changed.
        /// This implies that only one device can be attached to a single I2C bus at a time.
        /// </summary>
        public const int DefaultI2cAddress = 0x38;

        private readonly byte _initCommand;
        private I2cDevice _i2cDevice;
        private double _temperature;
        private double _humidity;

        /// <summary>
        /// Initializes a new instance of the binding for a sensor connected through I2C interface.
        /// </summary>
        /// <paramref name="i2cDevice">Reference to the initialized I2C interface device</paramref>
        /// <paramref name="initCommand">Type specific command for device initialization</paramref>
        public AhtBase(I2cDevice i2cDevice, byte initCommand)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _initCommand = initCommand;

            // even if not clearly stated in datasheet, start with a software reset to assure clear start conditions
            SoftReset();

            // check whether the device indicates the need for a calibration cycle
            // and perform calibration if indicated ==> c.f. datasheet, version 1.1, ch. 5.4
            if (!IsCalibrated())
            {
                Initialize();
            }
        }

        /// <summary>
        /// Gets the current temperature reading from the sensor.
        /// Reading the temperature takes between 10 ms and 80 ms.
        /// </summary>
        /// <returns>Temperature reading</returns>
        public Temperature GetTemperature()
        {
            Measure();
            return Temperature.FromDegreesCelsius(_temperature);
        }

        /// <summary>
        /// Gets the current relative humidity reading from the sensor.
        /// Reading the humidity takes between 10 ms and 80 ms.
        /// </summary>
        /// <returns>Relative humidity reading</returns>
        public Ratio GetHumidity()
        {
            Measure();
            return Ratio.FromPercent(_humidity);
        }

        /// <summary>
        /// Perform sequence to retrieve current readings from device
        /// </summary>
        private void Measure()
        {
            Span<byte> buffer = stackalloc byte[3]
            {
                // command parameters c.f. datasheet, version 1.1, ch. 5.4
                (byte)CommonCommand.Measure,
                0x33,
                0x00
            };

            _i2cDevice.Write(buffer);

            // According to the datasheet the measurement takes 80 ms and completion is indicated by the status bit.
            // However, it seems to be faster at around 10 ms and sometimes up to 50 ms.
            while (IsBusy())
            {
                Thread.Sleep(10);
            }

            buffer = stackalloc byte[6];
            _i2cDevice.Read(buffer);

            // data format: 20 bit humidity, 20 bit temperature
            // 7               0 7              0 7             4           0 7          0 7         0
            // [humidity 19..12] [humidity 11..4] [humidity 3..0|temp 19..16] [temp 15..8] [temp 7..0]
            // c.f. datasheet ch. 5.4.5
            Int32 rawHumidity = (buffer[1] << 12) | (buffer[2] << 4) | (buffer[3] >> 4);
            Int32 rawTemperature = ((buffer[3] & 0xF) << 16) | (buffer[4] << 8) | buffer[5];
            // RH[%] = Hraw / 2^20 * 100%, c.f. datasheet ch. 6.1
            _humidity = (rawHumidity * 100.0) / 0x100000;
            // T[°C] = Traw / 2^20 * 200 - 50, c.f. datasheet ch. 6.1
            _temperature = ((rawTemperature * 200.0) / 0x100000) - 50;
        }

        /// <summary>
        /// Perform soft reset command sequence
        /// </summary>
        private void SoftReset()
        {
            _i2cDevice.WriteByte((byte)CommonCommand.SoftReset);
            // reset requires 20ms at most, c.f. datasheet version 1.1, ch. 5.5
            Thread.Sleep(20);
        }

        /// <summary>
        /// Perform initialization (calibration) command sequence
        /// </summary>
        private void Initialize()
        {
            Span<byte> buffer = stackalloc byte[3]
            {
                _initCommand,
                0x08, // command parameters c.f. datasheet, version 1.1, ch. 5.4
                0x00
            };

            _i2cDevice.Write(buffer);
            // wait 10ms c.f. datasheet, version 1.1, ch. 5.4
            Thread.Sleep(10);
        }

        private byte GetStatus()
        {
            _i2cDevice.WriteByte(0x71);
            // whithout this delay the reading the status fails often.
            Thread.Sleep(10);
            byte status = _i2cDevice.ReadByte();
            return status;
        }

        private bool IsBusy()
        {
            return (GetStatus() & (byte)StatusBit.Busy) == (byte)StatusBit.Busy;
        }

        private bool IsCalibrated()
        {
            return (GetStatus() & (byte)StatusBit.Calibrated) == (byte)StatusBit.Calibrated;
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose() => Dispose(true);

        /// <inheritdoc cref="IDisposable" />
        protected virtual void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        // datasheet version 1.1, table 10
        [Flags]
        private enum StatusBit : byte
        {
            Calibrated = 0b_0000_1000,
            Busy = 0b1000_0000
        }

        private enum CommonCommand : byte
        {
            SoftReset = 0b1011_1010,
            Measure = 0b1010_1100
        }
    }
}
