// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd1331
{
    /// <summary>
    /// COM Scan Direction Remap
    /// </summary>
    public enum ScanMode
    {
        /// <summary>
        /// Scan direction from COM0 to COM63
        /// </summary>
        FromColumn0 = 0,

        /// <summary>
        /// Scan direction from COM63 to COM0
        /// </summary>
        ToColumn0 = 1
    }
}
