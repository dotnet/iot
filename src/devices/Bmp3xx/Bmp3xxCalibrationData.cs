// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The factory calibration coefficients of a BMP3xx sensor and the compensation formulas that use them.
    /// </summary>
    /// <remarks>
    /// Layout: BMP390 datasheet (BST-BMP390-DS002), section 3.11.1, table 24 (21 bytes from register 0x31, little-endian).
    /// Scaling and formulas: same datasheet, appendix sections 8.4 to 8.6. The BMP388 datasheet defines the same.
    /// The coefficients are scaled with <see cref="Math.ScaleB(double, int)"/>, which multiplies by a power of two exactly.
    /// </remarks>
    internal sealed class Bmp3xxCalibrationData
    {
        /// <summary>
        /// The number of calibration bytes, starting at register 0x31.
        /// </summary>
        public const int Length = 21;

        private Bmp3xxCalibrationData(ReadOnlySpan<byte> bytes)
        {
            T1 = Math.ScaleB(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(0, 2)), 8);
            T2 = Math.ScaleB(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(2, 2)), -30);
            T3 = Math.ScaleB((sbyte)bytes[4], -48);
            P1 = Math.ScaleB(BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(5, 2)) - 16384, -20);
            P2 = Math.ScaleB(BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(7, 2)) - 16384, -29);
            P3 = Math.ScaleB((sbyte)bytes[9], -32);
            P4 = Math.ScaleB((sbyte)bytes[10], -37);
            P5 = Math.ScaleB(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(11, 2)), 3);
            P6 = Math.ScaleB(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(13, 2)), -6);
            P7 = Math.ScaleB((sbyte)bytes[15], -8);
            P8 = Math.ScaleB((sbyte)bytes[16], -15);
            P9 = Math.ScaleB(BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(17, 2)), -48);
            P10 = Math.ScaleB((sbyte)bytes[19], -48);
            P11 = Math.ScaleB((sbyte)bytes[20], -65);
        }

        // Scaled coefficients, named after the datasheet's PAR_T1..PAR_P11.
        public double T1 { get; }
        public double T2 { get; }
        public double T3 { get; }
        public double P1 { get; }
        public double P2 { get; }
        public double P3 { get; }
        public double P4 { get; }
        public double P5 { get; }
        public double P6 { get; }
        public double P7 { get; }
        public double P8 { get; }
        public double P9 { get; }
        public double P10 { get; }
        public double P11 { get; }

        /// <summary>
        /// Creates the calibration data from the 21 bytes read from register 0x31 onwards.
        /// </summary>
        /// <param name="bytes">The calibration bytes.</param>
        /// <returns>The scaled calibration coefficients.</returns>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is not <see cref="Length"/> bytes long.</exception>
        public static Bmp3xxCalibrationData Parse(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != Length)
            {
                throw new ArgumentException($"Expected {Length} calibration bytes, got {bytes.Length}.", nameof(bytes));
            }

            return new Bmp3xxCalibrationData(bytes);
        }

        /// <summary>
        /// Converts a raw temperature reading to degrees Celsius. The result is not range-checked.
        /// </summary>
        /// <param name="rawTemperature">The unsigned 24-bit raw temperature from the data registers.</param>
        /// <returns>The temperature in degrees Celsius.</returns>
        public double CompensateTemperature(uint rawTemperature)
        {
            double difference = rawTemperature - T1;
            return (difference * T2) + (difference * difference * T3);
        }

        /// <summary>
        /// Converts a raw pressure reading to pascals. The result is not range-checked.
        /// </summary>
        /// <param name="rawPressure">The unsigned 24-bit raw pressure from the data registers.</param>
        /// <param name="temperatureCelsius">The compensated temperature of the same measurement, from <see cref="CompensateTemperature"/>.</param>
        /// <returns>The pressure in pascals.</returns>
        public double CompensatePressure(uint rawPressure, double temperatureCelsius)
        {
            double t = temperatureCelsius;
            double p = rawPressure;

            double offset = P5 + (P6 * t) + (P7 * t * t) + (P8 * t * t * t);
            double sensitivity = p * (P1 + (P2 * t) + (P3 * t * t) + (P4 * t * t * t));
            double nonLinearity = (p * p * (P9 + (P10 * t))) + (p * p * p * P11);

            return offset + sensitivity + nonLinearity;
        }
    }
}
