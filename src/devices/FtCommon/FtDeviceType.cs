// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// List of FTDI device types
    /// </summary>
    public enum FtDeviceType
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

        /// <summary>
        /// FT900 Device
        /// </summary>
        Ft900,

        /// <summary>
        /// FT930 Device
        /// </summary>
        Ft930,

        /// <summary>
        /// FTUMFTPD3A Device
        /// </summary>
        FtUmftpd3A,

        /// <summary>
        /// FT2233HP Device
        /// </summary>
        Ft2233HP,

        /// <summary>
        /// FT4233HP Device
        /// </summary>
        Ft4233HP,

        /// <summary>
        /// FT2232HP Device
        /// </summary>
        Ft2232HP,

        /// <summary>
        /// FT4232HP Device
        /// </summary>
        Ft4232HP,

        /// <summary>
        /// FT233HP Device
        /// </summary>
        Ft233HP,

        /// <summary>
        /// FT232HP Device
        /// </summary>
        Ft232HP,

        /// <summary>
        /// FT2232HA Device
        /// </summary>
        Ft2232HA,

        /// <summary>
        /// FT4232HA Device
        /// </summary>
        Ft4232HA,
    }
}
