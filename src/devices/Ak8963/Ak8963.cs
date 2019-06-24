// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;
using System.Threading;

namespace Iot.Device.Ak8963
{
    public class Ak8963 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private bool _autoDispose;
        private MeasurementMode _measurementMode;
        private OutputBitMode _outputBitMode;
        private bool _selfTest = false;
        private Vector3 _magnetometerBias = new Vector3();
        private Ak8963Interface _ak8963Interface;

        public const byte DefaultI2cAddress = 0x0C;

        public Ak8963(I2cDevice i2CDevice, bool autoDispose = true) : this(i2CDevice, new Ak8963I2c(), autoDispose)
        { }

        public Ak8963(I2cDevice i2CDevice, Ak8963Interface ak8963Interface, bool autoDispose = true)
        {
            _i2cDevice = i2CDevice;
            _autoDispose = autoDispose;
            _ak8963Interface = ak8963Interface;
            // Initialize the default modes
            _measurementMode = MeasurementMode.PowerDown;
            _outputBitMode = OutputBitMode.Output14bit;
            byte mode = (byte)((byte)_measurementMode | ((byte)_outputBitMode << 4));
            WriteRegister(Register.CNTL, mode);
        }

        /// <summary>
        /// Reset the device
        /// </summary>
        public void Reset()
        {
            WriteRegister(Register.RSV, 0x01);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Get the device information
        /// </summary>
        /// <returns></returns>
        public byte GetDeviceInfo()
        {
            return ReadByte(Register.INFO);
        }

        /// <summary>
        /// Get the magnetometer bias
        /// </summary>
        public Vector3 MagnometerBias => _magnetometerBias;

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

            // Stop the magetometer
            MeasurementMode = MeasurementMode.PowerDown;
            // Enter the magnetometer Fuse mode to read the calibration data
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

            // Setup the 100Hz continous mode
            MeasurementMode = MeasurementMode.ContinousMeasurement100Hz;
            for (int reading = 0; reading < numberMeasurements; reading++)
            {
                var bias = ReadMagnetometer(true);
                minbias.X = bias.X < minbias.X ? bias.X : minbias.X;
                minbias.Y = bias.Y < minbias.Y ? bias.Y : minbias.Y;
                minbias.Z = bias.Z < minbias.Z ? bias.Z : minbias.Z;
                maxbias.X = bias.X > maxbias.X ? bias.X : maxbias.X;
                maxbias.Y = bias.Y > maxbias.Y ? bias.Y : maxbias.Y;
                maxbias.Z = bias.Z > maxbias.Z ? bias.Z : maxbias.Z;
                Thread.Sleep(12);
            }
            // Store the bias
            _magnetometerBias = ((maxbias + minbias) / 2) + calib;

            return calib;
        }

        /// <summary>
        /// True if there is a data to read
        /// </summary>
        public bool DataReady => (ReadByte(Register.ST1) & 0x01) == 0x01;

        /// <summary>
        /// Check if the version is the correct one (0x48)
        /// </summary>
        /// <returns>Returns true if the version match</returns>
        public bool CheckVersion()
        {
            return ReadByte(Register.WIA) == 0x48;
        }

        /// <summary>
        /// Read the magnetomer and can wait for new data to be present
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
                while (!DataReady)
                    ;
            }
            ReadByteArray(Register.HXL, rawData);
            // In continous mode, make sure to read the ST2 data to clear up
            if ((_measurementMode == MeasurementMode.ContinousMeasurement100Hz) ||
                (_measurementMode == MeasurementMode.ContinousMeasurement8Hz))
                ReadByte(Register.ST2);

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
        /// Get the Magetometer data and wait for them to be ready
        ///         +X
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///    +Z   +Y
        /// </summary>
        public Vector3 Magnetometer => ReadMagnetometer(true);

        /// <summary>
        /// Get or set the device self test mode. 
        /// If set to true, this creates a magnetic field
        /// Once you read it, you will have the results of the self test
        /// 14-bit output(BIT=“0”)  
        ///          | HX[15:0]        | HY[15:0]        | HZ[15:0] 
        /// Criteria | -50 =< HX =< 50 | -50 =< HY =< 50 | -800 =< HZ =< -200 
        /// 16-bit output(BIT=“1”)  
        ///          | HX[15:0]          | HY[15:0]          | HZ[15:0] 
        /// Criteria | -200 =< HX =< 200 | -200 =< HY =< 200 | -3200 =< HZ =< -800 
        /// </summary>
        public bool SelfTest
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
                // Need to wait a little bit for the mode to change
                Thread.Sleep(10);
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
