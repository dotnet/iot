// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Magnetometer;
using Iot.Units;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Iot.Device.Imu
{
    /// <summary>
    ///  MPU9250 class. MPU6500 has an embedded gyroscope, accelerometer and temperature.
    /// </summary>
    public class Mpu6500 : IDisposable
    {
        /// <summary>
        /// Default address for MPU9250
        /// </summary>
        public const byte DefaultI2cAddress = 0x68;

        /// <summary>
        /// Second address for MPU9250
        /// </summary>
        public const byte SecondI2cAddress = 0x69;

        private const float Adc = 0x8000;
        private const float Gravity = 9.807f;
        internal I2cDevice _i2cDevice;
        private Vector3 _accelerometerBias = new Vector3();
        private Vector3 _gyroscopeBias = new Vector3();
        private AccelerometerRange _accelerometerRange;
        private GyroscopeRange _gyroscopeRange;
        private AccelerometerBandwidth _accelerometerBandwidth;
        private GyroscopeBandwidth _gyroscopeBandwidth;
        internal bool _wakeOnMotion;

        /// <summary>
        /// Initialize the MPU6500
        /// </summary>
        /// <param name="i2cDevice">The I2C device</param>
        public Mpu6500(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            Reset();
            PowerOn();
            if (!CheckVersion())
                throw new IOException($"This device does not contain the correct signature 0x70 for a MPU6500");
            GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0250Hz;
            GyroscopeRange = GyroscopeRange.Range0250Dps;
            AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth1130Hz;
            AccelerometerRange = AccelerometerRange.Range02G;
        }

        /// <summary>
        /// Used to create the class for the MPU9250. Initialization is a bit different than for the MPU65000
        /// </summary>
        internal Mpu6500()
        { }

        #region Accelerometer

        /// <summary>
        /// Accelerometer bias data
        /// </summary>
        public Vector3 AccelerometerBias => _accelerometerBias;

        /// <summary>
        /// Get or set the accelerometer range
        /// </summary>
        public AccelerometerRange AccelerometerRange
        {
            get => _accelerometerRange;

            set
            {
                WriteRegister(Register.ACCEL_CONFIG, (byte)((byte)value << 3));
                // We will cache the range to avoid i2c access every time this data is requested
                // This allow as well to make sure the stored data is the same as the one read
                _accelerometerRange = (AccelerometerRange)(ReadByte(Register.ACCEL_CONFIG) >> 3);
                if (_accelerometerRange != value)
                    throw new IOException($"Can set {nameof(AccelerometerRange)}, desired value {value}, stored value {_accelerometerRange}");
            }
        }

        /// <summary>
        /// Get or set the accelerometer bandwidth
        /// </summary>
        public AccelerometerBandwidth AccelerometerBandwidth
        {
            get => _accelerometerBandwidth;

            set
            {
                WriteRegister(Register.ACCEL_CONFIG_2, (byte)value);
                _accelerometerBandwidth = (AccelerometerBandwidth)ReadByte(Register.ACCEL_CONFIG_2);
                if (_accelerometerBandwidth != value)
                    throw new IOException($"Can set {nameof(AccelerometerBandwidth)}, desired value {value}, stored value {_accelerometerBandwidth}");
            }
        }

        /// <summary>
        /// Get the real accelerometer bandwidth. This allows to calculate the real
        /// degree per second
        /// </summary>
        public float AccelerationScale
        {
            get
            {
                float val = 0;
                switch (AccelerometerRange)
                {
                    case AccelerometerRange.Range02G:
                        val = 2.0f;
                        break;
                    case AccelerometerRange.Range04G:
                        val = 4.0f;
                        break;
                    case AccelerometerRange.Range08G:
                        val = 8.0f;
                        break;
                    case AccelerometerRange.Range16G:
                        val = 16.0f;
                        break;
                    default:
                        break;
                }

                val = (val * Gravity) / Adc;
                return val / (1 + SampleRateDivider);
            }
        }

        /// <summary>
        /// Get the accelerometer in G
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///    +Z   +Y
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///         +X
        /// </remarks>
        public Vector3 GetAccelerometer() => GetRawAccelerometer() * AccelerationScale;

        private Vector3 GetRawAccelerometer()
        {
            Span<byte> rawData = stackalloc byte[6] { 0, 0, 0, 0, 0, 0 };
            Vector3 ace = new Vector3();
            ReadBytes(Register.ACCEL_XOUT_H, rawData);
            ace.X = BinaryPrimitives.ReadInt16BigEndian(rawData);
            ace.Y = BinaryPrimitives.ReadInt16BigEndian(rawData.Slice(2));
            ace.Z = BinaryPrimitives.ReadInt16BigEndian(rawData.Slice(4));
            return ace;
        }

        /// <summary>
        /// Set or get the accelerometer low power mode
        /// </summary>
        public AccelerometerLowPowerFrequency AccelerometerLowPowerFrequency
        {
            get { return (AccelerometerLowPowerFrequency)ReadByte(Register.LP_ACCEL_ODR); }
            set { WriteRegister(Register.LP_ACCEL_ODR, (byte)value); }
        }

        #endregion

        #region Gyroscope

        /// <summary>
        /// Gyroscope bias data
        /// </summary>
        public Vector3 GyroscopeBias => _gyroscopeBias;

        /// <summary>
        /// Get or set the gyroscope range
        /// </summary>
        public GyroscopeRange GyroscopeRange
        {
            get => _gyroscopeRange;

            set
            {
                WriteRegister(Register.GYRO_CONFIG, (byte)((byte)value << 3));
                _gyroscopeRange = (GyroscopeRange)(ReadByte(Register.GYRO_CONFIG) >> 3);
                if (_gyroscopeRange != value)
                    throw new IOException($"Can set {nameof(GyroscopeRange)}, desired value {value}, stored value {_gyroscopeRange}");
            }
        }

        /// <summary>
        /// Get or set the gyroscope bandwidth
        /// </summary>
        public GyroscopeBandwidth GyroscopeBandwidth
        {
            get => _gyroscopeBandwidth;

            set
            {
                if (value == GyroscopeBandwidth.Bandwidth8800HzFS32)
                {
                    WriteRegister(Register.GYRO_CONFIG, (byte)((byte)GyroscopeRange | 0x01));
                }
                else if (value == GyroscopeBandwidth.Bandwidth3600HzFS32)
                {
                    WriteRegister(Register.GYRO_CONFIG, (byte)((byte)GyroscopeRange | 0x02));
                }
                else
                {
                    WriteRegister(Register.GYRO_CONFIG, (byte)GyroscopeRange);
                    WriteRegister(Register.CONFIG, (byte)value);
                }

                var retConf = ReadByte(Register.GYRO_CONFIG);
                if ((retConf & 0x01) == 0x01)
                    _gyroscopeBandwidth = GyroscopeBandwidth.Bandwidth8800HzFS32;
                else if ((retConf & 0x03) == 0x00)
                    _gyroscopeBandwidth = (GyroscopeBandwidth)ReadByte(Register.CONFIG);
                else
                    _gyroscopeBandwidth = GyroscopeBandwidth.Bandwidth3600HzFS32;
                if (_gyroscopeBandwidth != value)
                    throw new IOException($"Can set {nameof(GyroscopeBandwidth)}, desired value {value}, stored value {_gyroscopeBandwidth}");
            }
        }

        /// <summary>
        /// Get the real gyroscope bandwidth. This allows to calculate the real
        /// angular rate in degree per second
        /// </summary>
        public float GyroscopeScale
        {
            get
            {
                float val = 0;
                switch (GyroscopeRange)
                {
                    case GyroscopeRange.Range0250Dps:
                        val = 250.0f;
                        break;
                    case GyroscopeRange.Range0500Dps:
                        val = 500.0f;
                        break;
                    case GyroscopeRange.Range1000Dps:
                        val = 1000.0f;
                        break;
                    case GyroscopeRange.Range2000Dps:
                        val = 2000.0f;
                        break;
                    default:
                        break;
                }

                val /= Adc;
                // the sample rate diver only apply for the non FS modes
                if ((GyroscopeBandwidth != GyroscopeBandwidth.Bandwidth3600HzFS32) &&
                    (GyroscopeBandwidth != GyroscopeBandwidth.Bandwidth8800HzFS32))
                    return val / (1 + SampleRateDivider);
                return val;
            }
        }

        /// <summary>
        /// Get the gyroscope in degrees per seconds
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///    +Z   +Y
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///         +X
        /// </remarks>
        public Vector3 GetGyroscopeReading() => GetRawGyroscope() * GyroscopeScale;

        private Vector3 GetRawGyroscope()
        {
            Span<byte> rawData = stackalloc byte[6] { 0, 0, 0, 0, 0, 0 };
            Vector3 gyro = new Vector3();
            ReadBytes(Register.GYRO_XOUT_H, rawData);
            gyro.X = BinaryPrimitives.ReadInt16BigEndian(rawData);
            gyro.Y = BinaryPrimitives.ReadInt16BigEndian(rawData.Slice(2));
            gyro.Z = BinaryPrimitives.ReadInt16BigEndian(rawData.Slice(4));
            return gyro;
        }

        #endregion

        #region Temperature

        /// <summary>
        /// Get the temperature
        /// </summary>
        public Temperature GetTemperature()
        {
            Span<byte> rawData = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.TEMP_OUT_H, rawData);
            // formula from the documentation
            return Temperature.FromCelsius((BinaryPrimitives.ReadInt16BigEndian(rawData) - 21) / 333.87 + 21);
        }

        #endregion

        #region Modes, constructor, Dispose

        /// <summary>
        /// Setup the Wake On Motion. This mode generate a rising signal on pin INT
        /// You can catch it with a normal GPIO and place an interruption on it if supported
        /// Reading the sensor won't give any value until it wakes up periodically
        /// Only Accelerator data is available in this mode
        /// </summary>
        /// <param name="accelerometerThreshold">Threshold of magnetometer x/y/z axes. LSB = 4mg. Range is 0mg to 1020mg</param>
        /// <param name="acceleratorLowPower">Frequency used to measure data for the low power consumption mode</param>
        public void SetWakeOnMotion(uint accelerometerThreshold, AccelerometerLowPowerFrequency acceleratorLowPower)
        {
            // Using documentation page 31 of Product Specification to setup
            _wakeOnMotion = true;
            if (accelerometerThreshold > 1020)
                throw new ArgumentException($"{nameof(accelerometerThreshold)} has to be between 0mg and 1020mg");
            // LSB = 4mg
            accelerometerThreshold /= 4;
            // Make sure we start from a clean soft reset
            PowerOn();
            //  PWR_MGMT_1 (0x6B) make CYCLE =0, SLEEP = 0  and STANDBY = 0 
            WriteRegister(Register.PWR_MGMT_1, (byte)ClockSource.Internal20MHz);
            // PWR_MGMT_2 (0x6C) set DIS_XA, DIS_YA, DIS_ZA = 0 and DIS_XG, DIS_YG, DIS_ZG = 1
            // Remove the Gyroscope
            WriteRegister(Register.PWR_MGMT_2, (byte)(DisableModes.DisableGyroscopeX | DisableModes.DisableGyroscopeY | DisableModes.DisableGyroscopeZ));
            // ACCEL_CONFIG 2 (0x1D) set ACCEL_FCHOICE_B = 0 and A_DLPFCFG[2:0]=1(b001)
            // Bandwidth for Accelerator to 184Hz
            AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth0184Hz;
            // Enable Motion Interrupt
            //  In INT_ENABLE (0x38), set the whole register to 0x40 to enable motion interrupt only
            WriteRegister(Register.INT_ENABLE, 0x40);
            // Enable AccelHardware Intelligence:
            // In MOT_DETECT_CTRL (0x69), set ACCEL_INTEL_EN = 1 and ACCEL_INTEL_MODE  = 1
            WriteRegister(Register.MOT_DETECT_CTRL, 0b1100_0000);
            // Set Motion Threshold:
            // In WOM_THR (0x1F), set the WOM_Threshold[7:0] to 1~255 LSBs (0~1020mg)
            WriteRegister(Register.WOM_THR, (byte)accelerometerThreshold);
            // Set Frequency of Wake-up:
            // In LP_ACCEL_ODR (0x1E), set Lposc_clksel[3:0] = 0.24Hz ~ 500Hz
            WriteRegister(Register.LP_ACCEL_ODR, (byte)acceleratorLowPower);
            // Enable Cycle Mode (AccelLow Power Mode):
            // In PWR_MGMT_1 (0x6B) make CYCLE =1
            WriteRegister(Register.PWR_MGMT_1, 0b0010_0000);
            // Motion Interrupt Configuration Completed
        }

        internal void Reset()
        {
            WriteRegister(Register.PWR_MGMT_1, 0x80);
            // http://www.invensense.com/wp-content/uploads/2015/02/PS-MPU-9250A-01-v1.1.pdf, section 4.23.
            // Maximum risen time is 100 ms after VDD
            Thread.Sleep(100);
            _wakeOnMotion = false;
        }

        internal void PowerOn()
        {
            // this should be a soft reset
            WriteRegister(Register.PWR_MGMT_1, 0x01);
            // http://www.invensense.com/wp-content/uploads/2015/02/PS-MPU-9250A-01-v1.1.pdf, section 4.23.
            // Maximum risen time is 100 ms after VDD
            Thread.Sleep(100);
            _wakeOnMotion = false;
        }

        /// <summary>
        /// Return true if the version of MPU6500 is the correct one
        /// </summary>
        /// <returns>True if success</returns>
        internal bool CheckVersion()
        {
            // Check if the version is thee correct one
            return ReadByte(Register.WHO_AM_I) == 0x70;
        }

        /// <summary>
        /// Get or set the sample diver mode
        /// </summary>
        public byte SampleRateDivider
        {
            get { return ReadByte(Register.SMPLRT_DIV); }
            set { WriteRegister(Register.SMPLRT_DIV, value); }
        }

        /// <summary>
        /// Get or set the elements to disable. 
        /// It can be any axes of the accelerometer and or the gyroscope
        /// </summary>
        public DisableModes DisableModes
        {
            get { return (DisableModes)ReadByte(Register.PWR_MGMT_2); }
            set { WriteRegister(Register.PWR_MGMT_2, (byte)value); }
        }

        #endregion

        #region FIFO

        /// <summary>
        /// Get the number of elements to read from the FIFO (First In First Out) buffer
        /// </summary>
        public uint FifoCount
        {
            get
            {
                Span<byte> rawData = stackalloc byte[2] { 0, 0 };
                ReadBytes(Register.FIFO_COUNTH, rawData);
                return BinaryPrimitives.ReadUInt16BigEndian(rawData);
            }
        }

        /// <summary>
        /// Get or set the FIFO (First In First Out) modes
        /// </summary>
        public FifoModes FifoModes
        {
            get { return (FifoModes)(ReadByte(Register.FIFO_EN)); }
            set
            {
                if (value != FifoModes.None)
                {
                    // Make sure the FIFO is enabled
                    var usrCtl = (UserControls)ReadByte(Register.USER_CTRL);
                    usrCtl = usrCtl | UserControls.FIFO_RST;
                    WriteRegister(Register.USER_CTRL, (byte)usrCtl);
                }
                else
                {
                    // Deactivate FIFO
                    var usrCtl = (UserControls)ReadByte(Register.USER_CTRL);
                    usrCtl = usrCtl & ~UserControls.FIFO_RST;
                    WriteRegister(Register.USER_CTRL, (byte)usrCtl);
                }
                WriteRegister(Register.FIFO_EN, (byte)value);
            }
        }

        /// <summary>
        /// Read data in the FIFO (First In First Out) buffer, read as many data as the size of readData byte span
        /// You should read the number of data available in the FifoCount property then
        /// read them here.
        /// You will read only data you have selected in FifoModes.
        /// Data are in the order of the Register from 0x3B to 0x60.
        /// ACCEL_XOUT_H and ACCEL_XOUT_L
        /// ACCEL_YOUT_H and ACCEL_YOUT_L 
        /// ACCEL_ZOUT_H and ACCEL_ZOUT_L
        /// TEMP_OUT_H and TEMP_OUT_L
        /// GYRO_XOUT_H and GYRO_XOUT_L
        /// GYRO_YOUT_H and GYRO_YOUT_L
        /// GYRO_ZOUT_H and GYRO_ZOUT_L
        /// EXT_SENS_DATA_00 to EXT_SENS_DATA_24
        /// </summary>
        /// <param name="readData">Data which will be read</param>
        public void ReadFifo(Span<byte> readData)
        {
            ReadBytes(Register.FIFO_R_W, readData);
        }

        #endregion

        #region Calibration and tests

        /// <summary>
        /// Perform full calibration the gyroscope and the accelerometer
        /// It will automatically adjust as well the offset stored in the device
        /// The result bias will be stored in the AcceloremeterBias and GyroscopeBias
        /// </summary>
        /// <returns>Gyroscope and accelerometer bias</returns>
        public (Vector3 gyroscopeBias, Vector3 accelerometerBias) CalibrateGyroscopeAccelerometer()
        {
            // = 131 LSB/degrees/sec
            const int GyroSensitivity = 131;
            // = 16384 LSB/g
            const int AccSensitivity = 16384;
            byte i2cMaster;
            byte userControls;

            Span<byte> rawData = stackalloc byte[12];

            Vector3 gyroBias = new Vector3();
            Vector3 acceBias = new Vector3();

            Reset();
            // Enable accelerator and gyroscope
            DisableModes = DisableModes.DisableNone;
            Thread.Sleep(200);
            // Disable all interrupts
            WriteRegister(Register.INT_ENABLE, 0x00);
            // Disable FIFO
            FifoModes = FifoModes.None;
            // Disable I2C master
            i2cMaster = ReadByte(Register.I2C_MST_CTRL);
            WriteRegister(Register.I2C_MST_CTRL, 0x00);
            // Disable FIFO and I2C master modes
            userControls = ReadByte(Register.USER_CTRL);
            WriteRegister(Register.USER_CTRL, (byte)UserControls.None);
            // Reset FIFO and DMP
            WriteRegister(Register.USER_CTRL, (byte)UserControls.FIFO_RST);
            DelayHelper.DelayMilliseconds(15, false);

            // Configure MPU6050 gyro and accelerometer for bias calculation
            // Set low-pass filter to 184 Hz
            GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0184Hz;
            AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth0184Hz;
            // Set sample rate to 1 kHz
            SampleRateDivider = 0;
            // Set gyro to maximum sensitivity
            GyroscopeRange = GyroscopeRange.Range0250Dps;
            AccelerometerRange = AccelerometerRange.Range02G;

            // Configure FIFO will be needed for bias calculation
            FifoModes = FifoModes.GyroscopeX | FifoModes.GyroscopeY | FifoModes.GyroscopeZ | FifoModes.Accelerometer;
            // accumulate 40 samples in 40 milliseconds = 480 bytes
            // Do not exceed 512 bytes max buffer
            DelayHelper.DelayMilliseconds(40, false);
            // We have our data, deactivate FIFO
            FifoModes = FifoModes.None;

            // How many sets of full gyro and accelerometer data for averaging
            var packetCount = FifoCount / 12;

            for (uint reading = 0; reading < packetCount; reading++)
            {
                Vector3 accel_temp = new Vector3();
                Vector3 gyro_temp = new Vector3();

                // Read data
                ReadBytes(Register.FIFO_R_W, rawData);

                // Form signed 16-bit integer for each sample in FIFO
                accel_temp.X = BinaryPrimitives.ReadUInt16BigEndian(rawData);
                accel_temp.Y = BinaryPrimitives.ReadUInt16BigEndian(rawData.Slice(2));
                accel_temp.Z = BinaryPrimitives.ReadUInt16BigEndian(rawData.Slice(4));
                gyro_temp.X = BinaryPrimitives.ReadUInt16BigEndian(rawData.Slice(6));
                gyro_temp.Y = BinaryPrimitives.ReadUInt16BigEndian(rawData.Slice(8));
                gyro_temp.Z = BinaryPrimitives.ReadUInt16BigEndian(rawData.Slice(10));

                acceBias += accel_temp;
                gyroBias += gyro_temp;
            }
            // Make the average
            acceBias /= packetCount;
            gyroBias /= packetCount;

            // bias on Z is cumulative
            acceBias.Z += acceBias.Z > 0 ? -AccSensitivity : AccSensitivity;

            // Divide by 4 to get 32.9 LSB per deg/s 
            // Biases are additive, so change sign on calculated average gyro biases
            rawData[0] = (byte)(((int)(-gyroBias.X / 4) >> 8) & 0xFF);
            rawData[1] = (byte)((int)(-gyroBias.X / 4) & 0xFF);
            rawData[2] = (byte)(((int)(-gyroBias.Y / 4) >> 8) & 0xFF);
            rawData[3] = (byte)((int)(-gyroBias.Y / 4) & 0xFF);
            rawData[4] = (byte)(((int)(-gyroBias.Z / 4) >> 8) & 0xFF);
            rawData[5] = (byte)((int)(-gyroBias.Z / 4) & 0xFF);

            // Changes all Gyroscope offsets
            WriteRegister(Register.XG_OFFSET_H, rawData[0]);
            WriteRegister(Register.XG_OFFSET_L, rawData[1]);
            WriteRegister(Register.YG_OFFSET_H, rawData[2]);
            WriteRegister(Register.YG_OFFSET_L, rawData[3]);
            WriteRegister(Register.ZG_OFFSET_H, rawData[4]);
            WriteRegister(Register.ZG_OFFSET_L, rawData[5]);

            // Output scaled gyro biases for display in the main program
            _gyroscopeBias = gyroBias / GyroSensitivity;

            // Construct the accelerometer biases for push to the hardware accelerometer
            // bias registers. These registers contain factory trim values which must be
            // added to the calculated accelerometer biases; on boot up these registers
            // will hold non-zero values. In addition, bit 0 of the lower byte must be
            // preserved since it is used for temperature compensation calculations.
            // Accelerometer bias registers expect bias input as 2048 LSB per g, so that
            // the accelerometer biases calculated above must be divided by 8.

            // A place to hold the factory accelerometer trim biases
            Vector3 accel_bias_reg = new Vector3();
            Span<byte> accData = stackalloc byte[2];
            // Read factory accelerometer trim values
            ReadBytes(Register.XA_OFFSET_H, accData);
            accel_bias_reg.X = BinaryPrimitives.ReadUInt16BigEndian(accData);
            ReadBytes(Register.YA_OFFSET_H, accData);
            accel_bias_reg.Y = BinaryPrimitives.ReadUInt16BigEndian(accData);
            ReadBytes(Register.ZA_OFFSET_H, accData);
            accel_bias_reg.Z = BinaryPrimitives.ReadUInt16BigEndian(accData);

            // Define mask for temperature compensation bit 0 of lower byte of
            // accelerometer bias registers

            uint mask = 0x01;
            // Define array to hold mask bit for each accelerometer bias axis
            Span<byte> mask_bit = stackalloc byte[3];

            // If temperature compensation bit is set, record that fact in mask_bit
            mask_bit[0] = (((uint)accel_bias_reg.X & mask) == mask) ? (byte)0x01 : (byte)0x00;
            mask_bit[1] = (((uint)accel_bias_reg.Y & mask) == mask) ? (byte)0x01 : (byte)0x00;
            mask_bit[2] = (((uint)accel_bias_reg.Z & mask) == mask) ? (byte)0x01 : (byte)0x00;

            // Construct total accelerometer bias, including calculated average
            // accelerometer bias from above
            // Subtract calculated averaged accelerometer bias scaled to 2048 LSB/g
            // (16 g full scale) and keep the mask
            accel_bias_reg -= acceBias / 8;
            // Add the "reserved" mask as it was
            rawData[0] = (byte)(((int)accel_bias_reg.X >> 8) & 0xFF);
            rawData[1] = (byte)(((int)accel_bias_reg.X & 0xFF) | mask_bit[0]);
            rawData[2] = (byte)(((int)accel_bias_reg.Y >> 8) & 0xFF);
            rawData[3] = (byte)(((int)accel_bias_reg.Y & 0xFF) | mask_bit[1]);
            rawData[4] = (byte)(((int)accel_bias_reg.Z >> 8) & 0xFF);
            rawData[5] = (byte)(((int)accel_bias_reg.Z & 0xFF) | mask_bit[2]);
            // Push accelerometer biases to hardware registers
            WriteRegister(Register.XA_OFFSET_H, rawData[0]);
            WriteRegister(Register.XA_OFFSET_L, rawData[1]);
            WriteRegister(Register.YA_OFFSET_H, rawData[2]);
            WriteRegister(Register.YA_OFFSET_L, rawData[3]);
            WriteRegister(Register.ZA_OFFSET_H, rawData[4]);
            WriteRegister(Register.ZA_OFFSET_L, rawData[5]);

            // Restore the previous modes
            WriteRegister(Register.USER_CTRL, (byte)(userControls | (byte)UserControls.I2C_MST_EN));
            i2cMaster = (byte)(i2cMaster & (~(byte)(I2cBusFrequency.Frequency348kHz) | (byte)I2cBusFrequency.Frequency400kHz));
            WriteRegister(Register.I2C_MST_CTRL, i2cMaster);
            DelayHelper.DelayMilliseconds(10, false);

            // Finally store the acceleration bias
            _accelerometerBias = acceBias / AccSensitivity;

            return (_gyroscopeBias, _accelerometerBias);
        }

        /// <summary>
        /// <![CDATA[
        /// Run a self test and returns the gyroscope and accelerometer vectores
        /// a. If factory Self-Test values ST_OTP≠0, compare the current Self-Test response (GXST, GYST, GZST, AXST, AYST and AZST) 
        /// to the factory Self-Test values (ST_OTP) and report Self-Test is passing if all the following criteria are fulfilled: 
        /// Axis    | Pass criteria 
        /// X-gyro  | (GXST / GXST_OTP) > 0.5 
        /// Y-gyro  | (GYST / GYST_OTP) > 0.5 
        /// Z-gyro  | (GZST / GZST_OTP) > 0.5
        /// X-Accel |  0.5 < (AXST / AXST_OTP) < 1.5
        /// Y-Accel | 0.5 < (AYST / AYST_OTP) < 1.5
        /// Z-Accel | 0.5 < (AZST / AZST_OTP) < 1.5
        /// b. If factory Self-Test values ST_OTP=0, compare the current Self-Test response (GXST, GYST, GZST, AXST, AYST and AZST) 
        /// to the ST absolute limits (ST_AL) and report Self-Test is passing if all the  following criteria are fulfilled.  
        /// Axis   | Pass criteria
        /// X-gyro | |GXST| ≥ 60dps  
        /// Y-gyro | |GYST| ≥ 60dps 
        /// Z-gyro | |GZST| ≥ 60dps 
        /// X-Accel| 225mgee ≤ |AXST| ≤ 675mgee
        /// Y-Accel| 225mgee ≤ |AXST| ≤ 675mgee
        /// Z-Accel| 225mgee ≤ |AXST| ≤ 675mgee
        /// c. If the Self-Test passes criteria (a) and (b), it’s necessary to check gyro offset values. 
        /// Report passing Self-Test if the following criteria fulfilled. 
        /// Axis   | Pass criteria 
        /// X-gyro | |GXOFFSET| ≤ 20dps
        /// Y-gyro | |GYOFFSET| ≤ 20dps
        /// Z-gyro | |GZOFFSET| ≤ 20dps
        /// ]]>
        /// </summary>
        /// <returns>the gyroscope and accelerometer vectors</returns>
        public (Vector3 gyroscopeAverage, Vector3 accelerometerAverage) RunGyroscopeAccelerometerSelfTest()
        {
            // Used for the number of cycles to run the test
            // Value is 200 according to documentation AN-MPU-9250A-03
            const int numCycles = 200;

            Vector3 accAverage = new Vector3();
            Vector3 gyroAvegage = new Vector3();
            Vector3 accSelfTestAverage = new Vector3();
            Vector3 gyroSelfTestAverage = new Vector3();
            Vector3 gyroSelfTest = new Vector3();
            Vector3 accSelfTest = new Vector3();
            Vector3 gyroFactoryTrim = new Vector3();
            Vector3 accFactoryTrim = new Vector3();
            // Tests done with slower GyroScale and Accelerator so 2G so value is 0 in both cases
            byte gyroAccScale = 0;

            // Setup the registers for Gyroscope as in documentation 
            // DLPF Config | LPF BW | Sampling Rate | Filter Delay 
            // 2           | 92Hz   | 1kHz          | 3.9ms 
            WriteRegister(Register.SMPLRT_DIV, 0x00);
            WriteRegister(Register.CONFIG, 0x02);
            GyroscopeRange = GyroscopeRange.Range0250Dps;
            // Set full scale range for the gyro to 250 dps
            // Setup the registers for accelerometer as in documentation
            // DLPF Config | LPF BW | Sampling Rate | Filter Delay 
            // 2           | 92Hz   | 1kHz          | 7.8ms 
            WriteRegister(Register.ACCEL_CONFIG_2, 0x02);
            AccelerometerRange = AccelerometerRange.Range02G;

            // Read the data 200 times as per the documentation page 5

            for (int reading = 0; reading < numCycles; reading++)
            {
                gyroAvegage = GetRawGyroscope();
                accAverage = GetRawAccelerometer();
            }

            accAverage /= numCycles;
            gyroAvegage /= numCycles;

            // Set USR_Reg: (1Bh) Gyro_Config, gdrive_axisCTST [0-2] to b111 to enable Self-Test. 
            WriteRegister(Register.ACCEL_CONFIG, 0xE0);
            // Set USR_Reg: (1Ch) Accel_Config, AX/Y/Z_ST_EN   [0-2] to b111 to enable Self-Test. 
            WriteRegister(Register.GYRO_CONFIG, 0xE0);
            // Wait 20ms for oscillations to stabilize 
            Thread.Sleep(20);

            // Read the gyroscope and accelerometer output at a 1kHz rate and average 200 readings. 
            // The averaged values will be the LSB of GX_ST_OS, GY_ST_OS, GZ_ST_OS, AX_ST_OS, AY_ST_OS and AZ_ST_OS in the software
            for (int reading = 0; reading < numCycles; reading++)
            {
                gyroSelfTestAverage = GetRawGyroscope();
                accSelfTestAverage = GetRawAccelerometer();
            }

            accSelfTestAverage /= numCycles;
            gyroSelfTestAverage /= numCycles;

            // To cleanup the configuration after the test
            // Set USR_Reg: (1Bh) Gyro_Config, gdrive_axisCTST [0-2] to b000.
            WriteRegister(Register.ACCEL_CONFIG, 0x00);
            // Set USR_Reg: (1Ch) Accel_Config, AX/Y/Z_ST_EN [0-2] to b000.
            WriteRegister(Register.GYRO_CONFIG, 0x00);
            // Wait 20ms for oscillations to stabilize
            Thread.Sleep(20);

            // Retrieve factory Self-Test code (ST_Code) from USR_Reg in the software
            gyroSelfTest.X = ReadByte(Register.SELF_TEST_X_GYRO);
            gyroSelfTest.Y = ReadByte(Register.SELF_TEST_Y_GYRO);
            gyroSelfTest.Z = ReadByte(Register.SELF_TEST_Z_GYRO);
            accSelfTest.X = ReadByte(Register.SELF_TEST_X_ACCEL);
            accSelfTest.Y = ReadByte(Register.SELF_TEST_Y_ACCEL);
            accSelfTest.Z = ReadByte(Register.SELF_TEST_Z_ACCEL);

            // Calculate all factory trim
            accFactoryTrim.X = (float)((2620 / 1 << gyroAccScale) * (Math.Pow(1.01, accSelfTest.X - 1.0)));
            accFactoryTrim.Y = (float)((2620 / 1 << gyroAccScale) * (Math.Pow(1.01, accSelfTest.Y - 1.0)));
            accFactoryTrim.Z = (float)((2620 / 1 << gyroAccScale) * (Math.Pow(1.01, accSelfTest.Z - 1.0)));
            gyroFactoryTrim.X = (float)((2620 / 1 << gyroAccScale) * (Math.Pow(1.01, gyroSelfTest.X - 1.0)));
            gyroFactoryTrim.Y = (float)((2620 / 1 << gyroAccScale) * (Math.Pow(1.01, gyroSelfTest.Y - 1.0)));
            gyroFactoryTrim.Z = (float)((2620 / 1 << gyroAccScale) * (Math.Pow(1.01, gyroSelfTest.Z - 1.0)));

            if (gyroFactoryTrim.X != 0)
            {
                gyroAvegage.X = (gyroSelfTestAverage.X - gyroAvegage.X) / gyroFactoryTrim.X;
            }
            else
            {
                gyroAvegage.X = Math.Abs(gyroSelfTestAverage.X - gyroAvegage.X);
            }

            if (gyroFactoryTrim.Y != 0)
            {
                gyroAvegage.Y = (gyroSelfTestAverage.Y - gyroAvegage.Y) / gyroFactoryTrim.Y;
            }
            else
            {
                gyroAvegage.Y = Math.Abs(gyroSelfTestAverage.Y - gyroAvegage.Y);
            }

            if (gyroFactoryTrim.Z != 0)
            {
                gyroAvegage.Z = (gyroSelfTestAverage.Z - gyroAvegage.Z) / gyroFactoryTrim.Z;
            }
            else
            {
                gyroAvegage.Z = Math.Abs(gyroSelfTestAverage.Z - gyroAvegage.Z);
            }

            // Accelerator
            if (accFactoryTrim.X != 0)
            {
                accAverage.X = (accSelfTestAverage.X - accAverage.X) / accFactoryTrim.X;
            }
            else
            {
                accAverage.X = Math.Abs(accSelfTestAverage.X - accSelfTestAverage.X);
            }

            if (accFactoryTrim.Y != 0)
            {
                accAverage.Y = (accSelfTestAverage.Y - accAverage.Y) / accFactoryTrim.Y;
            }
            else
            {
                accAverage.Y = Math.Abs(accSelfTestAverage.Y - accSelfTestAverage.Y);
            }

            if (accFactoryTrim.Z != 0)
            {
                accAverage.Z = (accSelfTestAverage.Z - accAverage.Z) / accFactoryTrim.Z;
            }
            else
            {
                accAverage.Z = Math.Abs(accSelfTestAverage.Z - accSelfTestAverage.Z);
            }

            return (gyroAvegage, accAverage);
        }

        #endregion

        #region I2C

        /// <summary>
        /// Write data on any of the I2C slave attached to the MPU9250
        /// </summary>
        /// <param name="i2cChannel">The slave channel to attached to the I2C device</param>
        /// <param name="address">The I2C address of the slave I2C element</param>
        /// <param name="register">The register to write to the slave I2C element</param>
        /// <param name="data">The byte data to write to the slave I2C element</param>
        public void WriteByteToSlaveDevice(I2cChannel i2cChannel, byte address, byte register, byte data)
        {
            // I2C_SLVx_ADDR += 3 * i2cChannel
            byte slvAddress = (byte)((byte)Register.I2C_SLV0_ADDR + 3 * (byte)i2cChannel);
            Span<byte> dataout = stackalloc byte[2] { slvAddress, address };
            _i2cDevice.Write(dataout);
            // I2C_SLVx_REG = I2C_SLVx_ADDR + 1
            dataout[0] = (byte)(slvAddress + 1);
            dataout[1] = register;
            _i2cDevice.Write(dataout);
            // I2C_SLVx_D0 =  I2C_SLV0_DO + i2cChannel
            // Except Channel4
            byte channelData = i2cChannel != I2cChannel.Slave4 ? (byte)((byte)Register.I2C_SLV0_DO + (byte)i2cChannel) : (byte)Register.I2C_SLV4_DO;
            dataout[0] = channelData;
            dataout[1] = data;
            _i2cDevice.Write(dataout);
            // I2C_SLVx_CTRL = I2C_SLVx_ADDR + 2
            dataout[0] = (byte)(slvAddress + 2);
            dataout[1] = 0x81;
            _i2cDevice.Write(dataout);
        }

        /// <summary>
        /// Read data from any of the I2C slave attached to the MPU9250
        /// </summary>
        /// <param name="i2cChannel">The slave channel to attached to the I2C device</param>
        /// <param name="address">The I2C address of the slave I2C element</param>
        /// <param name="register">The register to read from the slave I2C element</param>
        /// <param name="readBytes">The read data</param>
        public void ReadByteFromSlaveDevice(I2cChannel i2cChannel, byte address, byte register, Span<byte> readBytes)
        {
            if (readBytes.Length > 24)
                throw new ArgumentException($"Can't read more than 24 bytes at once");
            byte slvAddress = (byte)((byte)Register.I2C_SLV0_ADDR + 3 * (byte)i2cChannel);
            Span<byte> dataout = stackalloc byte[2] { slvAddress, (byte)(address | 0x80) };
            _i2cDevice.Write(dataout);
            dataout[0] = (byte)(slvAddress + 1);
            dataout[1] = (byte)register;
            _i2cDevice.Write(dataout);
            dataout[0] = (byte)(slvAddress + 2);
            dataout[1] = (byte)(0x80 | readBytes.Length);
            _i2cDevice.Write(dataout);
            // Just need to wait a very little bit
            // For data transfer to happen and process on the MPU9250 side
            DelayHelper.DelayMicroseconds(140 + readBytes.Length * 10, false);
            _i2cDevice.WriteByte((byte)Register.EXT_SENS_DATA_00);
            _i2cDevice.Read(readBytes);
        }

        internal void WriteRegister(Register reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[] { (byte)reg, data };
            _i2cDevice.Write(dataout);
        }

        internal byte ReadByte(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        internal void ReadBytes(Register reg, Span<byte> readBytes)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(readBytes);
        }

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        #endregion
    }
}
