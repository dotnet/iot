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
    public enum MatrixMode
    {
        Size8x8,
        Size7x9,
        Size6x10,
        Size5x11
    }
}
