// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Iot.Device.Bmp3xx.Tests
{
    /// <summary>
    /// Construction and configuration of <see cref="Bmp3xxBase"/>, driven through <see cref="SimulatedBmp3xx"/>.
    /// </summary>
    public class Bmp3xxBaseTests
    {
        private const byte Bmp390ChipId = 0x60;
        private const byte Bmp388ChipId = 0x50;
        private const byte PowerControl = 0x1B;
        private const byte Oversampling = 0x1C;
        private const byte OutputDataRate = 0x1D;
        private const byte Configuration = 0x1F;
        private const byte Command = 0x7E;

        private static readonly byte[] Calibration = Convert.FromHexString("3f6b3849f6ca0092f623007e62e575f3f6c54213c4");

        [Fact]
        public void Constructor_Bmp390_AcceptsChipId60()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);

            using var sensor = new Bmp390(chip);

            Assert.Equal(Bmp3xxPowerMode.Sleep, sensor.ReadPowerMode());
        }

        [Fact]
        public void Constructor_Bmp388_AcceptsChipId50()
        {
            using var chip = new SimulatedBmp3xx(Bmp388ChipId, Calibration);

            using var sensor = new Bmp388(chip);

            Assert.Equal(Bmp3xxPowerMode.Sleep, sensor.ReadPowerMode());
        }

        [Theory]
        [InlineData(Bmp388ChipId)]
        [InlineData(0x58)] // BMP280: a common mix-up on cheap breakout boards
        public void Constructor_WrongChipId_ThrowsIOException(byte chipId)
        {
            using var chip = new SimulatedBmp3xx(chipId, Calibration);

            var exception = Assert.Throws<IOException>(() => new Bmp390(chip));

            Assert.Contains("0x60", exception.Message);
            Assert.Contains($"0x{chipId:X2}", exception.Message);
        }

        [Fact]
        public void Constructor_NullDevice_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Bmp390(null!));
        }

        [Fact]
        public void Constructor_ResetsOnce_AndWritesDefaults()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);

            using var sensor = new Bmp390(chip);

            Assert.Equal(1, chip.ResetCount);
            Assert.Equal((Command, (byte)0xB6), chip.WriteLog.First());

            // Defaults: pressure and temperature x1, filter off, 200 Hz, both sensors enabled, sleep mode.
            Assert.Equal(0x00, chip.GetRegister(Oversampling));
            Assert.Equal(0x00, chip.GetRegister(Configuration));
            Assert.Equal(0x00, chip.GetRegister(OutputDataRate));
            Assert.Equal(0x03, chip.GetRegister(PowerControl));
            Assert.Equal(Bmp3xxOversampling.X1, sensor.PressureSampling);
            Assert.Equal(Bmp3xxOversampling.X1, sensor.TemperatureSampling);
            Assert.Equal(Bmp3xxFilterCoefficient.Off, sensor.FilterCoefficient);
            Assert.Equal(Bmp3xxOutputDataRate.Period5Milliseconds, sensor.OutputDataRate);
        }

        [Fact]
        public void PressureSampling_SetX8_WritesOsrBitsAndKeepsTemperatureBits()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            sensor.TemperatureSampling = Bmp3xxOversampling.X2;

            sensor.PressureSampling = Bmp3xxOversampling.X8;

            Assert.Equal((1 << 3) | 3, chip.GetRegister(Oversampling));
            Assert.Equal(Bmp3xxOversampling.X8, sensor.PressureSampling);
            Assert.Equal(Bmp3xxOversampling.X2, sensor.TemperatureSampling);
        }

        [Fact]
        public void TemperatureSampling_SetX32_WritesOsrBitsAndKeepsPressureBits()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            sensor.PressureSampling = Bmp3xxOversampling.X4;

            sensor.TemperatureSampling = Bmp3xxOversampling.X32;

            Assert.Equal((5 << 3) | 2, chip.GetRegister(Oversampling));
            Assert.Equal(Bmp3xxOversampling.X32, sensor.TemperatureSampling);
            Assert.Equal(Bmp3xxOversampling.X4, sensor.PressureSampling);
        }

        [Fact]
        public void FilterCoefficient_RoundTrips()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            sensor.FilterCoefficient = Bmp3xxFilterCoefficient.Coefficient15;

            Assert.Equal(4 << 1, chip.GetRegister(Configuration));
            Assert.Equal(Bmp3xxFilterCoefficient.Coefficient15, sensor.FilterCoefficient);
        }

        [Fact]
        public void OutputDataRate_RoundTrips()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            sensor.OutputDataRate = Bmp3xxOutputDataRate.Period1280Milliseconds;

            Assert.Equal(0x08, chip.GetRegister(OutputDataRate));
            Assert.Equal(Bmp3xxOutputDataRate.Period1280Milliseconds, sensor.OutputDataRate);
        }

        [Fact]
        public void Settings_InvalidValue_ThrowsArgumentOutOfRange()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            Assert.Throws<ArgumentOutOfRangeException>(() => sensor.PressureSampling = (Bmp3xxOversampling)6);
            Assert.Throws<ArgumentOutOfRangeException>(() => sensor.TemperatureSampling = (Bmp3xxOversampling)6);
            Assert.Throws<ArgumentOutOfRangeException>(() => sensor.FilterCoefficient = (Bmp3xxFilterCoefficient)8);
            Assert.Throws<ArgumentOutOfRangeException>(() => sensor.OutputDataRate = (Bmp3xxOutputDataRate)0x12);
            Assert.Throws<ArgumentOutOfRangeException>(() => sensor.SetPowerMode((Bmp3xxPowerMode)2));
        }

        [Fact]
        public void SetPowerMode_Forced_KeepsSensorsEnabled()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            sensor.SetPowerMode(Bmp3xxPowerMode.Forced);

            Assert.Equal((PowerControl, (byte)0x13), chip.WriteLog.Last());
        }

        [Fact]
        public void SetPowerMode_Normal_IsReportedByReadPowerMode()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            sensor.SetPowerMode(Bmp3xxPowerMode.Normal);

            Assert.Equal(Bmp3xxPowerMode.Normal, sensor.ReadPowerMode());
            Assert.Equal(0x33, chip.GetRegister(PowerControl));
        }

        [Fact]
        public void SetPowerMode_FromNormalToForced_GoesThroughSleep()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            sensor.SetPowerMode(Bmp3xxPowerMode.Normal);
            chip.WriteLog.Clear();

            sensor.SetPowerMode(Bmp3xxPowerMode.Forced);

            Assert.Equal(new (byte, byte)[] { (PowerControl, 0x03), (PowerControl, 0x13) }, chip.WriteLog);
        }

        [Fact]
        public void SetPowerMode_FromNormalToForced_StartsAMeasurement()
        {
            // The forced-mode write must wait until the measurement running in normal mode has finished.
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            sensor.SetPowerMode(Bmp3xxPowerMode.Normal);
            chip.RawPressure = 7033326;
            chip.RawTemperature = 8464793;

            var result = sensor.Read(); // in normal mode: no measurement is started
            sensor.SetPowerMode(Bmp3xxPowerMode.Forced);

            Assert.NotNull(result.Pressure);
            Assert.Contains((PowerControl, (byte)0x13), chip.WriteLog);
            Assert.Equal(0x03, chip.GetRegister(PowerControl)); // the forced measurement ran and the sensor is back asleep
            Assert.Equal(0x70, chip.GetRegister(0x03) & 0x70); // and its data is ready
        }

        [Fact]
        public void SetPowerMode_NormalSleepNormal_EndsInNormalMode()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            sensor.SetPowerMode(Bmp3xxPowerMode.Normal);

            sensor.SetPowerMode(Bmp3xxPowerMode.Sleep);
            sensor.SetPowerMode(Bmp3xxPowerMode.Normal);

            Assert.Equal(Bmp3xxPowerMode.Normal, sensor.ReadPowerMode());
        }

        [Fact]
        public void SetPowerMode_Normal_WithConfigurationError_Throws_AndReturnsToSleep()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration) { ConfigurationErrorOnNormalMode = true };
            using var sensor = new Bmp390(chip);

            var exception = Assert.Throws<InvalidOperationException>(() => sensor.SetPowerMode(Bmp3xxPowerMode.Normal));

            Assert.Contains("output data rate", exception.Message);
            Assert.Equal(Bmp3xxPowerMode.Sleep, sensor.ReadPowerMode());
            Assert.Equal(0x03, chip.GetRegister(PowerControl)); // sensors still enabled
        }

        [Fact]
        public void GetMeasurementDuration_X1X1_Is5ms()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            // 234 + (392 + 2020) + (163 + 2020) = 4829 µs, rounded up (BMP390 datasheet, section 3.9.2).
            Assert.Equal(5, sensor.GetMeasurementDuration());
        }

        [Theory]
        [InlineData(Bmp390ChipId, 70)] // 234 + (392 + 32 * 2020) + (163 + 2 * 2020) = 69469 µs
        [InlineData(Bmp388ChipId, 69)] // 234 + (392 + 32 * 2000) + (313 + 2 * 2000) = 68939 µs (BMP388 datasheet, section 3.9.2)
        public void GetMeasurementDuration_X32X2_MatchesFormula(byte chipId, int expectedMilliseconds)
        {
            using var chip = new SimulatedBmp3xx(chipId, Calibration);
            using Bmp3xxBase sensor = chipId == Bmp390ChipId ? new Bmp390(chip) : new Bmp388(chip);
            sensor.PressureSampling = Bmp3xxOversampling.X32;
            sensor.TemperatureSampling = Bmp3xxOversampling.X2;

            Assert.Equal(expectedMilliseconds, sensor.GetMeasurementDuration());
        }

        [Fact]
        public void Reset_WritesB6ToCommandRegister_AndKeepsSettings()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            sensor.PressureSampling = Bmp3xxOversampling.X8;
            sensor.FilterCoefficient = Bmp3xxFilterCoefficient.Coefficient3;
            chip.WriteLog.Clear();

            sensor.Reset();

            Assert.Equal(2, chip.ResetCount);
            Assert.Equal((Command, (byte)0xB6), chip.WriteLog.First());
            Assert.Equal(0x03, chip.GetRegister(Oversampling));
            Assert.Equal(2 << 1, chip.GetRegister(Configuration));
            Assert.Equal(Bmp3xxOversampling.X8, sensor.PressureSampling);
        }

        [Fact]
        public void Reset_CommandError_ThrowsIOException()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);
            chip.SoftResetFails = true;

            Assert.Throws<IOException>(() => sensor.Reset());
        }

        [Fact]
        public void ReadStatus_ReportsFlags()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            using var sensor = new Bmp390(chip);

            Assert.Equal(Bmp3xxStatus.CommandReady, sensor.ReadStatus());
        }

        [Fact]
        public void ReadErrors_ReportsFlags_AndReadingClearsThem()
        {
            using var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration) { ConfigurationErrorOnNormalMode = true };
            using var sensor = new Bmp390(chip);
            chip.Write(new byte[] { PowerControl, 0x33 }); // normal mode written behind the binding's back

            Assert.Equal(Bmp3xxErrors.Configuration, sensor.ReadErrors());
            Assert.Equal(Bmp3xxErrors.None, sensor.ReadErrors());
        }

        [Fact]
        public void Dispose_DisposesI2cDevice_AndBlocksFurtherUse()
        {
            var chip = new SimulatedBmp3xx(Bmp390ChipId, Calibration);
            var sensor = new Bmp390(chip);

            sensor.Dispose();
            sensor.Dispose(); // a second call is harmless

            Assert.True(chip.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => sensor.ReadStatus());
            Assert.Throws<ObjectDisposedException>(() => sensor.PressureSampling = Bmp3xxOversampling.X2);
        }
    }
}
