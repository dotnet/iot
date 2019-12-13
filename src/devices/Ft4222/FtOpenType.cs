// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// How to open the FTDI chip
    /// </summary>
    internal enum FtOpenType
    {
        /// <summary>
        /// Open by serial number
        /// </summary>
        OpenBySerialNumber = 1,

        /// <summary>
        /// Open by description
        /// </summary>
        OpenByDescription = 2,

        /// <summary>
        /// Open by location
        /// </summary>
        OpenByLocation = 4,
    }
}
