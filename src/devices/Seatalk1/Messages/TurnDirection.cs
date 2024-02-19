// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Turn direction, for Autopilot operation
    /// </summary>
    public enum TurnDirection
    {
        /// <summary>
        /// Turn or turning to port (left)
        /// </summary>
        Port = 0,

        /// <summary>
        /// Turn or turning to starboard (right)
        /// </summary>
        Starboard = 1,
    }
}
