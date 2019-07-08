// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;
using System.Threading;
using Iot.Units;

namespace Iot.Device.Bno055
{
    public class Bno055Sensor : IDisposable
    {
        /// <summary>
        /// The default I2C Address, page 91 of the main documentation
        /// https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BNO055-DS000.pdf
        /// </summary>
        public const byte DefaultI2cAddress = 0x28;

        /// <summary>
        /// This is the second I2C Address. It needs to be activated to be valid
        /// </summary>
        public const byte SecondI2cAddress = 0x29;

        private const byte DeviceId = 0xA0;
        private readonly bool _autoDispoable;
        private I2cDevice _i2cDevice;
        private OperationMode _operationMode;
        private Units _units;

        /// <summary>
        /// Get/Set the operation mode
        /// </summary>
        public OperationMode OperationMode
        {
            get { return _operationMode; }
            set
            {
                _operationMode = value;
                SetConfigMode(true);
                SetOperationMode(_operationMode);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Set/Get the power mode
        /// </summary>
        public PowerMode PowerMode
        {
            get { return (PowerMode)ReadByte(Registers.PWR_MODE); }
            set
            {
                SetConfigMode(true);
                WriteReg(Registers.PWR_MODE, (byte)value);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Set/Get the temperature source
        /// </summary>
        public TemperatureSource TemperatureSource
        {
            get { return (TemperatureSource)ReadByte(Registers.TEMP_SOURCE); }
            set
            {
                SetConfigMode(true);
                WriteReg(Registers.TEMP_SOURCE, (byte)value);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Set/Get the units used. By default, international system is used
        /// </summary>
        public Units Units
        {
            get { return (Units)ReadByte(Registers.UNIT_SEL); }
            set
            {
                SetConfigMode(true);
                _units = value;
                WriteReg(Registers.UNIT_SEL, (byte)_units);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Get the information about various sensor system versions and ID
        /// </summary>
        public Info Info { get; internal set; }

        /// <summary>
        /// Create an BNO055 sensor
        /// </summary>
        /// <param name="i2cDevice">The I2C Device</param>
        /// <param name="operationMode">The operation mode to setup</param>
        /// <param name="autoDispoable">true to dispose the I2C device at dispose</param>
        public Bno055Sensor(I2cDevice i2cDevice, OperationMode operationMode = OperationMode.AccelerometerMagnetometerGyroscopeRelativeOrientation, bool autoDispoable = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException($"{nameof(i2cDevice)} can't be null");
            _autoDispoable = autoDispoable;

            // A first write to initate the device
            WriteReg(Registers.PAGE_ID, 0);
            SetConfigMode(true);
            // A second write
            WriteReg(Registers.PAGE_ID, 0);
            // Get the ID
            Info = new Info();
            Info.ChipId = ReadByte(Registers.CHIP_ID);
            if (Info.ChipId != DeviceId)
                throw new Exception($"{nameof(Bno055Sensor)} is not a valid sensor");
            Info.AcceleratorId = ReadByte(Registers.ACCEL_REV_ID);
            Info.MagnetometerId = ReadByte(Registers.MAG_REV_ID);
            Info.GyroscopeId = ReadByte(Registers.GYRO_REV_ID);
            Info.FirmwareVersion = new Version(ReadByte(Registers.SW_REV_ID_MSB), ReadByte(Registers.SW_REV_ID_LSB));
            Info.BootloaderVersion = new Version(ReadByte(Registers.BL_REV_ID), 0);

            // Check if a reset is needed
            if (TemperatureSource != TemperatureSource.Gyroscope)
            {
                WriteReg(Registers.SYS_TRIGGER, 0x20);
                // Need 650 milliseconds after reset
                Thread.Sleep(650);
                PowerMode = PowerMode.Normal;
                WriteReg(Registers.SYS_TRIGGER, 0x00);
                TemperatureSource = TemperatureSource.Gyroscope;
            }
            // Select default units
            Units = Units.AccelerationMeterPerSecond | Units.AngularRateDegreePerSecond | Units.DataOutputFormatWindows | Units.EulerAnglesDegrees | Units.TemperatureCelsius;
            // Using the gyroscope as temeprature source
            TemperatureSource = TemperatureSource.Gyroscope;
            // Set the operation mode
            OperationMode = operationMode;
            // Get the current unit (should be all at default)
            _units = Units;
        }

        /// <summary>
        /// Set internal or external crystal usage.
        /// Note: if you don't have an external crystal, don't use this function
        /// </summary>
        /// <param name="external">true to set to external</param>
        public void SetExternalCrystal(bool external)
        {
            SetConfigMode(true);
            if (external)
            {
                WriteReg(Registers.SYS_TRIGGER, 0x80);
            }
            else
            {
                WriteReg(Registers.SYS_TRIGGER, 0x00);
            }
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the status. If there is an error, GetError() will give more details
        /// </summary>
        /// <returns></returns>
        public Status GetStatus() => (Status)ReadByte(Registers.SYS_STAT);

        /// <summary>
        /// Get the latest error
        /// </summary>
        /// <returns>eturns the latest error</returns>
        public Error GetError() => (Error)ReadByte(Registers.SYS_ERR);

        /// <summary>
        /// Run a self test. In case of error, use GetStatus() and GetError() to get the last error
        /// </summary>
        /// <returns>Status fo the test</returns>
        public TestResult RunSelfTest()
        {
            SetConfigMode(true);
            var oldTrigger = ReadByte(Registers.SYS_TRIGGER);
            // Set test mode
            WriteReg(Registers.SYS_TRIGGER, (byte)(oldTrigger | 0x01));
            // Wait for the test to go
            Thread.Sleep(1000);
            var testRes = ReadByte(Registers.SELFTEST_RESULT);
            SetConfigMode(false);
            return (TestResult)testRes;
        }

        /// <summary>
        /// Returns the calibration status for the system and sensors
        /// </summary>
        /// <returns>Returns the calibration status for the system and sensors</returns>
        public CalibrationStatus GetCalibrationStatus() => (CalibrationStatus)ReadByte(Registers.CALIB_STAT);

        /// <summary>
        /// Get the accelerometer calibration data
        /// </summary>
        /// <returns>Returns the accelerometers calibration data</returns>
        public Vector4 GetAccelerometerCalibrationData()
        {
            SetConfigMode(true);
            var vect = GetVectorData(Registers.ACCEL_OFFSET_X_LSB);
            var radius = ReadInt16(Registers.ACCEL_RADIUS_LSB);
            SetConfigMode(false);
            return new Vector4(vect, radius);
        }

        /// <summary>
        /// Set the accelerometer calibration data
        /// </summary>
        /// <param name="calibrationData"></param>
        public void SetAccelerometerCalibrationData(Vector4 calibrationData)
        {
            SetConfigMode(true);
            Span<byte> outArray = stackalloc byte[7];
            outArray[0] = (byte)Registers.ACCEL_OFFSET_X_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), (short)calibrationData.X);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(3), (short)calibrationData.Y);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.Z);
            _i2cDevice.Write(outArray);
            outArray[4] = (byte)Registers.ACCEL_RADIUS_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.W);
            _i2cDevice.Write(outArray.Slice(4));
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the magnetometer calibration data
        /// </summary>
        /// <returns>Returns the magnetometer calibration data</returns>
        public Vector4 GetMagnetometerCalibrationData()
        {
            SetConfigMode(true);
            var vect = GetVectorData(Registers.MAG_OFFSET_X_LSB);
            var radius = ReadInt16(Registers.MAG_RADIUS_LSB);
            SetConfigMode(false);
            return new Vector4(vect, radius);
        }

        /// <summary>
        /// Set the magnetometer calibration data
        /// </summary>
        /// <param name="calibrationData"></param>
        public void SetMagnetometerCalibrationData(Vector4 calibrationData)
        {
            SetConfigMode(true);
            Span<byte> outArray = stackalloc byte[7];
            outArray[0] = (byte)Registers.MAG_OFFSET_X_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), (short)calibrationData.X);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(3), (short)calibrationData.Y);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.Z);
            _i2cDevice.Write(outArray);
            outArray[4] = (byte)Registers.MAG_RADIUS_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.W);
            _i2cDevice.Write(outArray.Slice(4));
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the gyroscope calibration data
        /// </summary>
        /// <returns>X, Y and Z data</returns>
        public Vector3 GetGyroscopeCalibrationData()
        {
            SetConfigMode(true);
            var vect = GetVectorData(Registers.GYRO_OFFSET_X_LSB);
            SetConfigMode(false);
            return vect;
        }

        /// <summary>
        /// Set the gyroscope calibration data
        /// </summary>
        /// <param name="calibrationData">X, Y and Z data</param>
        public void SetGyroscopeCalibrationData(Vector3 calibrationData)
        {
            SetConfigMode(true);
            Span<byte> outArray = stackalloc byte[7];
            outArray[0] = (byte)Registers.GYRO_OFFSET_X_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), (short)calibrationData.X);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(3), (short)calibrationData.Y);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.Z);
            _i2cDevice.Write(outArray);
            SetConfigMode(false);
        }

        /// <summary>
        /// Set the Axis map
        /// </summary>
        /// <param name="x">X axis setting</param>
        /// <param name="y">Y axis setting</param>
        /// <param name="z">Z axis setting</param>
        public void SetAxisMap(AxisSetting x, AxisSetting y, AxisSetting z)
        {
            SetConfigMode(true);
            WriteReg(Registers.AXIS_MAP_CONFIG, (byte)(((byte)z.Axis << 4) | ((byte)y.Axis << 2) | (byte)x.Axis));
            WriteReg(Registers.AXIS_MAP_SIGN, (byte)(((byte)z.Sign << 2) | ((byte)y.Sign << 1) | (byte)x.Sign));
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the Axis map
        /// </summary>
        /// <returns>Returns an array where first element is axis X, then Y then Z</returns>
        public AxisSetting[] GetAxisMap()
        {
            SetConfigMode(true);
            var retMap = ReadByte(Registers.AXIS_MAP_CONFIG);
            var retSign = ReadByte(Registers.AXIS_MAP_SIGN);
            AxisSetting[] axisSettings = new AxisSetting[3];
            axisSettings[0].Axis = (AxisMap)(retMap & 0x03);
            axisSettings[1].Axis = (AxisMap)((retMap >> 2) & 0x03);
            axisSettings[2].Axis = (AxisMap)((retMap >> 4) & 0x03);
            axisSettings[0].Sign = (AxisSign)(retSign & 0x01);
            axisSettings[1].Sign = (AxisSign)((retSign >> 1) & 0x01);
            axisSettings[2].Sign = (AxisSign)((retSign >> 2) & 0x01);
            SetConfigMode(false);
            return axisSettings;
        }

        /// <summary>
        /// Get the orientation (Euler Angles) X = Heading, Y = Roll, Z = Pitch
        /// </summary>
        public Vector3 Orientation
        {
            get
            {
                var retVect = GetVectorData(Registers.EULER_H_LSB);
                // If unit is MeterG, then divide by 900, otherwise divide by 16
                if (_units.HasFlag(Units.EulerAnglesRadians))
                    return retVect / 900;
                else
                    return retVect / 16;
            }
        }
        /// <summary>
        /// Get the Magnetometer
        /// </summary>
        public Vector3 Magnetometer => GetVectorData(Registers.MAG_DATA_X_LSB) / 16;

        /// <summary>
        /// Get the gyroscope
        /// </summary>
        public Vector3 Gyroscope
        {
            get
            {
                var retVect = GetVectorData(Registers.GYRO_DATA_X_LSB);
                if ((_units & Units.AngularRateRotationPerSecond) == Units.AngularRateRotationPerSecond)
                    return retVect / 900;
                else
                    return retVect / 16;
            }
        }

        /// <summary>
        /// Get the accelerometer
        /// Acceleration Vector (100Hz)
        /// Three axis of acceleration (gravity + linear motion)
        /// Default unit in m/s^2, can be changed for mg
        /// </summary>
        public Vector3 Accelerometer
        {
            get
            {
                var retVect = GetVectorData(Registers.ACCEL_DATA_X_LSB);
                // If unit is MeterG, then no convertion, otherwise divide by 100
                if ((_units & Units.AccelerationMeterG) == Units.AccelerationMeterG)
                    return retVect;
                else
                    return retVect / 100;
            }
        }

        /// <summary>
        /// Get the linear acceleration
        /// Linear Acceleration Vector (100Hz)
        /// Three axis of linear acceleration data (acceleration minus gravity)
        /// Default unit in m/s^2, can be changed for mg
        /// </summary>
        public Vector3 LinearAcceleration
        {
            get
            {
                var retVect = GetVectorData(Registers.LINEAR_ACCEL_DATA_X_LSB);
                // If unit is MeterG, then no convertion, otherwise divide by 100
                if ((_units & Units.AccelerationMeterG) == Units.AccelerationMeterG)
                    return retVect;
                else
                    return retVect / 100;
            }
        }
        /// <summary>
        /// Get the gravity
        /// Gravity Vector (100Hz)
        /// Three axis of gravitational acceleration (minus any movement)
        /// Default unit in m/s^2, can be changed for mg
        /// </summary>
        public Vector3 Gravity
        {
            get
            {
                var retVect = GetVectorData(Registers.GRAVITY_DATA_X_LSB);
                // If unit is MeterG, then no convertion, otherwise divide by 100
                if ((_units & Units.AccelerationMeterG) == Units.AccelerationMeterG)
                    return retVect;
                else
                    return retVect / 100;
            }
        }
        /// <summary>
        /// Get the quaternion, unit is 1 Quaternion (unit less) = 2^14 returned result
        /// </summary>
        public Vector4 Quaternion => new Vector4(GetVectorData(Registers.QUATERNION_DATA_X_LSB), ReadInt16(Registers.QUATERNION_DATA_W_LSB));

        /// <summary>
        /// Get the temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                // If unit is Farenheit, then divide by 2, otherwise no convertion
                if ((_units & Units.TemperatureFarenheit) == Units.TemperatureFarenheit)
                    return Temperature.FromFahrenheit(ReadByte(Registers.TEMP) / 2.0f);
                else
                    return Temperature.FromCelsius(ReadByte(Registers.TEMP));
            }
        }

        /// <summary>
        /// Get the interupt status
        /// </summary>
        /// <returns></returns>
        public InteruptStatus GetInteruptStatus() => (InteruptStatus)ReadByte(Registers.INTR_STAT);

        private void SetOperationMode(OperationMode operation)
        {
            WriteReg(Registers.OPR_MODE, (byte)operation);
            // It is necessary to wait 30 milliseconds
            Thread.Sleep(30);
        }

        private Vector3 GetVectorData(Registers reg)
        {
            Span<Byte> retArray = stackalloc byte[6];
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(retArray);
            var x = BinaryPrimitives.ReadInt16LittleEndian(retArray);
            var y = BinaryPrimitives.ReadInt16LittleEndian(retArray.Slice(2));
            var z = BinaryPrimitives.ReadInt16LittleEndian(retArray.Slice(4));
            return new Vector3(x, y, z);
        }

        private void SetConfigMode(bool mode)
        {
            if (mode)
            {
                SetOperationMode(OperationMode.Config);
            }
            else
            {
                SetOperationMode(_operationMode);
            }
        }

        public void Dispose()
        {
            if (_autoDispoable)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }

        private void WriteReg(Registers reg, byte param)
        {
            _i2cDevice.Write(new byte[] { (byte)reg, param });
        }

        private byte ReadByte(Registers reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        private Int16 ReadInt16(Registers reg)
        {
            Span<byte> retArray = stackalloc byte[2];
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(retArray);
            return BinaryPrimitives.ReadInt16LittleEndian(retArray);
        }

    }
}
