// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.IS31FL3730
{
    /// <summary>
    /// Matrix layout mode for IS31FL3730 LED Matrix controllers
    /// </summary>
    public enum MatrixLayout
    {
        /// <summary>
        /// 8x8 LED Matrix mode.
        /// </summary>
        Matrix8by8,

        /// <summary>
        /// 7x9 LED Matrix mode.
        /// </summary>
        Matrix7by9,

        /// <summary>
        /// 6x10 LED Matrix mode.
        /// </summary>
        Matrix6by10,

        /// <summary>
        /// 5x11 LED Matrix mode.
        /// /// </summary>
        Matrix5by11
    }
}
