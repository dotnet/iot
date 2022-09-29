// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Device.Spi;
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
        MA5 = 0b1000,

        /// <summary>
        /// 10 mA
        /// </summary>
        MA10 = 0b1001,

        /// <summary>
        /// 35 mA
        /// </summary>
        MA35 = 0b1110,

        /// <summary>
        /// 10 mA
        /// </summary>
        MA40 = 0b0,

        /// <summary>
        /// 10 mA
        /// </summary>
        MA45 = 0b0001,

        /// <summary>
        /// 40 mA
        /// </summary>
        MA75 = 0b0111,
    }
}
