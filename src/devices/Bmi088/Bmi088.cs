using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;

namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Based on the C++ implementation on https://github.com/Seeed-Studio/Grove_6Axis_Accelerometer_And_Gyroscope_BMI088
    /// Product page: https://www.bosch-sensortec.com/products/motion-sensors/imus/bmi088.html
    /// Data sheet: https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmi088-ds001.pdf
    /// </summary>
    public class Bmi088 : IDisposable
    {
        /// <summary>
        /// Default i2c address for the accelerometer
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
        private AccelerometerScaleRange _accelerometerScaleRange;
        private GyroscopeScaleRange _gyroscopeScaleRange;
        private AccelerometerPowerMode _accelerometerPowerMode;
        private GyroscopePowerMode _gyroscopePowerMode;
        private AccelerometerOutputDataRate _accelerometerOutputDataRate;
        private GyroscopeOutputDataRate _gyroscopeOutputDataRate;

        /// <summary>
        /// Initialize on default bus (1) with default I2C addresses.
        /// </summary>
        public Bmi088()
            : this(I2cDevice.Create(new I2cConnectionSettings(1, DEFAULT_ACCELEROMETER_ADDRESS)), I2cDevice.Create(new I2cConnectionSettings(1, DEFAULT_GYROSCOPE_ADDRESS)))
        {
        }

        /// <summary>
        /// Initialize using user provided i2c devices for accelerometer and gyroscope
        /// </summary>
        /// <param name="i2cDeviceAccelerometer">i2c device for accelerometer</param>
        /// <param name="i2cDeviceGyroscope">i2c device for gyroscope</param>
        public Bmi088(I2cDevice i2cDeviceAccelerometer, I2cDevice i2cDeviceGyroscope)
        {
            _i2cAccelerometer = i2cDeviceAccelerometer;
            _i2cGyroscope = i2cDeviceGyroscope;

            AccelerometerScaleRange = AccelerometerScaleRange.Range6g;
            AccelerometerOutputDataRate = AccelerometerOutputDataRate.Odr100;
            AccelerometerPowerMode = AccelerometerPowerMode.Active;

            GyroscopeScaleRange = GyroscopeScaleRange.Range2000;
            GyroscopeOutputDataRate = GyroscopeOutputDataRate.Odr2000_Bw532;
            GyroscopePowerMode = GyroscopePowerMode.Normal;
        }

        /// <summary>
        /// Gets a reading from the accelerometer
        /// </summary>
        /// <returns>Vector3 with values in mg (milli gravities)</returns>
        public Vector3 GetAccelerometer()
        {
            var reading = ReadVector3(_i2cAccelerometer, (byte)AccelerometerRegister.XLsb);

            return Vector3.Multiply(reading, _accelerometerMultiplier);
        }

        /// <summary>
        /// Gets a reading from the gyroscope
        /// </summary>
        /// <returns>Vector3 with the readings in degrees/second</returns>
        public Vector3 GetGyroscope()
        {
            var reading = ReadVector3(_i2cGyroscope, (byte)GyroscopeRegister.RateXLsb);

            return Vector3.Multiply(reading, _gyroscopeMultiplier);
        }

        /// <summary>
        /// Gets or sets the sensitivity of the accelerometer (3, 6, 12 or 24g)
        /// </summary>
        public AccelerometerScaleRange AccelerometerScaleRange
        {
            get => _accelerometerScaleRange;

            set
            {
                if (value == AccelerometerScaleRange.Range3g)
                {
                    _accelerometerMultiplier = 3000 / 32768f;
                }
                else if (value == AccelerometerScaleRange.Range6g)
                {
                    _accelerometerMultiplier = 6000 / 32768f;
                }
                else if (value == AccelerometerScaleRange.Range12g)
                {
                    _accelerometerMultiplier = 12000 / 32768f;
                }
                else if (value == AccelerometerScaleRange.Range24g)
                {
                    _accelerometerMultiplier = 24000 / 32768f;
                }

                WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.Range, (byte)value);
                _accelerometerScaleRange = value;
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity of the gyroscope (125, 250, 500, 1000 or 2000 degrees/second)
        /// </summary>
        public GyroscopeScaleRange GyroscopeScaleRange
        {
            get => _gyroscopeScaleRange;

            set
            {
                if (value == GyroscopeScaleRange.Range2000)
                {
                    _gyroscopeMultiplier = 2000 / 32768f;
                }
                else if (value == GyroscopeScaleRange.Range1000)
                {
                    _gyroscopeMultiplier = 1000 / 32768f;
                }
                else if (value == GyroscopeScaleRange.Range500)
                {
                    _gyroscopeMultiplier = 500 / 32768f;
                }
                else if (value == GyroscopeScaleRange.Range250)
                {
                    _gyroscopeMultiplier = 250 / 32768f;
                }
                else if (value == GyroscopeScaleRange.Range125)
                {
                    _gyroscopeMultiplier = 125 / 32768f;
                }

                WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.Range, (byte)value);
                _gyroscopeScaleRange = value;
            }
        }

        /// <summary>
        /// Resets the accelerometer
        /// </summary>
        protected void ResetAccelerometer()
        {
            WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.SoftReset, 0xB6);
        }

        /// <summary>
        /// Resets the gyroscope
        /// </summary>
        protected void ResetGyroscope()
        {
            WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.SoftReset, 0xB6);
        }

        /// <summary>
        /// Id of the accelerometer
        /// </summary>
        /// <returns></returns>
        protected byte GetAccelerometerId()
        {
            return ReadByte(_i2cAccelerometer, (byte)GyroscopeRegister.ChipId);
        }

        /// <summary>
        /// Id of the gyroscope
        /// </summary>
        /// <returns></returns>
        protected byte GetGyroscopeId()
        {
            return ReadByte(_i2cGyroscope, (byte)GyroscopeRegister.ChipId);
        }

        /// <summary>
        /// Gets or sets power mode of the accelerometer (Active or Suspend), see datasheet
        /// </summary>
        protected AccelerometerPowerMode AccelerometerPowerMode
        {
            get => _accelerometerPowerMode;

            set
            {
                if (value == AccelerometerPowerMode.Active)
                {
                    WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PwrConf, 0x00);
                    WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PwrCtrl, 0x04);
                }
                else if (value == AccelerometerPowerMode.Suspend)
                {
                    WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PwrConf, 0x03);
                    WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.PwrCtrl, 0x00);
                }

                _accelerometerPowerMode = value;
            }
        }

        /// <summary>
        /// Gets or sets power mode of the gyroscope (Normal, Suspend or Deep suspend), see datasheet
        /// </summary>
        protected GyroscopePowerMode GyroscopePowerMode
        {
            get => _gyroscopePowerMode;

            set
            {
                WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.Lmp1, (byte)value);
                _gyroscopePowerMode = value;
            }
        }

        /// <summary>
        /// Gets or sets data rate of the accelerometer, see datasheet
        /// </summary>
        protected AccelerometerOutputDataRate AccelerometerOutputDataRate
        {
            get => _accelerometerOutputDataRate;

            set
            {
                var data = ReadByte(_i2cAccelerometer, (byte)AccelerometerRegister.Conf);
                data = (byte)(data & 0xf0);
                data = (byte)(data | (byte)value);

                WriteByte(_i2cAccelerometer, (byte)AccelerometerRegister.Conf, data);
                _accelerometerOutputDataRate = value;
            }
        }

        /// <summary>
        /// Gets or sets data rate of the gyroscope, see datasheet
        /// </summary>
        protected GyroscopeOutputDataRate GyroscopeOutputDataRate
        {
            get => _gyroscopeOutputDataRate;

            set
            {
                WriteByte(_i2cGyroscope, (byte)GyroscopeRegister.Bandwidth, (byte)value);
                _gyroscopeOutputDataRate = value;
            }
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
                reg,
                val
            };

            i2c.Write(buff);
        }
    }
}
