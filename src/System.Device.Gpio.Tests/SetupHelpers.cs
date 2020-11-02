// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.Device;
using System.Device.Spi;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

using Iot.Device.Adc;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;

using Xunit;
using UnitsNet;

namespace System.Device.Gpio.Tests
{
    public static class SetupHelpers
    {
        public const int MinAdc = 0;
        public const int MaxAdc = 1023;
        public const int HalfAdc = MaxAdc / 2;

        public static void AdcValueAround(int expected, int actual, int error = 50)
        {
            Assert.InRange(
                actual,
                Math.Max(MinAdc, expected - error),
                Math.Min(MaxAdc, expected + error));
        }

        public static void DutyCycleAround(double expected, double actual, double error = 0.01)
        {
            Assert.InRange(
                actual,
                Math.Max(0, expected - error),
                Math.Min(1, expected + error));
        }

        public static Mcp3008 CreateAdc()
        {
            return new Mcp3008(SpiDevice.Create(new SpiConnectionSettings(0, 0)));
        }

        public static Bme280 CreateBme280()
        {
            var settings = new I2cConnectionSettings(1, Bme280.DefaultI2cAddress);
            var bme280 = new Bme280(I2cDevice.Create(settings));

            // https://github.com/dotnet/iot/issues/753
            bme280.SetPowerMode(Bmx280PowerMode.Forced);

            return bme280;
        }

        private static PwmChannel CreatePwmChannelCore(int frequency, double dutyCycle)
        {
            try
            {
                return PwmChannel.Create(0, 0, frequency, dutyCycle);
            }
            catch (ArgumentException) when (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // PWM is likely not enabled
                // Let's try to run against software PWM implementation
                return new SoftwarePwmChannel(18, frequency, dutyCycle, usePrecisionTimer: true);
            }
        }

        public static PwmChannel CreatePwmChannel(int frequency = 10000, bool stopped = false, double dutyCycle = 0.5)
        {
            var pwm = CreatePwmChannelCore(frequency, dutyCycle);

            if (!stopped)
            {
                pwm.Start();
            }

            return pwm;
        }
    }
}
