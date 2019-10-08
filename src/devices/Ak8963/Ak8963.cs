// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using System.Numerics;
using System.Threading;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// AK8963 class implementing a magnetometer
    /// </summary>
    public sealed class Ak8963 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private MeasurementMode _measurementMode;
        private OutputBitMode _outputBitMode;
        private bool _selfTest = false;
        private Ak8963I2cBase _ak8963Interface;
        private bool _autodispose = true;

        /// <summary>
        /// Default I2C address for the AK8963
        /// </summary>
        public const byte DefaultI2cAddress = 0x0C;

        /// <summary>
        /// Default constructor for an independent AK8963
        /// </summary>
        /// <param name="i2CDevice">The I2C device</param>
        public Ak8963(I2cDevice i2CDevice) : this(i2CDevice, new Ak8963I2c())
        { }

        /// <summary>
        /// Constructor to use if AK8963 is behind another element and need a special I2C protocol like
        /// when used with the MPU9250
        /// </summary>
        /// <param name="i2CDevice">The I2C device</param>
        /// <param name="ak8963Interface">The specific interface to communicate with the AK8963</param>
        /// <param name="autoDispose">True to dispose the I2C device when class is disposed</param>
        public Ak8963(I2cDevice i2CDevice, Ak8963I2cBase ak8963Interface, bool autoDispose = true)
        {
            _i2cDevice = i2CDevice;
            _ak8963Interface = ak8963Interface;
            // Initialize the default modes
            _measurementMode = MeasurementMode.PowerDown;
            _outputBitMode = OutputBitMode.Output14bit;
            _selfTest = false;
            _autodispose = autoDispose;
            byte mode = (byte)((byte)_measurementMode | ((byte)_outputBitMode << 4));
            WriteRegister(Register.CNTL, mode);
            if (!IsVersionCorrect())
                throw new IOException($"This device does not contain the correct signature 0x48 for a AK8963");
        }

        /// <summary>
        /// Reset the device
        /// </summary>
        public void Reset()
        {
            WriteRegister(Register.RSV, 0x01);
            _measurementMode = MeasurementMode.PowerDown;
            _outputBitMode = OutputBitMode.Output14bit;
            _selfTest = false;
            // When powering the AK893, doc says 50 ms needed
            Thread.Sleep(50);
        }

        /// <summary>
        /// Get the device information
        /// </summary>
        /// <returns>The device information</returns>
        public byte GetDeviceInfo()
        {
            return ReadByte(Register.INFO);
        }

        /// <summary>
        /// Get the magnetometer bias
        /// </summary>
        public Vector3 MagnetometerBias { get; set; } = Vector3.Zero;

        /// <summary>
        /// Get the magnetometer hardware adjustment bias
        /// </summary>
        public Vector3 MagnetometerAdjustment { get; set; } = Vector3.One;

        /// <summary>
        /// Calibrate the magnetometer. Make sure your sensor is as far as possible of magnet
        /// Calculate as well the magnetometer bias. Please make sure you are moving the magnetometer all over space, rotating it.
        /// Please make sure you are not close to any magnetic field like magnet or phone
        /// </summary>
        /// <param name="numberOfMeasurements">Number of measurement for the calibration, default is 1000</param>
        /// <returns>Returns the factory calibration data</returns>
        public Vector3 CalibrateMagnetometer(int numberOfMeasurements = 1000)
        {
            Vector3 calib = new Vector3();
            Span<byte> rawData = stackalloc byte[3];

            var oldPower = MeasurementMode;

            // Stop the magnetometer
            MeasurementMode = MeasurementMode.PowerDown;
            // Enter the magnetometer Fuse mode to read the calibration data
            // Page 13 of documentation 
            MeasurementMode = MeasurementMode.FuseRomAccess;
            // Read the data
            // See http://www.invensense.com/wp-content/uploads/2017/11/RM-MPU-9250A-00-v1.6.pdf
            // Page 53
            ReadBytes(Register.ASAX, rawData);
            calib.X = (float)(((rawData[0] - 128) / 256.0 + 1.0));
            calib.Y = (float)(((rawData[1] - 128) / 256.0 + 1.0));
            calib.Z = (float)(((rawData[2] - 128) / 256.0 + 1.0));
            MeasurementMode = MeasurementMode.PowerDown;
            MeasurementMode = oldPower;

            // Now calculate the bias
            // Store old mode to restore after
            var oldMode = MeasurementMode;
            Vector3 minbias = new Vector3();
            Vector3 maxbias = new Vector3();

            // Setup the 100Hz continuous mode
            MeasurementMode = MeasurementMode.ContinuousMeasurement100Hz;
            for (int reading = 0; reading < numberOfMeasurements; reading++)
            {
                // Timeout = 100Hz = 10 ms + 2 ms for propagation
                // First read may not go thru correctly
                try
                {
                    var bias = ReadMagnetometerWithoutCorrection(true, TimeSpan.FromMilliseconds(12));
                    minbias.X = Math.Min(bias.X, minbias.X);
                    minbias.Y = Math.Min(bias.Y, minbias.Y);
                    minbias.Z = Math.Min(bias.Z, minbias.Z);
                    maxbias.X = Math.Max(bias.X, maxbias.X);
                    maxbias.Y = Math.Max(bias.Y, maxbias.Y);
                    maxbias.Z = Math.Max(bias.Z, maxbias.Z);
                    // 10 ms = 100Hz, so waiting to make sure we have new data
                    Thread.Sleep(10);
                }
                catch (TimeoutException)
                {
                    // We skip the reading
                }

            }
            // Store the bias
            var magBias = (maxbias + minbias) / 2;
            magBias *= calib;
            MagnetometerBias = magBias;
            MagnetometerAdjustment = calib;

            return calib;
        }

        /// <summary>
        /// True if there is a data to read
        /// </summary>
        public bool HasDataToRead => (ReadByte(Register.ST1) & 0x01) == 0x01;

        /// <summary>
        /// Check if the version is the correct one (0x48). This is fixed for this device
        /// Page 28 from the documentation :
        /// Device ID of AKM. It is described in one byte and fixed value.  48H: fixed 
        /// </summary>
        /// <returns>Returns true if the version match</returns>
        public bool IsVersionCorrect()
        {
            return ReadByte(Register.WIA) == 0x48;
        }

        /// <summary>
        /// Read the magnetometer without Bias correction and can wait for new data to be present
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///         +X
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///    +Z   +Y
        /// </remarks>
        /// <param name="waitForData">true to wait for new data</param>
        /// <param name="timeout">timeout for waiting the data, ignored if waitForData is false</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometerWithoutCorrection(bool waitForData, TimeSpan timeout)
        {
            Span<byte> rawData = stackalloc byte[6];
            // Wait for a data to be present
            if (waitForData)
            {
                DateTime dt = DateTime.Now.Add(timeout);
                while (!HasDataToRead)
                {
                    if (DateTime.Now > dt)
                        throw new TimeoutException($"{nameof(ReadMagnetometer)} timeout reading value");
                }
            }

            ReadBytes(Register.HXL, rawData);
            // In continuous mode, make sure to read the ST2 data to clear up
            if ((_measurementMode == MeasurementMode.ContinuousMeasurement100Hz) ||
                (_measurementMode == MeasurementMode.ContinuousMeasurement8Hz))
            {
                ReadByte(Register.ST2);
            }

            Vector3 magneto = new Vector3();
            magneto.X = BinaryPrimitives.ReadInt16LittleEndian(rawData);
            magneto.Y = BinaryPrimitives.ReadInt16LittleEndian(rawData.Slice(2));
            magneto.Z = BinaryPrimitives.ReadInt16LittleEndian(rawData.Slice(4));

            if (OutputBitMode == OutputBitMode.Output16bit)
            {
                // From the documentation range is from 32760 which does represent 4912 µT
                // result of 4912.0f / 32760.0f
                magneto *= 0.1499389499389499f;
            }
            else
            {
                // result of 4912.0f / 8192.0f
                magneto *= 0.599609375f;
            }
            return magneto;

        }

        /// <summary>
        /// Read the magnetometer with bias correction and can wait for new data to be present
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///         +X
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///    +Z   +Y
        /// </remarks>
        /// <param name="waitForData">true to wait for new data</param>
        /// <param name="timeout">timeout for waiting the data, ignored if waitForData is false</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometer(bool waitForData, TimeSpan timeout)
        {
            var magn = ReadMagnetometerWithoutCorrection(waitForData, timeout);
            magn *= MagnetometerAdjustment;
            magn -= MagnetometerBias;
            return magn;
        }

        /// <summary>
        /// <![CDATA[
        /// Get or set the device self test mode. 
        /// If set to true, this creates a magnetic field
        /// Once you read it, you will have the results of the self test
        /// 14-bit output(BIT=“0”)  
        ///          | HX[15:0]        | HY[15:0]        | HZ[15:0] 
        /// Criteria | -50 =< HX =< 50 | -50 =< HY =< 50 | -800 =< HZ =< -200 
        /// 16-bit output(BIT=“1”)  
        ///          | HX[15:0]          | HY[15:0]          | HZ[15:0] 
        /// Criteria | -200 =< HX =< 200 | -200 =< HY =< 200 | -3200 =< HZ =< -800 
        /// ]]>
        /// </summary>
        public bool MageneticFieldGeneratorEnabled
        {
            get => _selfTest;

            set
            {
                byte mode = value ? (byte)0b01000_0000 : (byte)0b0000_0000;
                WriteRegister(Register.ASTC, mode);
                _selfTest = value;
            }
        }

        /// <summary>
        /// Select the measurement mode
        /// </summary>
        public MeasurementMode MeasurementMode
        {
            get => _measurementMode;

            set
            {
                byte mode = (byte)((byte)value | ((byte)_outputBitMode << 4));
                WriteRegister(Register.CNTL, mode);
                _measurementMode = value;
                // according to documentation:
                // After power-down mode is set, at least 100µs is needed before setting another mode
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Select the output bit rate
        /// </summary>
        public OutputBitMode OutputBitMode
        {
            get => _outputBitMode;

            set
            {
                byte mode = (byte)(((byte)value << 4) | (byte)_measurementMode);
                WriteRegister(Register.CNTL, mode);
                _outputBitMode = value;
            }
        }

        private void WriteRegister(Register reg, byte data) => _ak8963Interface.WriteRegister(_i2cDevice, (byte)reg, data);

        private byte ReadByte(Register reg) => _ak8963Interface.ReadByte(_i2cDevice, (byte)reg);

        private void ReadBytes(Register reg, Span<byte> readBytes) => _ak8963Interface.ReadBytes(_i2cDevice, (byte)reg, readBytes);

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public void Dispose()
        {
            if (_autodispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
