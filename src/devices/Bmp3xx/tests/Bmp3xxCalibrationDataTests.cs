// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Xunit;

namespace Iot.Device.Bmp3xx.Tests
{
    public class Bmp3xxCalibrationDataTests
    {
        // Calibration sets used by the reference vectors (21 bytes from register 0x31, hex).
        // A and B: coefficients reported for two real BMP388 sensors. C: set A with the signed fields' signs flipped.
        // E1: read from a real BMP390 (Adafruit board); its vectors V1..V5 use raw readings taken from that chip.
        private const string SetA = "3f6b3849f6ca0092f623007e62e575f3f6c54213c4";
        private const string SetB = "0a6bb449f60cff4af323001765f57af3f6d63f1dc4";
        private const string SetC = "3f6b38490a36ff6e09ddfb7e62e5750d0a3bbded3c";
        private const string SetE1 = "4b6b034cf9151e6817060121480d5a03fa531008f5";

        // Expected values come from the manufacturer's open-source reference driver (vectors V1..V15), which computes
        // in single precision. They differ from this binding's double-precision results by up to 5e-10 °C and 8e-5 Pa;
        // the tolerances sit well above that and well below the sensor's resolution (0.00015 °C, 0.016 Pa).
        private const double TemperatureTolerance = 1e-6; // °C
        private const double PressureTolerance = 1e-3; // Pa

        [Fact]
        public void Parse_ScalesEachCoefficient()
        {
            // Each field holds a raw value chosen to expose sign and width mistakes:
            // T1 = 0x8001 (negative if read as signed), T2 = 0xC000, T3 = -128, P1 = -32768, P2 = 32767, P3 = -1,
            // P4 = 127, P5 = 0xFFFF (-1 if read as signed), P6 = 0x8000, P7 = -2, P8 = 3, P9 = -12345, P10 = -100, P11 = 100.
            byte[] bytes = Convert.FromHexString("018000c0800080ff7fff7fffff0080fe03c7cf9c64");

            var calibration = Bmp3xxCalibrationData.Parse(bytes);

            // Scaling from the BMP390 datasheet, appendix 8.4. Powers of two are exact in a double.
            Assert.Equal(32769 * Math.Pow(2, 8), calibration.T1);
            Assert.Equal(49152 / Math.Pow(2, 30), calibration.T2);
            Assert.Equal(-128 / Math.Pow(2, 48), calibration.T3);
            Assert.Equal((-32768 - Math.Pow(2, 14)) / Math.Pow(2, 20), calibration.P1);
            Assert.Equal((32767 - Math.Pow(2, 14)) / Math.Pow(2, 29), calibration.P2);
            Assert.Equal(-1 / Math.Pow(2, 32), calibration.P3);
            Assert.Equal(127 / Math.Pow(2, 37), calibration.P4);
            Assert.Equal(65535 * Math.Pow(2, 3), calibration.P5);
            Assert.Equal(32768 / Math.Pow(2, 6), calibration.P6);
            Assert.Equal(-2 / Math.Pow(2, 8), calibration.P7);
            Assert.Equal(3 / Math.Pow(2, 15), calibration.P8);
            Assert.Equal(-12345 / Math.Pow(2, 48), calibration.P9);
            Assert.Equal(-100 / Math.Pow(2, 48), calibration.P10);
            Assert.Equal(100 / Math.Pow(2, 65), calibration.P11);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(20)]
        [InlineData(22)]
        public void Parse_RejectsWrongLength(int length)
        {
            Assert.Throws<ArgumentException>(() => Bmp3xxCalibrationData.Parse(new byte[length]));
        }

        [Theory]
        [InlineData("V1", SetE1, 8201728u, 21.172585163)]
        [InlineData("V2", SetE1, 8201472u, 21.167960675)]
        [InlineData("V3", SetE1, 8201984u, 21.177209649)]
        [InlineData("V6", SetA, 7028480u, 0.000000000)]
        [InlineData("V7", SetA, 8464793u, 25.000007690)]
        [InlineData("V8", SetA, 10489938u, 59.999994463)]
        [InlineData("V11", SetB, 8155705u, 20.000000971)]
        [InlineData("V12", SetB, 11610253u, 79.999992908)]
        [InlineData("V13", SetC, 7314736u, 4.999999257)]
        [InlineData("V14", SetC, 9309276u, 40.000008359)]
        [InlineData("V15", SetA, 0x800000u, 23.677637157)]
        public void CompensateTemperature_MatchesReference(string vector, string calibrationHex, uint rawTemperature, double expectedCelsius)
        {
            var calibration = Bmp3xxCalibrationData.Parse(Convert.FromHexString(calibrationHex));

            double actual = calibration.CompensateTemperature(rawTemperature);

            Assert.True(Math.Abs(expectedCelsius - actual) <= TemperatureTolerance, $"{vector}: expected {expectedCelsius} °C, got {actual} °C");
        }

        [Theory]
        [InlineData("V1", SetE1, 6258176u, 21.172585163, 101499.057700099)]
        [InlineData("V2", SetE1, 6257920u, 21.167960675, 101500.127897664)]
        [InlineData("V3", SetE1, 6257664u, 21.177209649, 101504.539749819)]
        [InlineData("V4", SetE1, 6258176u, 21.177209649, 101500.171557017)]
        [InlineData("V5", SetE1, 6257408u, 21.172585163, 101505.609924687)]
        [InlineData("V6", SetA, 6737211u, 0.000000000, 100000.000532955)]
        [InlineData("V7", SetA, 7033326u, 25.000007690, 101324.996390588)]
        [InlineData("V8", SetA, 8181227u, 59.999994463, 90000.006120835)]
        [InlineData("V11", SetB, 7321232u, 20.000000971, 97999.995818342)]
        [InlineData("V12", SetB, 11823592u, 79.999992908, 30999.993900953)]
        [InlineData("V13", SetC, 8205337u, 4.999999257, 69999.993475021)]
        [InlineData("V14", SetC, 6429257u, 40.000008359, 109999.996364267)]
        [InlineData("V15", SetA, 0x800000u, 23.677637157, 79898.812328695)]
        public void CompensatePressure_MatchesReference(string vector, string calibrationHex, uint rawPressure, double temperatureCelsius, double expectedPascals)
        {
            var calibration = Bmp3xxCalibrationData.Parse(Convert.FromHexString(calibrationHex));

            double actual = calibration.CompensatePressure(rawPressure, temperatureCelsius);

            Assert.True(Math.Abs(expectedPascals - actual) <= PressureTolerance, $"{vector}: expected {expectedPascals} Pa, got {actual} Pa");
        }

        [Fact]
        public void CompensateTemperature_DoesNotClamp()
        {
            // V9: below the sensor's -40 °C limit. The reference implementation clamps to -40 °C; the range check
            // belongs to the caller, so the formula itself must return the unclamped value (datasheet formula, float64).
            var calibration = Bmp3xxCalibrationData.Parse(Convert.FromHexString(SetA));

            double actual = calibration.CompensateTemperature(4464058u);

            Assert.True(Math.Abs(-45.000007735 - actual) <= TemperatureTolerance, $"got {actual} °C");
        }

        [Fact]
        public void CompensatePressure_DoesNotClamp()
        {
            // V10: above the sensor's 125000 Pa limit. The reference implementation clamps to 125000 Pa.
            var calibration = Bmp3xxCalibrationData.Parse(Convert.FromHexString(SetA));

            double actual = calibration.CompensatePressure(5463084u, 25.000007690);

            Assert.True(Math.Abs(125999.996645801 - actual) <= PressureTolerance, $"got {actual} Pa");
        }
    }
}
