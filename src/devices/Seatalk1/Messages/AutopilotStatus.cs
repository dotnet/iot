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
    /// The current state of the auto pilot controller
    /// </summary>
    public enum AutopilotStatus
    {
        Offline,
        Standby,
        Auto,
        Track,
        Wind,
        Display,
        Undefined,
    }
}
