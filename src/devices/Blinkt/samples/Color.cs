// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;

namespace Iot.Device.Blinkt.Samples
{
    internal static class ColorExtensions
    {
        /// <summary>
        /// Converts HSV to RGB.
        /// </summary>
        /// <param name="hue">Hue</param>
        /// <param name="saturation">Saturation</param>
        /// <param name="value">Value</param>
        /// <returns>RGB Color</returns>
        public static Color HsvToRgb(double hue, double saturation, double value)
        {
            double c = value * saturation;
            double huePrime = hue / (60 / 360.0);
            double x = c * (1 - Math.Abs(huePrime % 2 - 1));

            double r;
            double g;
            double b;

            if (0 <= huePrime && huePrime <= 1)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (1 < huePrime && huePrime <= 2)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (2 < huePrime && huePrime <= 3)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (3 < huePrime && huePrime <= 4)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (4 < huePrime && huePrime <= 5)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (5 < huePrime && huePrime <= 6)
            {
                r = c;
                g = 0;
                b = x;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }

            return Color.FromArgb(0, Expand(r), Expand(g), Expand(b));

            static byte Expand(double color) => (byte)(color * 255);
        }
    }
}
