// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.IO;
using Iot.Device.Bmxx80;
using Iot.Device.Imu.Tests;
using UnitsNet;
using Xunit;

namespace Iot.Device.Bmxx80.Tests
{
    /// <summary>
    /// Register shared by the Bmx280 family.
    /// </summary>
    internal enum Bmx280Register : byte
    {
        CTRL_MEAS = 0xF4,

        DIG_T1 = 0x88,
        DIG_T2 = 0x8A,
        DIG_T3 = 0x8C,

        DIG_P1 = 0x8E,
        DIG_P2 = 0x90,
        DIG_P3 = 0x92,
        DIG_P4 = 0x94,
        DIG_P5 = 0x96,
        DIG_P6 = 0x98,
        DIG_P7 = 0x9A,
        DIG_P8 = 0x9C,
        DIG_P9 = 0x9E,

        STATUS = 0xF3,
        CONFIG = 0xF5,

        PRESSUREDATA = 0xF7,
        TEMPDATA_MSB = 0xFA
    }

    public sealed class Bmp280SensorTests : IDisposable
    {
        private SimulatedI2cDevice _i2cDevice;

        public Bmp280SensorTests()
        {
            _i2cDevice = new SimulatedI2cDevice();
            _i2cDevice.SetRegister(0xD0, 0x58);
            // Calibration data
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_T1, 27504);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_T2, 26435);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_T3, -1000);

            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P1, 36477);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P2, -10685);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P3, 3024);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P4, 2855);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P5, 140);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P6, -7);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P7, 15500);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P8, -14600);
            _i2cDevice.SetRegister((int)Bmx280Register.DIG_P9, 6000);
            // Together, the next three registers give the 20 bit temperature readout, with the last 4 bits unused. The hex value is 0x655AC
            _i2cDevice.SetRegister(0xFA, 0x7E);
            _i2cDevice.SetRegister(0xFB, 0xED);
            _i2cDevice.SetRegister(0xFC, 0x00);

            // And the 20 bit pressure readout
            _i2cDevice.SetRegister(0xF7, 0x65);
            _i2cDevice.SetRegister(0xF8, 0x5A);
            _i2cDevice.SetRegister(0xF9, 0xC0);
        }

        // This runs the calculation with the sample values defined in https://cdn-shop.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf
        [Fact]
        public void CalculationWithSampleValues()
        {
            Bmp280 sensor = new Bmp280(_i2cDevice);
            sensor.TemperatureSampling = Sampling.HighResolution;
            sensor.TryReadTemperature(out Temperature temperature);
            Assert.Equal(25.08, temperature.DegreesCelsius, 2);
            sensor.TryReadPressure(out Pressure pressure);
            Assert.Equal(100653.27, pressure.Pascals, 2);
            sensor.Dispose();
        }

        public void Dispose()
        {
            _i2cDevice.Dispose();
        }
    }
}
