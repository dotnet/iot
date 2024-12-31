// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;

namespace Iot.Device.Bmp180
{
    /// <summary>
    /// Bmm150 class implementing a magnetometer
    /// </summary>
    [Interface("Bmm150 class implementing a magnetometer")]
    public sealed class Bmm150 : IDisposable
    {
        /// <summary>
        /// I2c device comm channel
        /// </summary>
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Bmm150 device interface
        /// </summary>
        private Bmm150I2cBase _bmm150Interface;

        /// <summary>
        /// Bmm150 Trim extended register data
        /// </summary>
        private Bmm150TrimRegisterData _trimData;

        /// <summary>
        /// Magnetometer (R-HALL) temperature compensation value, used in axis compensation calculation functions
        /// </summary>
        private uint _rHall;

        /// <summary>
        /// Flag to evaluate disposal of resources
        /// </summary>
        private bool _shouldDispose = true;

        /// <summary>
        /// Gets or sets Magnetometer calibration compensation vector
        /// </summary>
        public Vector3 CalibrationCompensation { get; set; } = new Vector3();

        /// <summary>
        /// Primary I2C address for the Bmm150
        /// In the official sheet (P36) states that address is 0x13: https://github.com/m5stack/M5_BMM150/blob/master/src/M5_BMM150_DEFS.h#L163
        /// </summary>
        public const byte PrimaryI2cAddress = 0x13;

        /// <summary>
        /// Secondary I2C address for the Bmm150
        /// In the official sheet (P36) states that address is 0x13, alhtough for m5stack is 0x10
        /// </summary>
        public const byte SecondaryI2cAddress = 0x10;

        /// <summary>
        /// Default timeout to use when timeout is not provided in the reading methods
        /// </summary>
        [Property]
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Default constructor for an independent Bmm150
        /// </summary>
        /// <param name="i2CDevice">The I2C device</param>
        public Bmm150(I2cDevice i2CDevice)
            : this(i2CDevice, new Bmm150I2c())
        {
        }

        /// <summary>
        /// Constructor to use if Bmm150 is behind another element and need a special I2C protocol like
        /// when used with the MPU9250
        /// </summary>
        /// <param name="i2cDevice">The I2C device</param>
        /// <param name="Bmm150Interface">The specific interface to communicate with the Bmm150</param>
        /// <param name="shouldDispose">True to dispose the I2C device when class is disposed</param>
        public Bmm150(I2cDevice i2cDevice, Bmm150I2cBase Bmm150Interface, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _bmm150Interface = Bmm150Interface;
            _shouldDispose = shouldDispose;

            Initialize();

            // After initializing the device we read the
            _trimData = ReadTrimRegisters();
        }

        /// <summary>
        /// Reads the trim registers of the sensor, used in compensation (x,y,z) calculation
        /// More info, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1199
        /// </summary>
        /// <returns>Trim registers value</returns>
        private Bmm150TrimRegisterData ReadTrimRegisters()
        {
            Span<Byte> trimX1y1Data = stackalloc byte[2];
            Span<Byte> trimXyzData = stackalloc byte[4];
            Span<Byte> trimXy1xy2Data = stackalloc byte[10];

            // Read trim extended registers
            ReadBytes(Bmp180Register.BMM150_DIG_X1, trimX1y1Data);
            ReadBytes(Bmp180Register.BMM150_DIG_Z4_LSB, trimXyzData);
            ReadBytes(Bmp180Register.BMM150_DIG_Z2_LSB, trimXy1xy2Data);

            return new Bmm150TrimRegisterData(trimX1y1Data, trimXyzData, trimXy1xy2Data);
        }

        /// <summary>
        /// Starts the Bmm150 init sequence
        /// </summary>
        private void Initialize()
        {
            // Set Sleep mode
            WriteRegister(Bmp180Register.POWER_CONTROL_ADDR, 0x01);
            Wait(6);

            // Check for a valid chip ID
            if (!IsVersionCorrect)
            {
                throw new IOException($"This device does not contain the correct signature (0x32) for a Bmm150");
            }

            // Set operation mode to: "Normal Mode"
            WriteRegister(Bmp180Register.OP_MODE_ADDR, 0x00);
        }

        /// <summary>
        /// Calibrate the magnetometer.
        /// Please make sure you are not close to any magnetic field like magnet or phone
        /// Please make sure you are moving the magnetometer all over space, rotating it.
        /// </summary>
        /// <param name="numberOfMeasurements">Number of measurement for the calibration, default is 100</param>
        // https://platformio.org/lib/show/12697/M5_BMM150
        [Obsolete("Prefer another overload")]
        public void CalibrateMagnetometer(int numberOfMeasurements = 100)
        {
            CalibrateMagnetometer(null, numberOfMeasurements);
        }

        /// <summary>
        /// Calibrate the magnetometer.
        /// Please make sure you are not close to any magnetic field like magnet or phone
        /// Please make sure you are moving the magnetometer all over space, rotating it.
        /// </summary>
        /// <param name="progress">A progress provider (returns a value in percent)</param>
        /// <param name="numberOfMeasurements">Number of measurement for the calibration, default is 100</param>
        // https://platformio.org/lib/show/12697/M5_BMM150
        public void CalibrateMagnetometer(IProgress<double>? progress, int numberOfMeasurements)
        {
            Vector3 mag_min = new Vector3() { X = 9000, Y = 9000, Z = 30000 };
            Vector3 mag_max = new Vector3() { X = -9000, Y = -9000, Z = -30000 };
            Vector3 rawMagnetometerData;
            if (numberOfMeasurements <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfMeasurements), "The number of measurements must be > 0");
            }

            for (int i = 0; i < numberOfMeasurements; i++)
            {
                try
                {
                    rawMagnetometerData = ReadMagnetometerWithoutCorrection();

                    if (rawMagnetometerData.X != 0)
                    {
                        mag_min.X = (rawMagnetometerData.X < mag_min.X) ? rawMagnetometerData.X : mag_min.X;
                        mag_max.X = (rawMagnetometerData.X > mag_max.X) ? rawMagnetometerData.X : mag_max.X;
                    }

                    if (rawMagnetometerData.Y != 0)
                    {
                        mag_max.Y = (rawMagnetometerData.Y > mag_max.Y) ? rawMagnetometerData.Y : mag_max.Y;
                        mag_min.Y = (rawMagnetometerData.Y < mag_min.Y) ? rawMagnetometerData.Y : mag_min.Y;
                    }

                    if (rawMagnetometerData.Z != 0)
                    {
                        mag_min.Z = (rawMagnetometerData.Z < mag_min.Z) ? rawMagnetometerData.Z : mag_min.Z;
                        mag_max.Z = (rawMagnetometerData.Z > mag_max.Z) ? rawMagnetometerData.Z : mag_max.Z;
                    }

                    // Wait for 100ms until next reading
                    Wait(100);
                }
                catch
                {
                    // skip this reading
                }

                if (progress != null)
                {
                    double percentDone = ((double)i / numberOfMeasurements) * 100.0;
                    progress.Report(percentDone);
                }
            }

            if (progress != null)
            {
                progress.Report(100.0);
            }

            // Refresh CalibrationCompensation vector
            CalibrationCompensation = new Vector3()
            {
                X = (mag_max.X + mag_min.X) / 2,
                Y = (mag_max.Y + mag_min.Y) / 2,
                Z = (mag_max.Z + mag_min.Z) / 2
            };
        }

        /// <summary>
        /// True if there is a data to read
        /// </summary>
        public bool HasDataToRead => (ReadByte(Bmp180Register.DATA_READY_STATUS) & 0x01) == 0x01;

        /// <summary>
        /// Check if the version is the correct one (0x32). This is fixed for this device
        /// </summary>
        /// <returns>Returns true if the version match</returns>
        private bool IsVersionCorrect => ReadByte(Bmp180Register.WIA) == 0x32;

        /// <summary>
        /// Read the magnetometer without Bias correction and can wait for new data to be present
        /// </summary>
        /// <param name="waitForData">true to wait for new data</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometerWithoutCorrection(bool waitForData = true) => ReadMagnetometerWithoutCorrection(waitForData, DefaultTimeout);

        /// <summary>
        /// Read the magnetometer without Bias correction and can wait for new data to be present
        /// More info, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L921
        /// </summary>
        /// <param name="waitForData">true to wait for new data</param>
        /// <param name="timeout">timeout for waiting the data, ignored if waitForData is false</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometerWithoutCorrection(bool waitForData, TimeSpan timeout)
        {
            Span<Byte> rawData = stackalloc byte[8];

            // Wait for a data to be present
            if (waitForData)
            {
                DateTime dt = DateTime.UtcNow.Add(timeout);
                while (!HasDataToRead)
                {
                    if (DateTime.UtcNow > dt)
                    {
                        throw new TimeoutException($"{nameof(ReadMagnetometerWithoutCorrection)} timeout reading value");
                    }
                }
            }

            ReadBytes(Bmp180Register.HXL, rawData);

            Vector3 magnetoRaw = new Vector3();

            // Because we mix and match signed and unsigned below
            unchecked
            {
                int temp;
                // Shift the MSB data to left by 5 bits
                // Multiply by 32 to get the shift left by 5 value
                // X and Y have 13 significant bits each
                temp = (rawData[1]) << 5 | rawData[0] >> 3;
                if ((rawData[1] & 0x80) == 0x80)
                {
                    temp = temp | (int)0xFFFFE000;
                }

                magnetoRaw.X = temp;

                // Shift the MSB data to left by 5 bits
                // Multiply by 32 to get the shift left by 5 value
                temp = (rawData[3]) << 5 | rawData[2] >> 3;
                if ((rawData[3] & 0x80) == 0x80)
                {
                    temp = temp | (int)0xFFFFE000;
                }

                magnetoRaw.Y = temp;

                // Shift the MSB data to left by 7 bits
                // Multiply by 128 to get the shift left by 7 value
                // The Z value has 15 significant bits
                temp = (rawData[5]) << 7 | rawData[4] >> 1;
                if ((rawData[5] & 0x80) == 0x80)
                {
                    temp = temp | (int)0xFFFF8000;
                }

                magnetoRaw.Z = temp;
            }

            _rHall = (uint)(rawData[7] << 6 | rawData[6] >> 2);

            return magnetoRaw;
        }

        /// <summary>
        /// Read the magnetometer with bias correction and can wait for new data to be present
        /// </summary>
        /// <param name="waitForData">true to wait for new data</param>
        /// <returns>The data from the magnetometer</returns>
        [Telemetry("Magnetometer")]
        public Vector3 ReadMagnetometer(bool waitForData = true) => ReadMagnetometer(waitForData, DefaultTimeout);

        /// <summary>
        /// Read the magnetometer with compensation calculation and can wait for new data to be present
        /// </summary>
        /// <param name="waitForData">true to wait for new data</param>
        /// <param name="timeout">timeout for waiting the data, ignored if waitForData is false</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometer(bool waitForData, TimeSpan timeout)
        {
            var magn = ReadMagnetometerWithoutCorrection(waitForData, timeout);

            magn.X = (float)Bmm150Compensation.CompensateX(magn.X - CalibrationCompensation.X, _rHall, _trimData);
            magn.Y = (float)Bmm150Compensation.CompensateY(magn.Y - CalibrationCompensation.Y, _rHall, _trimData);
            magn.Z = (float)Bmm150Compensation.CompensateZ(magn.Z - CalibrationCompensation.Z, _rHall, _trimData);

            return magn;
        }

        private void WriteRegister(Bmp180Register reg, byte data) => _bmm150Interface.WriteRegister(_i2cDevice, (byte)reg, data);

        private byte ReadByte(Bmp180Register reg) => _bmm150Interface.ReadByte(_i2cDevice, (byte)reg);

        private void ReadBytes(Bmp180Register reg, Span<Byte> readBytes) => _bmm150Interface.ReadBytes(_i2cDevice, (byte)reg, readBytes);

        private void Wait(int milisecondsTimeout)
        {
            Thread.Sleep(milisecondsTimeout);
        }

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }
    }
}
