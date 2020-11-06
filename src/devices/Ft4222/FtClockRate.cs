// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// System clock rate
    /// </summary>
    internal enum FtClockRate
    {
        /// <summary>
        /// 60 MHz
        /// </summary>
        Clock60MHz = 0,

        /// <summary>
        /// 24 MHz
        /// </summary>
        Clock24MHz,

        /// <summary>
        /// 48 MHz
        /// </summary>
        Clock48MHz,

        /// <summary>
        /// 80 MHz
        /// </summary>
        Clock80MHz,
    }
}
