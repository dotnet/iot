// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Common
{
    /// <summary>
    /// Helpers for number.
    /// </summary>
    internal static class NumberHelper
    {
        /// <summary>
        /// BCD To decimal
        /// </summary>
        /// <param name="bcd">BCD Code</param>
        /// <returns>decimal</returns>
        public static int Bcd2Dec(byte bcd)
        {
            return ((bcd >> 4) * 10) + (bcd % 16);
        }

        /// <summary>
        /// BCD To decimal
        /// </summary>
        /// <param name="bcds">BCD Code</param>
        /// <returns>decimal</returns>
        public static int Bcd2Dec(byte[] bcds)
        {
            int result = 0;
            foreach (byte bcd in bcds)
            {
                result *= 100;
                result += Bcd2Dec(bcd);
            }

            return result;
        }

        /// <summary>
        /// Decimal To BCD
        /// </summary>
        /// <param name="dec">decimal</param>
        /// <returns>BCD Code</returns>
        public static byte Dec2Bcd(int dec)
        {
            if ((dec > 99) || (dec < 0))
            {
                throw new ArgumentException($"{nameof(dec)}, encoding value can't be more than 99");
            }

            return (byte)(((dec / 10) << 4) + (dec % 10));
        }
    }
}
