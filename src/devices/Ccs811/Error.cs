// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ccs811
{
    /// <summary>
    /// The possible errors from the error register
    /// </summary>
    [Flags]
    public enum Error
    {
        /// <summary>No error</summary>
        NoError = 0,

        /// <summary>The CCS811 received an I²C write request addressed to this station but with
        /// invalid register address ID</summary>
        WriteRegisterInvalid = 0b0000_0001,

        /// <summary>The CCS811 received an I²C read request to a mailbox ID that is invalid</summary>
        ReadRegisterInvalid = 0b0000_0010,

        /// <summary>The CCS811 received an I²C request to write an unsupported mode to
        /// MEAS_MODE</summary>
        MeasurementModeInvalid = 0b0000_0100,

        /// <summary>The sensor resistance measurement has reached or exceeded the maximum
        /// range</summary>
        MaximumSensorResistanceReached = 0b0000_1000,

        /// <summary>The Heater current in the CCS811 is not in range</summary>
        HeaterCurrentFault = 0b0001_0000,

        /// <summary>The Heater voltage is not being applied correctly</summary>
        HeaterVoltageFault = 0b0010_0000,
    }
}
