// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Button press
    /// </summary>
    public enum ButtonPress
    {
        /// <summary>Short press twice</summary>
        Doubleclick = 0b0100_0000,

        /// <summary>Long press</summary>
        LongPress = 0,
    }
}
