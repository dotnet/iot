// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device
{
    /// <summary>
    /// Helpers for number.
    /// </summary>
    internal static class NumberTool
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
        /// Decimal To BCD
        /// </summary>
        /// <param name="dec">decimal</param>
        /// <returns>BCD Code</returns>
        public static byte Dec2Bcd(int dec)
        {
            return (byte)(((dec / 10) << 4) + (dec % 10));
        }
    }
}
