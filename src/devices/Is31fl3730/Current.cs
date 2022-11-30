// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Multiplexing.Utility;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    public enum Current
    {
        /// <summary>
        /// 5 mA
        /// </summary>
        CmA5 = 0b1000,

        /// <summary>
        /// 10 mA
        /// </summary>
        CmA10 = 0b1001,

        /// <summary>
        /// 35 mA
        /// </summary>
        CmA35 = 0b1110,

        /// <summary>
        /// 40 mA
        /// </summary>
        CmA40 = 0b0,

        /// <summary>
        /// 45 mA
        /// </summary>
        CMA45 = 0b0001,

        /// <summary>
        /// 75 mA
        /// </summary>
        CmA75 = 0b0111,
    }
}
