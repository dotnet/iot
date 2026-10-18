// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.Common;
using UnitsNet;
using Xunit;

namespace Iot.Device.Bmp3xx.Tests
{
    /// <summary>
    /// The reading path of <see cref="Bmp3xxBase"/>, driven through <see cref="SimulatedBmp3xx"/>.
    /// Raw values and expected results are the reference vectors from the manufacturer's open-source driver
    /// (see <see cref="Bmp3xxCalibrationDataTests"/>).
    /// </summary>
    public class Bmp3xxReadTests
    {
        private const byte PowerControl = 0x1B;
        private const double TemperatureTolerance = 1e-6; // °C
        private const double PressureTolerance = 1e-3; // Pa

        // Calibration set A with vector V7: about 25 °C and 1013.25 hPa.
        private const uint V7RawPressure = 7033326;
        private const uint V7RawTemperature = 8464793;
        private const double V7Celsius = 25.000007690;
        private const double V7Pascals = 101324.996390588;

        private static readonly byte[] Calibration = Convert.FromHexString("3f6b3849f6ca0092f623007e62e575f3f6c54213c4");

        [Fact]
        public void Read_InSleep_TriggersForcedMeasurement_AndReturnsReferenceValues()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);

            var result = sensor.Read();

            Assert.Contains((PowerControl, (byte)0x13), chip.WriteLog); // press_en | temp_en | forced
            AssertTemperature(V7Celsius, result.Temperature);
            AssertPressure(V7Pascals, result.Pressure);
        }

        [Fact]
        public void Read_InNormalMode_DoesNotWritePowerControl()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            sensor.SetPowerMode(Bmp3xxPowerMode.Normal);
            chip.WriteLog.Clear();

            var result = sensor.Read();

            Assert.Empty(chip.WriteLog);
            AssertTemperature(V7Celsius, result.Temperature);
            AssertPressure(V7Pascals, result.Pressure);
        }

        [Fact]
        public void Read_WhenNeverReady_TimesOutAndReturnsNulls()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            chip.NeverBecomesReady = true;
            var stopwatch = Stopwatch.StartNew();

            var result = sensor.Read();

            stopwatch.Stop();
            Assert.Null(result.Temperature);
            Assert.Null(result.Pressure);

            // Timeout = twice the 5 ms measurement time + 10 ms; allow for a slow test machine.
            Assert.InRange(stopwatch.ElapsedMilliseconds, 5, 1000);
        }

        [Fact]
        public async Task ReadAsync_WhenNeverReady_TimesOutAndReturnsNulls()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            chip.NeverBecomesReady = true;

            var result = await sensor.ReadAsync();

            Assert.Null(result.Temperature);
            Assert.Null(result.Pressure);
        }

        [Fact]
        public void Read_OutOfRangeTemperature_ReturnsNullTemperatureAndPressure()
        {
            // V9: -45 °C, below the sensor's -40 °C limit. The pressure (about 1000 hPa) would be in range, but it is
            // computed from that temperature, so it isn't reported either.
            using var chip = CreateChip(5932470, 4464058);
            using var sensor = new Bmp390(chip);

            var result = sensor.Read();

            Assert.Null(result.Temperature);
            Assert.Null(result.Pressure);
        }

        [Fact]
        public void Read_OutOfRangePressure_ReturnsNullPressure()
        {
            // V10: about 1260 hPa, above the sensor's 1250 hPa limit.
            using var chip = CreateChip(5463084, V7RawTemperature);
            using var sensor = new Bmp390(chip);

            var result = sensor.Read();

            AssertTemperature(V7Celsius, result.Temperature);
            Assert.Null(result.Pressure);
        }

        [Fact]
        public async Task ReadAsync_MatchesRead()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);

            var result = await sensor.ReadAsync();

            Assert.Contains((PowerControl, (byte)0x13), chip.WriteLog);
            AssertTemperature(V7Celsius, result.Temperature);
            AssertPressure(V7Pascals, result.Pressure);
        }

        [Fact]
        public void TryReadTemperature_AfterReset_ReturnsFalse()
        {
            // The data registers still hold their reset value 0x800000 (BMP390 datasheet, table 25).
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);

            Assert.False(sensor.TryReadTemperature(out _));
            Assert.False(sensor.TryReadPressure(out _));
        }

        [Fact]
        public void TryReadTemperature_OnlyTemperatureAtResetValue_ReturnsTrue()
        {
            // With this calibration a raw temperature of 0x800000 is a normal 23.68 °C. Only both raw values at
            // their reset value mean "no measurement yet".
            using var chip = CreateChip(V7RawPressure, 0x800000);
            using var sensor = new Bmp390(chip);
            sensor.Read();

            Assert.True(sensor.TryReadTemperature(out var temperature));
            Assert.Equal(23.677637157, temperature.DegreesCelsius, TemperatureTolerance);
        }

        [Fact]
        public void TryReadTemperature_DoesNotTriggerMeasurement()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            sensor.Read();
            chip.WriteLog.Clear();

            Assert.True(sensor.TryReadTemperature(out var temperature));

            Assert.Empty(chip.WriteLog);
            Assert.Equal(V7Celsius, temperature.DegreesCelsius, TemperatureTolerance);
        }

        [Fact]
        public void TryReadPressure_AfterMeasurement_ReturnsTrue()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            sensor.Read();

            Assert.True(sensor.TryReadPressure(out var pressure));
            Assert.Equal(V7Pascals, pressure.Pascals, PressureTolerance);
        }

        [Fact]
        public void TryReadPressure_OutOfRange_ReturnsFalse()
        {
            using var chip = CreateChip(5463084, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            sensor.Read();

            Assert.False(sensor.TryReadPressure(out _));
        }

        [Fact]
        public void TryReadPressure_TemperatureOutOfRange_ReturnsFalse()
        {
            // V9: the pressure is in range, the temperature it is computed from isn't.
            using var chip = CreateChip(5932470, 4464058);
            using var sensor = new Bmp390(chip);
            sensor.Read();

            Assert.False(sensor.TryReadPressure(out _));
        }

        [Fact]
        public void TryReadAltitude_AtSeaLevelPressure_IsNearZero()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            sensor.Read();

            Assert.True(sensor.TryReadAltitude(out var altitude));

            // The pressure is 1013.24996 hPa, 0.00004 hPa below mean sea-level pressure: well under a centimetre.
            Assert.Equal(0, altitude.Meters, 0.01);
        }

        [Fact]
        public void TryReadAltitude_UsesGivenSeaLevelPressure()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);
            sensor.Read();
            var seaLevelPressure = Pressure.FromHectopascals(1100);

            Assert.True(sensor.TryReadAltitude(seaLevelPressure, out var altitude));

            var expected = WeatherHelper.CalculateAltitude(Pressure.FromPascals(V7Pascals), seaLevelPressure, Temperature.FromDegreesCelsius(V7Celsius));
            Assert.Equal(expected.Meters, altitude.Meters, 0.001);
            Assert.True(altitude.Meters > 600, $"About 87 hPa below the given sea-level pressure should be several hundred metres up; got {altitude.Meters} m");
        }

        [Fact]
        public void TryReadAltitude_AfterReset_ReturnsFalse()
        {
            using var chip = CreateChip(V7RawPressure, V7RawTemperature);
            using var sensor = new Bmp390(chip);

            Assert.False(sensor.TryReadAltitude(out _));
        }

        private static SimulatedBmp3xx CreateChip(uint rawPressure, uint rawTemperature) =>
            new SimulatedBmp3xx(0x60, Calibration) { RawPressure = rawPressure, RawTemperature = rawTemperature };

        private static void AssertTemperature(double expectedCelsius, Temperature? actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expectedCelsius, actual.Value.DegreesCelsius, TemperatureTolerance);
        }

        private static void AssertPressure(double expectedPascals, Pressure? actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expectedPascals, actual.Value.Pascals, PressureTolerance);
        }
    }
}
