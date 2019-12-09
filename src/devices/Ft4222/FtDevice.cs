// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace System.Device.Ft4222
{
    /// <summary>
    /// List of FTDI device types
    /// </summary>
    public enum FtDevice
    {
        /// <summary>
        /// FT232B or FT245B device
        /// </summary>
        Ft232BOrFt245B = 0,
        /// <summary>
        /// FT8U232AM or FT8U245AM device
        /// </summary>
        Ft8U232AmOrFTtU245Am,
        /// <summary>
        /// FT8U100AX device
        /// </summary>
        Ft8U100Ax,
        /// <summary>
        /// Unknown device
        /// </summary>
        UnknownDevice,
        /// <summary>
        /// FT2232 device
        /// </summary>
        Ft2232,
        /// <summary>
        /// FT232R or FT245R device
        /// </summary>
        Ft232ROrFt245R,
        /// <summary>
        /// FT2232H device
        /// </summary>
        Ft2232H,
        /// <summary>
        /// FT4232H device
        /// </summary>
        Ft4232H,
        /// <summary>
        /// FT232H device
        /// </summary>
        Ft232H,
        /// <summary>
        /// FT X-Series device
        /// </summary>
        FtXSeries,
        /// <summary>
        /// FT4222 hi-speed device Mode 0 - 2 interfaces
        /// </summary>
        Ft4222HMode0or2With2Interfaces,
        /// <summary>
        /// FT4222 hi-speed device Mode 1 or 2 - 4 interfaces
        /// </summary>
        Ft4222HMode1or2With4Interfaces,
        /// <summary>
        /// FT4222 hi-speed device Mode 3 - 1 interface
        /// </summary>
        Ft4222HMode3With1Interface,
        /// <summary>
        /// OTP programmer board for the FT4222.
        /// </summary>
        Ft4222OtpProgrammerBoard,
    }
}
