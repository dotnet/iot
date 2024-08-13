﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.I2c;
using System.Device.Pwm;
using System.Threading;
using Iot.Device.Adc;
using Iot.Device.Bmxx80;
using Iot.Device.Board;
using UnitsNet;
using Xunit;
using static System.Device.Gpio.Tests.SetupHelpers;

namespace System.Device.Gpio.Tests;

public class ProtocolsTests
{
    // See #2339: This test is very frequently failing in CI, looks like a hardware issue.
    [Fact(Skip = "Test is running unreliably")]
    [Trait("feature", "spi")]
    public void SPI_Mcp3008CanRead()
    {
        using (Mcp3008 adc = CreateAdc())
        {
            // We don't care about specific value for the first 5 channels
            for (int i = 0; i <= 4; i++)
            {
                Assert.InRange(adc.Read(i), MinAdc, MaxAdc);
            }

            // VCC
            Assert.InRange(adc.Read(5), MaxAdc - 5, MaxAdc);

            // GND
            Assert.InRange(adc.Read(6), MinAdc, MinAdc + 5);

            // Voltage divider with equal resistors (50% VCC)
            AdcValueAround(HalfAdc, adc.Read(7));
        }
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_Bme280CanRead()
    {
        using (Bme280 bme280 = CreateBme280())
        {
            TestBme280Reading(bme280);
        }
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_Bme280CanRead()
    {
        using (I2cBus i2cBus = CreateI2cBusForBme280())
        using (Bme280 bme280 = CreateBme280(i2cBus))
        {
            TestBme280Reading(bme280);
        }
    }

    private static void TestBme280Reading(Bme280 bme280)
    {
        Assert.True(bme280.TryReadTemperature(out Temperature temperature));

        // assuming that tests are run in the room temperature
        // worst case scenario: it's very hot outside
        Assert.InRange(temperature.DegreesCelsius, 15, 40);

        Assert.True(bme280.TryReadPressure(out Pressure pressure));
        // https://en.wikipedia.org/wiki/List_of_weather_records
        // Min and max are extremes recorded on land
        double pressureHPa = pressure.Hectopascals;
        Assert.InRange(pressureHPa, 892, 1084);

        Assert.True(bme280.TryReadHumidity(out RelativeHumidity relativeHumidity));
        Assert.InRange(relativeHumidity.Percent, 0, 100);
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_MultipleDispose()
    {
        // Every dispose operation we do twice to make sure it won't cause issues.
        // Devices are frequently wrapped and this can happen in real apps

        // we dispose device first, then bus
        I2cBus i2cBus = CreateI2cBusForBme280();
        Bme280 bme280 = CreateBme280(i2cBus);
        bme280.Dispose();
        bme280.Dispose();
        i2cBus.Dispose();
        i2cBus.Dispose();

        // we dispose bus first, then device
        i2cBus = CreateI2cBusForBme280();
        bme280 = CreateBme280(i2cBus);
        i2cBus.Dispose();
        i2cBus.Dispose();
        bme280.Dispose();
        bme280.Dispose();
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_MultipleCreate()
    {
        I2cBus i2cBus = CreateI2cBusForBme280();

        I2cDevice device1 = i2cBus.CreateDevice(Bmp280.DefaultI2cAddress);
        device1.ReadByte();
        i2cBus.RemoveDevice(Bmp280.DefaultI2cAddress);

        I2cDevice device2 = i2cBus.CreateDevice(Bmp280.DefaultI2cAddress);
        device2.ReadByte();
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_MultipleCreateAndDispose()
    {
        I2cBus i2cBus = CreateI2cBusForBme280();

        I2cDevice device1 = i2cBus.CreateDevice(Bmp280.DefaultI2cAddress);
        device1.ReadByte();
        device1.Dispose();

        I2cDevice device2 = i2cBus.CreateDevice(Bmp280.DefaultI2cAddress);
        device2.ReadByte();
        device2.Dispose();
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_Scan()
    {
        I2cBus i2cBus = CreateI2cBusForBme280();
        List<int> addresses = i2cBus.PerformBusScan();
        Assert.NotNull(addresses);
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_ScanMultipleTimes()
    {
        I2cBus i2cBus = CreateI2cBusForBme280();

        List<int> addresses1 = i2cBus.PerformBusScan();
        Assert.NotNull(addresses1);

        List<int> addresses2 = i2cBus.PerformBusScan();
        Assert.NotNull(addresses2);
    }

    [Fact]
    [Trait("feature", "i2c")]
    public void I2C_I2cBus_HasBmp280Present()
    {
        I2cBus i2cBus = CreateI2cBusForBme280();
        List<int> addresses = i2cBus.PerformBusScan();
        Assert.NotEmpty(addresses);
        Assert.Contains(Bmp280.DefaultI2cAddress, addresses);
    }

    [Fact]
    [Trait("feature", "pwm")]
    [Trait("feature", "spi")]
    public void PWM_DutyCycleIsSetCorrectly()
    {
        using (PwmChannel pwm = CreatePwmChannel(dutyCycle: 0))
        using (Mcp3008 adc = CreateAdc())
        {
            for (int n = 0; n < 2; n++)
            {
                for (int i = 0; i <= 10; i++)
                {
                    pwm.DutyCycle = i * 0.1;

                    // Settling time is ~1.1ms (when going from GND to max)
                    // R=4.7k ohm
                    // C=0.1uF
                    // f=10k Hz
                    // peak to peak is ~0.18V (5.5% VCC)
                    //   in this scenario is 2 * error
                    Thread.Sleep(3);

                    int expected = (int)Math.Round(pwm.DutyCycle * 1023.0);
                    AdcValueAround(expected, adc.Read(0));
                }
            }
        }
    }
}
