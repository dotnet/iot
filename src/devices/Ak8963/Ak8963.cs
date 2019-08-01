// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using System.Numerics;
using System.Threading;

namespace Iot.Device.Ak8963
{
    /// <summary>
    /// AK8963 class implementing a magnetometer
    /// </summary>
    public class Ak8963 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private bool _autoDispose;
        private MeasurementMode _measurementMode;
        private OutputBitMode _outputBitMode;
        private bool _selfTest = false;
        private Ak8963I2cBase _ak8963Interface;

        public const byte DefaultI2cAddress = 0x0C;

        public Ak8963(I2cDevice i2CDevice, bool autoDispose = true) : this(i2CDevice, new Ak8963I2c(), autoDispose)
        { }

        public Ak8963(I2cDevice i2CDevice, Ak8963I2cBase ak8963Interface, bool autoDispose = true)
        {
            _i2cDevice = i2CDevice;
            _autoDispose = autoDispose;
            _ak8963Interface = ak8963Interface;
            // Initialize the default modes
            _measurementMode = MeasurementMode.PowerDown;
            _outputBitMode = OutputBitMode.Output14bit;
            byte mode = (byte)((byte)_measurementMode | ((byte)_outputBitMode << 4));
            WriteRegister(Register.CNTL, mode);
            if (!CheckVersion())
                throw new IOException($"This device does not contain the correct signature 0x48 for a AK8963");
        }

        /// <summary>
        /// Reset the device
        /// </summary>
        public void Reset()
        {
            WriteRegister(Register.RSV, 0x01);
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
        public Vector3 MagnometerBias { get; set; } = Vector3.Zero;

        /// <summary>
        /// Calibrate the magnetometer. Make sure your sensor is as far as possible of magnet
        /// Calculate as well the magnetometer bias
        /// </summary>
        /// <returns>Returns the factory calibration data</returns>
        public Vector3 CalibrateMagnetometer()
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
            ReadByteArray(Register.ASAX, rawData);
            calib.X = (float)((rawData[0] - 128) / 256.0 + 1.0);
            calib.Y = (float)((rawData[1] - 128) / 256.0 + 1.0);
            calib.Z = (float)((rawData[2] - 128) / 256.0 + 1.0);
            MeasurementMode = MeasurementMode.PowerDown;
            MeasurementMode = oldPower;

            // Now calculate the bias
            // Store old mode to restore after
            var oldMode = MeasurementMode;
            Vector3 minbias = new Vector3();
            Vector3 maxbias = new Vector3();
            int numberMeasurements = 1500;

            // Setup the 100Hz continuous mode
            MeasurementMode = MeasurementMode.ContinousMeasurement100Hz;
            for (int reading = 0; reading < numberMeasurements; reading++)
            {
                var bias = ReadMagnetometer(true);
                minbias.X = Math.Min(bias.X, minbias.X);
                minbias.Y = Math.Min(bias.Y, minbias.Y);
                minbias.Z = Math.Min(bias.Z, minbias.Z);
                maxbias.X = Math.Max(bias.X, maxbias.X);
                maxbias.Y = Math.Max(bias.Y, maxbias.Y);
                maxbias.Z = Math.Max(bias.Z, maxbias.Z);
                // 10 ms = 100Hz, so waiting to make sure we have new data
                Thread.Sleep(10);
            }
            // Store the bias
            MagnometerBias = ((maxbias + minbias) / 2) + calib;

            return calib;
        }

        /// <summary>
        /// True if there is a data to read
        /// </summary>
        public bool WaitForDataReady => (ReadByte(Register.ST1) & 0x01) == 0x01;

        /// <summary>
        /// Check if the version is the correct one (0x48). This is fixed for this device
        /// Page 28 from the documentation :
        /// Device ID of AKM. It is described in one byte and fixed value.  48H: fixed 
        /// </summary>
        /// <returns>Returns true if the version match</returns>
        public bool CheckVersion()
        {
            return ReadByte(Register.WIA) == 0x48;
        }

        /// <summary>
        /// Read the magnetometer and can wait for new data to be present
        ///         +X
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///    +Z   +Y
        /// </summary>
        /// <param name="waitForData">true to wait for new data</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometer(bool waitForData = false)
        {
            Span<byte> rawData = stackalloc byte[6];
            // Wait for a data to be present
            if (waitForData)
            {
                while (!WaitForDataReady)
                    ;
            }

            ReadByteArray(Register.HXL, rawData);
            // In continuous mode, make sure to read the ST2 data to clear up
            if ((_measurementMode == MeasurementMode.ContinousMeasurement100Hz) ||
                (_measurementMode == MeasurementMode.ContinousMeasurement8Hz))
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
                magneto *= 4912.0f / 32760.0f;
            }
            else
            {
                magneto *= 4912.0f / 8192.0f;
            }
            return magneto;

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
                // After power-down mode is set, at least 100µs(Twat) is needed before setting another mode
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

        private void WriteRegister(Register reg, byte data) => _ak8963Interface.WriteRegister(_i2cDevice, reg, data);

        private byte ReadByte(Register reg) => _ak8963Interface.ReadByte(_i2cDevice, reg);

        private void ReadByteArray(Register reg, Span<byte> readBytes) => _ak8963Interface.ReadByteArray(_i2cDevice, reg, readBytes);

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public void Dispose()
        {
            if (_autoDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
