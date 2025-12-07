// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Drawing;
using Ina236.Tests;
using Iot.Device.Graphics;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Adc.Tests
{
    public sealed class Ina236Tests : IDisposable
    {
        private Ina236 _ina236;
        private SimulatedIna236 _simulatedIna236;

        public Ina236Tests()
        {
            _simulatedIna236 = new SimulatedIna236(new I2cConnectionSettings(1, 0x40));
            // Use the setting that corresponds to the example values in the data sheet
            _ina236 = new Ina236(_simulatedIna236, ElectricResistance.FromMilliohms(8), ElectricCurrent.FromAmperes(16.384 / 2.0));
        }

        [Fact]
        public void InitialValues()
        {
            Assert.Equal(Ina236OperatingMode.ContinuousShuntAndBusVoltage, _ina236.OperatingMode);
            Assert.Equal(1u, _ina236.AverageOverNoSamples);
            Assert.Equal(1100, _ina236.BusConversionTime);
            Assert.Equal(1100, _ina236.ShuntConversionTime);
        }

        [Fact]
        public void CheckSetupComplete()
        {
            int calibrationValue = _simulatedIna236.RegisterMap[5].ReadRegister();
            Assert.Equal(1280, calibrationValue);
        }

        [Fact]
        public void ReadValues()
        {
            // Calibration has been set up, so we should get the values mentioned in the data sheet
            ElectricPotential voltage = _ina236.ReadBusVoltage();
            Assert.Equal(12.0, voltage.Volts);

            ElectricCurrent current = _ina236.ReadCurrent();
            Assert.Equal(6.0, current.Amperes);

            Power p = _ina236.ReadPower();
            Assert.Equal(72.0m, p.Watts);
        }

        public void Dispose()
        {
            _ina236.Dispose();
        }
    }
}
