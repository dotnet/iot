﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Device.Pwm;
using Iot.Device.Adc;
using Iot.Device.Board;
using Xunit;
using Xunit.Abstractions;
using static System.Device.Gpio.Tests.SetupHelpers;

namespace System.Device.Gpio.Tests;

[Trait("feature", "pwm")]
public class PwmTests
{
    private readonly ITestOutputHelper _output;

    public PwmTests(ITestOutputHelper output)
    {
        var board = new RaspberryPiBoard();
        _output = output;

        try
        {
            var isPwm = board.IsPwmActivated();
            _output.WriteLine($"Is PWM overlay actvated? {isPwm}");

            for (int busid = 0; busid < 2; busid++)
            {
                var pin = board.GetOverlayPinAssignmentForPwm(busid);
                if (pin != -1)
                {
                    _output.WriteLine($"PWM overlay pin on channel {busid}: {pin}.");
                }
                else
                {
                    _output.WriteLine($"No PWM pins defined in the overlay for channel {busid}");
                }
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception in checking PWM configuration: {ex}");
        }
    }

    [Fact]
    public void DutyCycle_ReportsValueBack()
    {
        using (PwmChannel pwm = CreatePwmChannel())
        {
            // min and max should report exact values
            pwm.DutyCycle = 0;
            Assert.Equal(0, pwm.DutyCycle);

            pwm.DutyCycle = 1;
            Assert.Equal(1, pwm.DutyCycle);

            // Some PWM devices can only set duty cycle
            // in increments depending on its resolution.
            // We should not require exact value to be reported.
            pwm.DutyCycle = 0.3;
            DutyCycleAround(0.3, pwm.DutyCycle);

            pwm.DutyCycle = 0.6;
            DutyCycleAround(0.6, pwm.DutyCycle);
        }
    }

    [Fact]
    public void Frequency_ReportsValueBack()
    {
        using (PwmChannel pwm = CreatePwmChannel(300))
        {
            Assert.Equal(300, pwm.Frequency);

            pwm.Frequency = 100;
            Assert.Equal(100, pwm.Frequency);

            pwm.Frequency = 200;
            Assert.Equal(200, pwm.Frequency);

            pwm.Frequency = 500;
            Assert.Equal(500, pwm.Frequency);
        }
    }

    [Fact]
    [Trait("feature", "spi")]
    public void ChangingFrequency_UpdatesDutyCycle()
    {
        // choice of frequencies is not random
        // 9k -> 20k makes sure that 0.5 duty cycle must be updated
        // otherwise it will go out of range in the driver (at least on Linux)

        // 9k is because DACs (or PWM + low pass filter)
        // settling time calculation was done for 10k and 9k was close enough
        // to not change calculations significantly
        using (PwmChannel pwm = CreatePwmChannel(9000))
        using (var adc = CreateAdc())
        {
            // Let the analog value to settle
            Thread.Sleep(3);
            AdcValueAround(HalfAdc, adc.Read(0));

            pwm.Frequency = 20000;
            Thread.Sleep(3);
            AdcValueAround(HalfAdc, adc.Read(0));
        }
    }

    [Fact]
    public void InvalidDutyCycle_ThrowsAndDoesNotModifyValue()
    {
        using (PwmChannel pwm = CreatePwmChannel())
        {
            pwm.DutyCycle = 0.3;

            Assert.Throws<ArgumentOutOfRangeException>(() => pwm.DutyCycle = -0.01);
            DutyCycleAround(0.3, pwm.DutyCycle);

            Assert.Throws<ArgumentOutOfRangeException>(() => pwm.DutyCycle = 1.01);
            DutyCycleAround(0.3, pwm.DutyCycle);
        }
    }

    [Fact]
    public void InvalidFrequency_ThrowsAndDoesNotModifyValue()
    {
        using (PwmChannel pwm = CreatePwmChannel())
        {
            pwm.Frequency = 123;

            Assert.Throws<ArgumentOutOfRangeException>(() => pwm.Frequency = -1);
            Assert.Equal(123, pwm.Frequency);

            Assert.Throws<ArgumentOutOfRangeException>(() => pwm.DutyCycle = int.MinValue);
            Assert.Equal(123, pwm.Frequency);
        }
    }
}
