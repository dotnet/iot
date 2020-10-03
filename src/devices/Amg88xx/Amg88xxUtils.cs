// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnitsNet;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// This class contains utilities for working with AMG88xx devices.
    /// </summary>
    public static class Amg88xxUtils
    {
        /// <summary>
        /// Converts a raw thermistor reading into a temperature.
        /// </summary>
        /// <param name="tl">Reading low byte</param>
        /// <param name="th">Reading high byte</param>
        /// <returns>Temperature reading</returns>
        public static Temperature ConvertThermistorReading(byte tl, byte th)
        {
            int reading = (th & 0x7) << 8 | tl;
            reading = th >> 3 == 0 ? reading : -reading;
            // The temperature is encoded as a 12 bit value with a sign.
            // The LSB is equivalent to 0.0625℃.
            return Temperature.FromDegreesCelsius(reading * 0.0625);
        }

        /// <summary>
        /// Converts a temperature from two's complements format into a floating-point form.
        /// </summary>
        /// <param name="tl">Reading low byte</param>
        /// <param name="th">Reading high byte</param>
        /// <returns>Temperature reading</returns>
        public static Temperature ConvertToTemperature(byte tl, byte th)
        {
            int reading = (th & 0x7) << 8 | tl;
            reading = th >> 3 == 0 ? reading : -(~(reading - 1) & 0x7ff);
            // The temperature of each pixel is encoded as a 12 bit value in two's complement form.
            // The LSB is equivalent to 0.25℃
            return Temperature.FromDegreesCelsius(reading * 0.25);
        }

        /// <summary>
        /// Converts a temperature to a two's complement representation (low- and high-byte).
        /// </summary>
        /// <param name="temperature">Temperature</param>
        /// <returns>Two's complement representation</returns>
        public static (byte, byte) ConvertFromTemperature(Temperature temperature)
        {
            // The temperature of each pixel is encoded as a 12 bit value in two's complement form.
            // The LSB is equivalent to 0.25℃
            var t = (int)(temperature.DegreesCelsius / 0.25);
            if (temperature.DegreesCelsius < 0)
            {
                t = ~(0x1000 - t) + 1;
            }

            return ((byte)(t & 0xff), (byte)((t >> 8) & 0x0f));
        }
    }
}