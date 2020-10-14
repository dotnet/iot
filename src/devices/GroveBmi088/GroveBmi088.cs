using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;

namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Based on the C++ implementation on https://github.com/Seeed-Studio/Grove_6Axis_Accelerometer_And_Gyroscope_BMI088
    /// Product page: https://www.seeedstudio.com/Grove-6-Axis-Accelerometer-Gyroscope-BMI088.html
    /// Data sheet: https://github.com/SeeedDocument/Grove-6-Axis_Accelerometer-Gyroscope-BMI088/raw/master/res/BMI088.pdf
    /// </summary>
    public class GroveBmi088 : IDisposable
    {
        /// <summary>
        /// Default i2c address for theaccelerometer
        /// </summary>
        public const byte DEFAULT_ACCELEROMETER_ADDRESS = 0x19;

        /// <summary>
        /// Default i2c address for the gyroscope
        /// </summary>
        public const byte DEFAULT_GYROSCOPE_ADDRESS = 0x69;

        private const byte ReadMask = 0x80;

        private readonly I2cDevice _i2cAccelerometer;
        private readonly I2cDevice _i2cGyroscope;

        private float _accelerometerMultiplier;
        private float _gyroscopeMultiplier;

        /// <summary>
        /// Initialize on default bus (1) with default I2C addresses.
        /// </summary>
        public GroveBmi088()
            : this(I2cDevice.Create(new I2cConnectionSettings(1, DEFAULT_ACCELEROMETER_ADDRESS)), I2cDevice.Create(new I2cConnectionSettings(1, DEFAULT_GYROSCOPE_ADDRESS)))
        {
        }

        /// <summary>
        /// Initialize using user provided i2c devices for accelerometer and gyroscope
        /// </summary>
        /// <param name="i2cDeviceAccelerometer">i2c device for accelerometer</param>
        /// <param name="i2cDeviceGyroscope">i2c device for gyroscope</param>
        public GroveBmi088(I2cDevice i2cDeviceAccelerometer, I2cDevice i2cDeviceGyroscope)
        {
            _i2cAccelerometer = i2cDeviceAccelerometer;
            _i2cGyroscope = i2cDeviceGyroscope;

            SetAccelerometerScaleRange(AccelerometerMeasurementRange.RANGE_6G);
            SetAccelerometerOutputDataRate(AccelerometerOutputDataRate.ODR_100);
            SetAccelerometerPowerMode(AccelerometerPowerMode.ACC_ACTIVE);

            SetGyroscopeScaleRange(GyroscopeScaleRange.RANGE_2000);
            SetGyroscopeOutputDataRate(GyroscopeOutputDataRate.ODR_2000_BW_532);
            SetGyroscopePowerMode(GyroscopePowerMode.GYRO_NORMAL);
        }

        /// <summary>
        /// Gets a reading from the accelerometer
        /// </summary>
        /// <returns>Vector3 with values in mg (milli gravities)</returns>
        public Vector3 GetAccelerometer()
        {
            var reading = ReadVector3(_i2cAccelerometer, (byte)AccelerometerRegister.X_LSB);

            return Vector3.Multiply(reading, _accelerometerMultiplier);
        }

        /// <summary>
        /// Gets a reading from the gyroscope
        /// </summary>
        /// <returns>Vector3 with the readings in degrees/second</returns>
        public Vector3 GetGyroscope()
        {
            var reading = ReadVector3(_i2cGyroscope, (byte)GyroscopeRegister.RATE_X_LSB);

            return Vector3.Multiply(reading, _gyroscopeMultiplier);
        }

        /// <summary>
        /// Sets the sensitivity of the accelerometer
        /// </summary>
        /// <param name="range">3, 6, 12 or 24g</param>
        public void SetAccelerometerScaleRange(AccelerometerMeasurementRange range)
        {
            if (range == AccelerometerMeasurementRange.RANGE_3G)
            {
                _accelerometerMultiplier = 3000 / 32768;
            }
            else if (range == AccelerometerMeasurementRange.RANGE_6G)
            {
                _accelerometerMultiplier = 6000 / 32768;
            }
            else if (range == AccelerometerMeasurementRange.RANGE_12G)
            {
                _accelerometerMultiplier = 12000 / 32768;
            }
            else if (range == AccelerometerMeasurementRange.RANGE_24G)
            {
                _accelerometerMultiplier = 24000 / 32768;
            }

            WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.RANGE, (byte)range);
        }

        /// <summary>
        /// Sets the sensitivity of the gyroscope
        /// </summary>
        /// <param name="range">125, 250, 500, 1000 or 2000 degrees/second</param>
        public void SetGyroscopeScaleRange(GyroscopeScaleRange range)
        {
            if (range == GyroscopeScaleRange.RANGE_2000)
            {
                _gyroscopeMultiplier = 2000 / 32768;
            }
            else if (range == GyroscopeScaleRange.RANGE_1000)
            {
                _gyroscopeMultiplier = 1000 / 32768;
            }
            else if (range == GyroscopeScaleRange.RANGE_500)
            {
                _gyroscopeMultiplier = 500 / 32768;
            }
            else if (range == GyroscopeScaleRange.RANGE_250)
            {
                _gyroscopeMultiplier = 250 / 32768;
            }
            else if (range == GyroscopeScaleRange.RANGE_125)
            {
                _gyroscopeMultiplier = 125 / 32768;
            }

            WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.RANGE, (byte)range);
        }

        /// <summary>
        /// Resets the accelerometer
        /// </summary>
        protected void ResetAccelerometer()
        {
            WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.SOFT_RESET, 0xB6);
        }

        /// <summary>
        /// Resets the gyroscope
        /// </summary>
        protected void ResetGyroscope()
        {
            WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.SOFT_RESET, 0xB6);
        }

        /// <summary>
        /// Id of the accelerometer
        /// </summary>
        /// <returns></returns>
        protected byte GetAccelerometerId()
        {
            return ReadByte(_i2cAccelerometer, (byte)GyroscopeRegister.CHIP_ID);
        }

        /// <summary>
        /// Id of the gyroscope
        /// </summary>
        /// <returns></returns>
        protected byte GetGyroscopeId()
        {
            return ReadByte(_i2cGyroscope, (byte)GyroscopeRegister.CHIP_ID);
        }

        /// <summary>
        /// Sets power mode of the accelerometer, see datasheet
        /// </summary>
        /// <param name="mode">Active or Suspend</param>
        protected void SetAccelerometerPowerMode(AccelerometerPowerMode mode)
        {
            if (mode == AccelerometerPowerMode.ACC_ACTIVE)
            {
                WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PWR_CTRl, 0x04);
                WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PWR_CONF, 0x00);
            }
            else if (mode == AccelerometerPowerMode.ACC_SUSPEND)
            {
                WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PWR_CONF, 0x03);
                WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PWR_CTRl, 0x00);
            }
        }

        /// <summary>
        /// Sets power mode of the gyroscope, see datasheet
        /// </summary>
        /// <param name="mode">Normal, Suspend or Deep suspend</param>
        protected void SetGyroscopePowerMode(GyroscopePowerMode mode)
        {
            if (mode == GyroscopePowerMode.GYRO_NORMAL)
            {
                WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.LPM_1, (byte)GyroscopePowerMode.GYRO_NORMAL);
            }
            else if (mode == GyroscopePowerMode.GYRO_SUSPEND)
            {
                WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.LPM_1, (byte)GyroscopePowerMode.GYRO_SUSPEND);
            }
            else if (mode == GyroscopePowerMode.GYRO_DEEP_SUSPEND)
            {
                WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.LPM_1, (byte)GyroscopePowerMode.GYRO_DEEP_SUSPEND);
            }
        }

        /// <summary>
        /// Set data rate of the accelerometer, see datasheet
        /// </summary>
        /// <param name="odr">data rate</param>
        protected void SetAccelerometerOutputDataRate(AccelerometerOutputDataRate odr)
        {
            var data = ReadByte(_i2cAccelerometer, (byte)AccelerometerRegister.CONF);
            data = (byte)(data & 0xf0);
            data = (byte)(data | (byte)odr);

            WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.CONF, data);
        }

        /// <summary>
        /// Set data rate of the gyroscope, see datasheet
        /// </summary>
        /// <param name="odr">data rate</param>
        protected void SetGyroscopeOutputDataRate(GyroscopeOutputDataRate odr)
        {
            WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.BAND_WIDTH, (byte)odr);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cAccelerometer?.Dispose();
            _i2cGyroscope?.Dispose();
        }

        private Vector3 ReadVector3(I2cDevice i2cDevice, byte register)
        {
            Span<byte> vec = stackalloc byte[6];
            ReadBuffer(i2cDevice, register, vec);

            short x = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(4, 2));

            return new Vector3(x, y, z);
        }

        private void ReadBuffer(I2cDevice i2cDevice, byte register, Span<byte> buffer)
        {
            i2cDevice.WriteByte((byte)((byte)register | ReadMask));
            i2cDevice.Read(buffer);
        }

        private byte ReadByte(I2cDevice i2cDevice, byte register)
        {
            i2cDevice.WriteByte((byte)((byte)register | ReadMask));

            return i2cDevice.ReadByte();
        }

        private void WriteByte(I2cDevice i2c, byte reg, byte val)
        {
            Span<byte> buff = stackalloc byte[2]
            {
                (byte)reg,
                val
            };

            i2c.Write(buff);
        }
    }
}
