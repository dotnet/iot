// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// The radio frequency status
    /// </summary>
    public enum RadioFrequencyStatus
    {
        /// <summary>Idle</summary>
        Idle = 0,

        /// <summary>Wait Transmit</summary>
        WaitTransmit = 1,

        /// <summary>Transmitting</summary>
        Transmitting = 2,

        /// <summary>Wait Receive</summary>
        WaitReceive = 3,

        /// <summary>Wait For Data</summary>
        WaitForData = 4,

        /// <summary>Receiving</summary>
        Receiving = 5,

        /// <summary>LoopBack</summary>
        LoopBack = 6,

        /// <summary>Error</summary>
        Error = 9,
    }
}
