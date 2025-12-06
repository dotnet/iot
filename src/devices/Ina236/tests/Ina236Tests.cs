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

        public Ina236Tests()
        {
            _ina236 = new Ina236(new SimulatedIna236(new I2cConnectionSettings(1, 0x40)), ElectricResistance.FromMilliohms(8), ElectricCurrent.FromAmperes(10));
        }

        [Fact]
        public void InitialValues()
        {
            Assert.Equal(Ina236OperatingMode.ContinuousShuntAndBusVoltage, _ina236.OperatingMode);
            Assert.Equal(1u, _ina236.AverageOverNoSamples);
            Assert.Equal(1100, _ina236.BusConversionTime);
            Assert.Equal(1100, _ina236.ShuntConversionTime);
        }

        public void Dispose()
        {
        }
    }
}
