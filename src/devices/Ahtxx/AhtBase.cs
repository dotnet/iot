// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Ahtxx
{
    /// <summary>
    /// AHT temperature and humidity sensor family.
    /// It has been tested with AHT20, but should work with AHT10 and AHT15 as well.
    /// Up to now all functions are contained in the base class as I'm not aware of differences
    /// between the sensors.
    /// </summary>
    public class AhtBase : IDisposable
    {
        /// <summary>
        /// Address of AHTxx device (0x38). This address is fix and cannot be changed.
        /// This implies that only one device can be attached to a single I2C bus at a time.
        /// </summary>
        public const int DeviceAddress = 0x38;

        private enum StatusBit : byte // datasheet version 1.1, table 10
        {
            Calibrated = 0x08,
            Busy = 0x80
        }

        private enum Command : byte
        {
            Calibrate = 0xbe,
            SoftRest = 0xba,
            Measure = 0xac
        }

        private I2cDevice _i2cDevice = null;
        private double _temperature;
        private double _humidity;

        /// <summary>
        /// Initializes a new instance of the device connected through I2C interface.
        /// </summary>
        /// <paramref name="i2cDevice">Reference to the initialized I2C interface device</paramref>
        public AhtBase(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            SoftReset(); // even if not clearly stated in datasheet, start with a software reset to assure clear start conditions

            // check whether the device indicates the need for a calibration cycle
            // and perform calibration if indicated ==> c.f. datasheet, version 1.1, ch. 5.4
            if (!IsCalibrated())
            {
                Calibrate();
            }
        }

        /// <summary>
        /// Gets the current temperature reading from the sensor.
        /// </summary>
        /// <returns>Temperature reading</returns>
        public Temperature Temperature
        {
            get
            {
                Measure();
                return new Temperature(_temperature, UnitsNet.Units.TemperatureUnit.DegreeCelsius);
            }
        }

        /// <summary>
        /// Gets the current humidity reading from the sensor.
        /// </summary>
        /// <returns>Temperature reading</returns>
        public Ratio Humidity
        {
            get
            {
                Measure();
                return new Ratio(_humidity, UnitsNet.Units.RatioUnit.Percent);
            }
        }

        /// <summary>
        /// Perform sequence to retrieve current readings from device
        /// </summary>
        private void Measure()
        {
            Span<byte> buffer = stackalloc byte[3]
            {
                (byte)Command.Measure,
                0x33, // command parameters c.f. datasheet, version 1.1, ch. 5.4
                0x00
            };
            _i2cDevice.Write(buffer);

            while (IsBusy())
            {
                Thread.Sleep(10);
            }

            buffer = stackalloc byte[6];
            _i2cDevice.Read(buffer);

            // Int32 rawHumidity;
            // rawHumidity = buffer[1] << 8;
            // rawHumidity |= buffer[2];
            // rawHumidity <<= 4;
            // rawHumidity |= buffer[3] >> 4;
            // _humidity = ((double)rawHumidity * 100) / 0x100000;

            // Int32 rawTemperature = buffer[3] & 0x0F;
            // rawTemperature <<= 8;
            // rawTemperature |= buffer[4];
            // rawTemperature <<= 8;
            // rawTemperature |= buffer[5];
            // _temperature = ((double)rawTemperature * 200 / 0x100000) - 50;
            Int32 rawHumidity = (buffer[1] << 12) | (buffer[2] << 4) | (buffer[3] >> 4);
            _humidity = (rawHumidity * 100.0) / 0x100000;
            Int32 rawTemperature = ((buffer[3] & 0xF) << 16) | (buffer[4] << 8) | buffer[5];
            _temperature = ((rawTemperature * 200.0) / 0x100000) - 50;
        }

        /// <summary>
        /// Perform soft reset command sequence
        /// </summary>
        private void SoftReset()
        {
            _i2cDevice.WriteByte((byte)Command.SoftRest);
            Thread.Sleep(20); // reset requires 20ms at most, c.f. datasheet version 1.1, ch. 5.5
        }

        /// <summary>
        /// Perform calibration command sequence
        /// </summary>
        private void Calibrate()
        {
            Span<byte> buffer = stackalloc byte[3]
            {
                (byte)Command.Calibrate,
                0x08, // command parameters c.f. datasheet, version 1.1, ch. 5.4
                0x00
            };
            _i2cDevice.Write(buffer);
            Thread.Sleep(10); // wait 10ms c.f. datasheet, version 1.1, ch. 5.4
        }

        private byte GetStatusByte()
        {
            _i2cDevice.WriteByte(0x71);
            Thread.Sleep(10); // whithout this delay the reading the status fails often.
            byte status = _i2cDevice.ReadByte();
            return status;
        }

        private bool IsBusy()
        {
            return (GetStatusByte() & (byte)StatusBit.Busy) == (byte)StatusBit.Busy;
        }

        private bool IsCalibrated()
        {
            return (GetStatusByte() & (byte)StatusBit.Calibrated) == (byte)StatusBit.Calibrated;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
