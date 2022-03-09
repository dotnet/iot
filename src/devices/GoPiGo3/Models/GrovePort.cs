// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The grove port used for analogic and/or digital read/write
    /// </summary>
    [Flags]
    public enum GrovePort
    {
        /// <summary>Grove 1 Pin 1</summary>
        Grove1Pin1 = 0x01,

        /// <summary>Grove 1 Pin 2</summary>
        Grove1Pin2 = 0x02,

        /// <summary>Grove 2 Pin 1</summary>
        Grove2Pin1 = 0x04,

        /// <summary>Grove 2 Pin 2</summary>
        Grove2Pin2 = 0x08,

        /// <summary>Grove 1</summary>
        Grove1 = Grove1Pin1 + Grove1Pin2,

        /// <summary>Grove 2</summary>
        Grove2 = Grove2Pin1 + Grove2Pin2,

        /// <summary>Both Groves</summary>
        Both = Grove1 + Grove2
    }
}
