// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ccs811
{
    /// <summary>
    /// Modes of Operation
    /// </summary>
    public enum OperationMode
    {
        /// <summary>
        /// Mode 0: Idle, low current mode
        /// </summary>
        Idle = 0,

        /// <summary>
        /// Mode 1: Constant power mode, IAQ measurement every second
        /// </summary>
        ConstantPower1Second = 1,

        /// <summary>
        /// Mode 2: Pulse heating mode IAQ measurement every 10 seconds
        /// </summary>
        PluseHeating10Second = 2,

        /// <summary>
        /// Mode 3: Low power pulse heating mode IAQ
        /// measurement every 60 seconds
        /// </summary>
        LowPower60Second = 3,

        /// <summary>
        /// Mode 4: Constant power mode, sensor measurement
        /// every 250ms
        /// </summary>
        ConstantPower250Millisecond = 4,
    }
}
