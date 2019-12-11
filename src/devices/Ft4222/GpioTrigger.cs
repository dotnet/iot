// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// Triggers for the GPIO events
    /// </summary>
    [Flags]
    internal enum GpioTrigger
    {
        None = 0x00,
        Rising = 0x01,
        Falling = 0x02,
        LevelHigh = 0x04,
        LevelLow = 0X08
    }
}
