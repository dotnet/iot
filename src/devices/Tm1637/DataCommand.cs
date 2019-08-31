// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Tm1637
{
    /// <summary>
    /// Internal registers to be send to the TM1637
    /// </summary>
    internal enum DataCommand
    {
        DataCommandSetting = 0b0100_0000,
        DisplayAndControlCommandSetting = 0b1000_0000,
        AddressCommandSetting = 0b1100_0000,
        ReadKeyScanData = 0b0100_0010,
        FixAddress = 0b0100_0100,
        TestMode = 0b0100_1000,
    }
}
