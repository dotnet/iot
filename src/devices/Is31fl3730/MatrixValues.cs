// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Matrix Values.
    /// </summary>
    public static class MatrixValues
    {
        /// <summary>
        /// Arbitrary 8-bit value to write to Update Column Register, as required by datasheet.
        /// </summary>
        public static byte EightBitValue = 8;

        /// <summary>
        /// Matrix one decimal point mask.
        /// </summary>
        public static byte MatrixOneDecimalMask = 128;

        /// <summary>
        /// Matrix one decimal point row.
        /// </summary>
        public static byte MatrixOneDecimalRow = 6;

        /// <summary>
        /// Matrix two mask for decimal point.
        /// </summary>
        public static byte MatrixTwoDecimalMask = 64;

        /// <summary>
        /// Matrix two decimal point row.
        /// </summary>
        public static byte MatrixTwoDecimalRow = 7;
    }
}
