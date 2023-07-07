// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd1331
{
    /// <summary>
    /// High voltage level (VCOMH) of common pins relative to VCC
    /// </summary>
    public enum VComHDeselectLevel
    {
        /// <summary>0.44 of VCC level</summary>
        VccX044 = 0x00,

        /// <summary>0.52 of VCC level</summary>
        VccX052 = 0x10,

        /// <summary>0.61 of VCC level</summary>
        VccX061 = 0x20,

        /// <summary>0.71 of VCC level</summary>
        VccX071 = 0x30,

        /// <summary>0.83 of VCC level</summary>
        VccX083 = 0x3E
    }
}
