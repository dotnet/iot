// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Threading;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// Mpu6886 accelerometer and gyroscope
    /// </summary>
    [Interface("Mpu6886 accelerometer and gyroscope")]
    public class Mpu6886AccelerometerGyroscope : IDisposable
    {
        private const float GyroscopeResolution = (float)(2000.0 / 32768.0); // default gyro scale 2000 dps
        private const float AccelerometerResolution = (float)(8.0 / 32768.0); // default accelerometer res 8G

        /// <summary>
        /// The default I2C address for the MPU6886 sensor. (Datasheet page 49)
        /// Mind that the address can be configured as well for 0x69 depending upon the value driven on AD0 pin.
        /// </summary>
        public const int DefaultI2cAddress = 0x68;

        /// <summary>
        /// The secondary I2C address for the MPU6886 sensor. (Datasheet page 49)
        /// </summary>
        public const int SecondaryI2cAddress = 0x69;

        private I2cDevice _i2c;

        /// <summary>
        /// Mpu6886 - Accelerometer and Gyroscope bus
        /// </summary>
        public Mpu6886AccelerometerGyroscope(
            I2cDevice i2cDevice)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            Span<byte> readBuffer = stackalloc byte[1];

            _i2c.WriteByte((byte)Mpu6886.Register.WhoAmI);
            _i2c.Read(readBuffer);
            if (readBuffer[0] != 0x19)
            {
                throw new IOException($"This device does not contain the correct signature 0x19 for a MPU6886");
            }

            // Initialization sequence
            // Thread.Sleep values according to startup times and delays documented in datasheet.
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0000_0000 });
            Thread.Sleep(10);

            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0100_0000 });
            Thread.Sleep(10);

            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0000_0001 });
            Thread.Sleep(10);

            AccelerometerScale = AccelerometerScale.Scale8G;
            GyroscopeScale = GyroscopeScale.Scale2000dps;

            // CONFIG(0x1a) 1khz output
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.Configuration, 0b0000_0001 });
            Thread.Sleep(1);

            SampleRateDivider = 0b0000_0001;
            AccelerometerInterruptEnabled = InterruptEnable.None;

            // ACCEL_CONFIG 2(0x1d)
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration2, 0b0000_0000 });
            Thread.Sleep(1);

            // USER_CTRL(0x6a)
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.UserControl, 0b0000_0000 });
            Thread.Sleep(1);

            // FIFO_EN(0x23)
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.FifoEnable, 0b0000_0000 });
            Thread.Sleep(1);

            // INT_PIN_CFG(0x37)
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.IntPinBypassEnabled, 0b0010_0010 });
            Thread.Sleep(1);

            // INT_ENABLE(0x38), "Data ready interrupt enable" bit
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.InteruptEnable, 0b0000_0001 });
            Thread.Sleep(100);

            // To avoid limiting sensor output to less than 0x7F7F, set this bit to 1. This should be done every time the MPU-6886 is powered up.
            // Datasheet page 46
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.AccelerometerIntelligenceControl, 0b0000_0010 });
            Thread.Sleep(10);
        }

        private Vector3 GetRawAccelerometer()
        {
            Span<Byte> vec = stackalloc byte[6];
            Read(Mpu6886.Register.AccelerometerMeasurementXHighByte, vec);

            short x = (short)(vec[0] << 8 | vec[1]);
            short y = (short)(vec[2] << 8 | vec[3]);
            short z = (short)(vec[4] << 8 | vec[5]);

            return new Vector3(x, y, z);
        }

        private Vector3 GetRawGyroscope()
        {
            Span<Byte> vec = stackalloc byte[6];
            Read(Mpu6886.Register.GyropscopeMeasurementXHighByte, vec);

            short x = (short)(vec[0] << 8 | vec[1]);
            short y = (short)(vec[2] << 8 | vec[3]);
            short z = (short)(vec[4] << 8 | vec[5]);

            return new Vector3(x, y, z);
        }

        private short GetRawInternalTemperature()
        {
            Span<Byte> vec = stackalloc byte[2];
            Read(Mpu6886.Register.TemperatureMeasurementHighByte, vec);

            return (short)(vec[0] << 8 | vec[1]);
        }

        /// <summary>
        /// Reads the current accelerometer values from the registers, and compensates them with the accelerometer resolution.
        /// </summary>
        /// <returns>Vector of acceleration</returns>
        public Vector3 GetAccelerometer() => GetRawAccelerometer() * AccelerometerResolution;

        /// <summary>
        /// Reads the current gyroscope values from the registers, and compensates them with the gyroscope resolution.
        /// </summary>
        /// <returns>Vector of the rotation</returns>
        public Vector3 GetGyroscope() => GetRawGyroscope() * GyroscopeResolution;

        /// <summary>
        /// Reads the register of the on-chip temperature sensor which represents the MPU-6886 die temperature.
        /// </summary>
        /// <returns>Temperature in degrees Celcius</returns>
        public Temperature GetInternalTemperature()
        {
            var rawInternalTemperature = GetRawInternalTemperature();

            // p43 of datasheet describes the room temp. compensation calcuation
            return new Temperature(rawInternalTemperature / 326.8 + 25.0, UnitsNet.Units.TemperatureUnit.DegreeCelsius);
        }

        private void WriteByte(Register register, byte data)
        {
            Span<Byte> buff = stackalloc byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private short ReadInt16(Register register)
        {
            Span<Byte> val = stackalloc byte[2];
            Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        private void Read(Register register, Span<byte> buffer)
        {
            _i2c.WriteByte((byte)((byte)register));
            _i2c.Read(buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }

        /// <summary>
        /// Calibrate the gyroscope by calculating the offset values and storing them in the GyroscopeOffsetAdjustment registers of the MPU6886.
        /// </summary>
        /// <param name="iterations">The number of sample gyroscope values to read</param>
        /// <returns>The calulated offset vector</returns>
        public Vector3 Calibrate(int iterations)
        {
            GyroscopeOffset = new Vector3(0, 0, 0);
            Thread.Sleep(2);

            var gyrSum = new double[3];

            for (int i = 0; i < iterations; i++)
            {
                var gyr = GetRawGyroscope();

                gyrSum[0] += gyr.X;
                gyrSum[1] += gyr.Y;
                gyrSum[2] += gyr.Z;

                Thread.Sleep(2);
            }

            Vector3 offset = new Vector3((float)(gyrSum[0] / iterations), (float)(gyrSum[1] / iterations), (float)(gyrSum[2] / iterations));

            GyroscopeOffset = offset;

            return offset;
        }

        /// <summary>
        /// Gets and sets the gyroscope offset in the GyroscopeOffsetAdjustment registers of the MPU6886.
        /// Setting the offset can be usefull when a custom callibration calculation is used, instead of the Calibrate function of this class.
        /// </summary>
        public Vector3 GyroscopeOffset
        {
            get
            {
                Span<Byte> vec = stackalloc byte[6];
                Read(Mpu6886.Register.GyroscopeOffsetAdjustmentXHighByte, vec);

                Vector3 v = new Vector3();
                v.X = (short)(vec[0] << 8 | vec[1]);
                v.Y = (short)(vec[2] << 8 | vec[3]);
                v.Z = (short)(vec[4] << 8 | vec[5]);

                return v;
            }

            set
            {
                Span<Byte> registerAndOffset = stackalloc byte[7];
                Span<Byte> offsetbyte = stackalloc byte[2];

                registerAndOffset[0] = (byte)Mpu6886.Register.GyroscopeOffsetAdjustmentXHighByte;

                BinaryPrimitives.WriteInt16BigEndian(offsetbyte, (short)value.X);
                registerAndOffset[1] = offsetbyte[0];
                registerAndOffset[2] = offsetbyte[1];

                BinaryPrimitives.WriteInt16BigEndian(offsetbyte, (short)value.Y);
                registerAndOffset[3] = offsetbyte[0];
                registerAndOffset[4] = offsetbyte[1];

                BinaryPrimitives.WriteInt16BigEndian(offsetbyte, (short)value.Z);
                registerAndOffset[5] = offsetbyte[0];
                registerAndOffset[6] = offsetbyte[1];

                _i2c.Write(registerAndOffset);
            }
        }

        /// <summary>
        /// Reset the internal registers and restores the default settings. (Datasheet, page 47)
        /// </summary>
        public void Reset()
        {
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b1000_0000 });
            Thread.Sleep(10);
        }

        /// <summary>
        /// Set the chip to sleep mode. (Datasheet, page 47)
        /// </summary>
        public void Sleep()
        {
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0100_0000 });
            Thread.Sleep(10);
        }

        /// <summary>
        /// Disables the sleep mode. (Datasheet, page 47)
        /// </summary>
        public void WakeUp()
        {
            _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0000_0000 });
            Thread.Sleep(10);
        }

        /// <summary>
        /// Gets and sets the accelerometer full scale. (Datasheet page 37)
        /// </summary>
        public AccelerometerScale AccelerometerScale
        {
            get
            {
                Span<Byte> buffer = stackalloc byte[1];
                Read(Mpu6886.Register.AccelerometerConfiguration1, buffer);
                return (AccelerometerScale)(buffer[0] & 0b0001_1000);
            }

            set
            {
                // First read the current register values
                Span<Byte> currentRegisterValues = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.AccelerometerConfiguration1);
                _i2c.Read(currentRegisterValues);

                // apply the new scale, we leave all bits except bit 3 and 4 untouched with mask 0b1110_0111
                byte newvalue = (byte)((currentRegisterValues[0] & 0b1110_0111) | (byte)value);

                // write the new register value
                _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration1, newvalue });

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets and sets the gyroscope full scale. (Datasheet page 37)
        /// </summary>
        public GyroscopeScale GyroscopeScale
        {
            get
            {
                Span<Byte> buffer = stackalloc byte[1];
                Read(Mpu6886.Register.GyroscopeConfiguration, buffer);
                return (GyroscopeScale)(buffer[0] & 0b0001_1000);
            }

            set
            {
                // First read the current register values
                Span<Byte> currentRegisterValues = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.GyroscopeConfiguration);
                _i2c.Read(currentRegisterValues);

                // apply the new scale, we leave all bits except bit 3 and 4 untouched with this mask 0b1110_0111
                byte newvalue = (byte)((currentRegisterValues[0] & 0b1110_0111) | (byte)value);

                // write the new register value
                _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.GyroscopeConfiguration, newvalue });

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Sets the enabled axes of the gyroscope and accelerometer. (Datasheet page 47)
        /// </summary>
        public EnabledAxis EnabledAxes
        {
            get
            {
                Span<Byte> readBuffer = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.PowerManagement2);
                _i2c.Read(readBuffer);

                // bit 1 in the register means disabled, so using bitwise not to flip bits.
                return (EnabledAxis)(~readBuffer[0] & 0b0011_1111);
            }

            set
            {
                // First read the current register values
                Span<Byte> currentRegisterValues = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.PowerManagement2);
                _i2c.Read(currentRegisterValues);

                // apply the new enabled axes, we leave all bits except bit 7 and 6 untouched with mask 0b1100_0000
                byte newvalue = (byte)((currentRegisterValues[0] & 0b1100_0000) | (byte)value);

                // write the new register value
                // bit 1 in the register means disabled, so using bitwise not to flip bits.
                _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.PowerManagement2, (byte)~newvalue });

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets and sets the averaging filter settings for low power accelerometer mode. (Datasheet page 37)
        /// </summary>
        public AccelerometerLowPowerMode AccelerometerLowPowerMode
        {
            get
            {
                Span<Byte> currentRegisterValues = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.AccelerometerConfiguration2);
                _i2c.Read(currentRegisterValues);

                // we leave all bits except bit 4 and 5 untouched with this mask
                byte cleaned = (byte)(currentRegisterValues[0] & 0b0011_0000);

                return (AccelerometerLowPowerMode)cleaned;
            }

            set
            {
                // First read the current register values
                Span<Byte> currentRegisterValues = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.AccelerometerConfiguration2);
                _i2c.Read(currentRegisterValues);

                // apply the new enabled axes, we leave all bits except bit 4 and 5 untouched with mask 0b1100_1111
                byte newvalue = (byte)((currentRegisterValues[0] & 0b1100_1111) | (byte)value);

                // write the new register value
                _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration2, newvalue });
                Thread.Sleep(2);
            }
        }

        /// <summary>
        /// Divides the internal sample rate (see register CONFIG) to generate the sample rate that
        /// controls sensor data output rate, FIFO sample rate. (Datasheet page 35)
        /// </summary>
        public byte SampleRateDivider
        {
            get
            {
                Span<Byte> readbuffer = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.SampleRateDevider);
                _i2c.Read(readbuffer);
                return readbuffer[0];
            }

            set
            {
                _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.SampleRateDevider, value });
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// The axes on which the interrupt should be enabled.
        /// </summary>
        public InterruptEnable AccelerometerInterruptEnabled
        {
            get
            {
                Span<Byte> readbuffer = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.InteruptEnable);
                _i2c.Read(readbuffer);
                return (InterruptEnable)(readbuffer[0] & 0b1110_0000);
            }

            set
            {
                // First read the current register values
                Span<Byte> currentRegisterValues = stackalloc byte[1];
                _i2c.WriteByte((byte)Mpu6886.Register.InteruptEnable);
                _i2c.Read(currentRegisterValues);

                // apply the new enabled axes, we leave all bits except bit 4 and 5 untouched with mask 0b0011_1111
                byte newvalue = (byte)((currentRegisterValues[0] & 0b0011_1111) | (byte)value);

                // write the new register value
                _i2c.Write(stackalloc byte[] { (byte)Mpu6886.Register.InteruptEnable, newvalue });
                Thread.Sleep(2);
            }
        }
    }
}