// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.MAX31856
{
    /// <summary>
    /// Register of MAX31856
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// Read Register for CR0
        /// </summary>
        READ_CR0 = 0x00,

        /// <summary>
        /// Read Register for CR1
        /// </summary>
        READ_CR1 = 0x01,

        /// <summary>
        /// Write Register for CR0
        /// </summary>
        WRITE_CR0 = 0x80,

        /// <summary>
        /// Write Register for CR1
        /// </summary>
        WRITE_CR1 = 0x81,

        /// <summary>
        /// Error code for an open thermocouple line which could be from a broken wire or disconnected device
        /// </summary>
        ERROR_OPEN = 0x01, // error code for when the thermocouple is open read on the SR register

         /// <summary>
         /// One shot read on register CR0 with the fault detection enabled
         /// </summary>
        ONESHOT_FAULT_SETTING = 0x90
    }
}
