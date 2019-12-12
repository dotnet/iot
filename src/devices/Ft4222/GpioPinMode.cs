// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// GPIO Pin Mode called Direction with FTDI
    /// </summary>
    internal enum GpioPinMode : int
    {
        /// <summary>
        /// Output
        /// </summary>
        Output = 0,
        /// <summary>
        /// Input
        /// </summary>
        Input,
    }
}
