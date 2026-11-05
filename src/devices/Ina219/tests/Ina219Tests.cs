// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Ina219.Tests;
using UnitsNet;
using Xunit;

namespace Iot.Device.Adc.Tests
{
    public sealed class Ina219Tests : IDisposable
    {
        private const byte ShuntVoltageRegister = 1;
        private const byte BusVoltageRegister = 2;
        private const byte CurrentRegister = 4;

        private Ina219 _ina219;
        private SimulatedIna219 _simulatedIna219;

        public Ina219Tests()
        {
            _simulatedIna219 = new SimulatedIna219(new I2cConnectionSettings(1, 0x40));
            _ina219 = new Ina219(_simulatedIna219);
            _ina219.Reset();
            // The calibration of the sample in the README: 12.2uA per bit
            _ina219.SetCalibration(33574, 12.2e-6f);
        }

        [Fact]
        public void InitialValues()
        {
            Assert.Equal(Ina219BusVoltageRange.Range32v, _ina219.BusVoltageRange);
            Assert.Equal(Ina219PgaSensitivity.PlusOrMinus320mv, _ina219.PgaSensitivity);
            Assert.Equal(Ina219AdcResolutionOrSamples.Adc12Bit, _ina219.BusAdcResolutionOrSamples);
            Assert.Equal(Ina219AdcResolutionOrSamples.Adc12Bit, _ina219.ShuntAdcResolutionOrSamples);
            Assert.Equal(Ina219OperatingMode.ShuntAndBusContinuous, _ina219.OperatingMode);
        }

        [Theory]
        [InlineData(0x0000, 0.0)]
        [InlineData(0x0FA0, 40.0)] // 4000 * 10uV
        [InlineData(0x7D00, 320.0)] // full scale with +/-320mV
        [InlineData(0xFFFF, -0.01)] // -1 * 10uV
        [InlineData(0xF060, -40.0)] // -4000 * 10uV
        [InlineData(0x8300, -320.0)] // negative full scale with +/-320mV
        public void ReadShuntVoltageIsSigned(int registerValue, double expectedMillivolts)
        {
            _simulatedIna219.RegisterMap[ShuntVoltageRegister].WriteRegister(registerValue);

            ElectricPotential shuntVoltage = _ina219.ReadShuntVoltage();

            Assert.Equal(expectedMillivolts, shuntVoltage.Millivolts, 6);
        }

        [Theory]
        [InlineData(0x0000, 0.0)]
        [InlineData(0x8000, 16.384)] // 4096 * 4mV, the most significant bit is set from 16.384V
        [InlineData(0x3E80, 8.0)] // 2000 * 4mV
        [InlineData(0x9C40, 20.0)] // 5000 * 4mV
        [InlineData(0xCB23, 26.0)] // 6500 * 4mV, the maximum bus voltage, with the CNVR and OVF bits set
        public void ReadBusVoltageIsUnsigned(int registerValue, double expectedVolts)
        {
            _simulatedIna219.RegisterMap[BusVoltageRegister].WriteRegister(registerValue);

            ElectricPotential busVoltage = _ina219.ReadBusVoltage();

            Assert.Equal(expectedVolts, busVoltage.Volts, 6);
        }

        [Theory]
        [InlineData(0x0000, 0.0)]
        [InlineData(0x0001, 0.0122)] // 1 * 12.2uA
        [InlineData(0x0FA0, 48.8)] // 4000 * 12.2uA
        [InlineData(0xFFFF, -0.0122)] // -1 * 12.2uA, read as 799.5mA when the register is taken as unsigned
        [InlineData(0xF060, -48.8)] // -4000 * 12.2uA
        public void ReadCurrentIsSigned(int registerValue, double expectedMilliamperes)
        {
            _simulatedIna219.RegisterMap[CurrentRegister].WriteRegister(registerValue);

            ElectricCurrent current = _ina219.ReadCurrent();

            Assert.Equal(expectedMilliamperes, current.Milliamperes, 3);
        }

        [Theory]
        [InlineData(Ina219AdcResolutionOrSamples.Adc9Bit)]
        [InlineData(Ina219AdcResolutionOrSamples.Adc11Bit)]
        [InlineData(Ina219AdcResolutionOrSamples.Adc128Sample)]
        public void SetShuntAdcResolutionOrSamplesOnlyChangesTheShuntAdc(Ina219AdcResolutionOrSamples value)
        {
            _ina219.ShuntAdcResolutionOrSamples = value;

            Assert.Equal(value, _ina219.ShuntAdcResolutionOrSamples);
            Assert.Equal(Ina219AdcResolutionOrSamples.Adc12Bit, _ina219.BusAdcResolutionOrSamples);
        }

        [Theory]
        [InlineData(Ina219AdcResolutionOrSamples.Adc9Bit)]
        [InlineData(Ina219AdcResolutionOrSamples.Adc11Bit)]
        [InlineData(Ina219AdcResolutionOrSamples.Adc128Sample)]
        public void SetBusAdcResolutionOrSamplesOnlyChangesTheBusAdc(Ina219AdcResolutionOrSamples value)
        {
            _ina219.BusAdcResolutionOrSamples = value;

            Assert.Equal(value, _ina219.BusAdcResolutionOrSamples);
            Assert.Equal(Ina219AdcResolutionOrSamples.Adc12Bit, _ina219.ShuntAdcResolutionOrSamples);
        }

        public void Dispose()
        {
            _ina219.Dispose();
        }
    }
}
