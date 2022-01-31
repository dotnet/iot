// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.HardwareMonitor
{
    /// <summary>
    /// Select the preferred transport protocol for the Hardware monitor
    /// </summary>
    public enum OpenHardwareMonitorTransport
    {
        /// <summary>
        /// Automatically choose a transport
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Use the legacy WMI transport
        /// </summary>
        Wmi = 1,

        /// <summary>
        /// Use HTTP transport
        /// </summary>
        Http = 2,
    }
}
