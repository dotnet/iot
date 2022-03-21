// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using Iot.Device.Magnetometer;
using UnitsNet;

namespace Iot.Device.Imu
{
    /// <summary>
    /// MPU6500 - gyroscope, accelerometer and temperature sensor
    /// </summary>
    [Interface("MPU6500 - gyroscope, accelerometer and temperature sensor")]
    public class Mpu6500 : Mpu6050
    {
        /// <summary>
        /// Initialize the MPU6500
        /// </summary>
        /// <param name="i2cDevice">The I2C device</param>
        public Mpu6500(I2cDevice i2cDevice)
            : base(i2cDevice, true)
        {
            Reset();
            PowerOn();
            if (!CheckVersion())
            {
                throw new IOException($"This device does not contain the correct signature 0x70 for a MPU6500");
            }

            GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0250Hz;
            GyroscopeRange = GyroscopeRange.Range0250Dps;
            AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth1130Hz;
            AccelerometerRange = AccelerometerRange.Range02G;
        }

        /// <summary>
        /// Used to create the class for the MPU9250. Initialization is a bit different than for the MPU6500
        /// </summary>
        internal Mpu6500(I2cDevice i2cDevice, bool isInternal)
            : base(i2cDevice, isInternal)
        {
        }

        #region Modes, constructor, Dispose

        /// <summary>
        /// Return true if the version of MPU6500 is the correct one
        /// </summary>
        /// <returns>True if success</returns>
        // Check if the version is thee correct one
        internal new bool CheckVersion() => ReadByte(Register.WHO_AM_I) == 0x70;

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
        [Command]
        public (Vector3 GyroscopeAverage, Vector3 AccelerometerAverage) RunGyroscopeAccelerometerSelfTest()
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

    }
}
