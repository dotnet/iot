// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display.Internal
{
    /// <summary>
    /// IS31FL3730 matrix driver values.
    /// </summary>
    internal static class Is31fl3730MatrixValues
    {
        /// <summary>
        /// Arbitrary 8-bit value to write to Update Column Register, as required by datasheet.
        /// </summary>
        internal static byte EightBitValue = 8;

        /// <summary>
        /// Matrix one decimal point mask.
        /// </summary>
        internal static byte MatrixOneDecimalMask = 128;

        /// <summary>
        /// Matrix one decimal point row.
        /// </summary>
        internal static byte MatrixOneDecimalRow = 6;

        /// <summary>
        /// Matrix two mask for decimal point.
        /// </summary>
        internal static byte MatrixTwoDecimalMask = 64;

        /// <summary>
        /// Matrix two decimal point row.
        /// </summary>
        internal static byte MatrixTwoDecimalRow = 7;
    }
}
