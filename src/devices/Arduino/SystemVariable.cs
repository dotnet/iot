// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// List of known system variables
    /// </summary>
    public enum SystemVariable
    {
        /// <summary>
        /// Check whether system variables can be queried. Should be true for protocol version 2.7 or later.
        /// </summary>
        FunctionSupportCheck = 0,

        /// <summary>
        /// Query the maximum size of sysex messages
        /// </summary>
        MaxSysexSize = 1,

        /// <summary>
        /// Query the input buffer size (might be larger than the above, in which case messages can be sent in larger chunks)
        /// </summary>
        InputBufferSize = 2,

        /// <summary>
        /// Enter sleep mode (after a timeout). The argument is in minutes. A value of 0 disables an active timer.
        /// The method cannot be used to wake up the MCU, since it might really be asleep and require an external interrupt to wake up.
        /// </summary>
        EnterSleepMode = 102,

        /// <summary>
        /// Configures sleep mode.
        /// Requires a valid interrupt pin. The argument is 0 for LOW trigger and 1 for HIGH trigger.
        /// </summary>
        SleepModeInterruptEnable = 103,
    }
}
