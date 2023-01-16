// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// IS31FL3730 display mode, controlling which matrices are used.
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// Enable matrix one only.
        /// </summary>
        MatrixOneOnly = 0x00,

        /// <summary>
        /// Enable matrix two only.
        /// </summary>
        MatrixTwoOnly = 0x08,

        /// <summary>
        /// Enable matrix one and two.
        /// </summary>
        MatrixOneAndTwo = 0x18,
    }
}
