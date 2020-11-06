// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.UFire
{
    /// <summary>
    /// Register with all adress values for μFire ISE (Ion Specific Electrode) Probe Interface controller
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// hardware version
        /// </summary>
        ISE_VERSION_REGISTER = 0x00,

        /// <summary>
        /// mV
        /// </summary>
        ISE_MV_REGISTER = 0x01,

        /// <summary>
        /// temperature in C
        /// </summary>
        ISE_TEMP_REGISTER = 0x05,

        /// <summary>
        /// calibration offset
        /// </summary>
        ISE_CALIBRATE_SINGLE_REGISTER = 0x09,

        /// <summary>
        /// reference high calibration
        /// </summary>
        ISE_CALIBRATE_REFHIGH_REGISTER = 0x0D,

        /// <summary>
        /// reference low calibration
        /// </summary>
        ISE_CALIBRATE_REFLOW_REGISTER = 0x11,

        /// <summary>
        /// reading high calibration
        /// </summary>
        ISE_CALIBRATE_READHIGH_REGISTER = 0x15,

        /// <summary>
        /// reading low calibration
        /// </summary>
        ISE_CALIBRATE_READLOW_REGISTER = 0x19,

        /// <summary>
        /// reference ISE solution
        /// </summary>
        ISE_SOLUTION_REGISTER = 0x1D,

        /// <summary>
        /// buffer
        /// </summary>
        ISE_BUFFER_REGISTER = 0x21,

        /// <summary>
        /// firmware version
        /// </summary>
        ISE_FW_VERSION_REGISTER = 0x25,

        /// <summary>
        /// config register
        /// </summary>
        ISE_CONFIG_REGISTER = 0x26,

        /// <summary>
        /// firmware version
        /// </summary>
        ISE_TASK_REGISTER = 0x27,

        /// <summary>
        /// potential
        /// </summary>
        POTENTIAL_REGISTER = 0x64
    }
}
