// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display.Pcd8544Enums
{
    /// <summary>
    /// Temperature set for the PCD8544
    /// </summary>
    public enum Temperature
    {
        /// <summary>Temperature Coefficient 0</summary>
        Coefficient0 = 0b0000_0100,

        /// <summary>Temperature Coefficient 1</summary>
        Coefficient1 = 0b0000_0101,

        /// <summary>Temperature Coefficient 2</summary>
        Coefficient2 = 0b0000_0110,

        /// <summary>Temperature Coefficient 3</summary>
        Coefficient3 = 0b0000_0111,
    }
}
