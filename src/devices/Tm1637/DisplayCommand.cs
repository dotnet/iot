// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Tm1637
{
    /// <summary>
    /// Switch on or off the 8 segments LCD
    /// </summary>
    internal enum DisplayCommand
    {
        DisplayOn = 0b1000_1000,
        DisplayOff = 0b1000_0000,
    }
}
