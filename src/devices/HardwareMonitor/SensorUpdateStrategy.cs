// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.HardwareMonitor;

namespace Iot.Device.HardwareMonitor
{
    /// <summary>
    /// Selects when the sensors get updated values
    /// </summary>
    public enum SensorUpdateStrategy
    {
        /// <summary>
        /// The setting has not been set
        /// </summary>
        Unspecified,

        /// <summary>
        /// Each time a sensor's TryGetValue is called, a new value is selected.
        /// This is the default (and only supported option) for WMI
        /// </summary>
        PerSensor,

        /// <summary>
        /// All sensors are updated synchronously when the value is older than the <see cref="OpenHardwareMonitor.MonitoringInterval"/>.
        /// </summary>
        SynchronousAfterTimeout,

        /// <summary>
        /// All sensors are updated only when <see cref="OpenHardwareMonitor.UpdateSensors"/> is explicitly called
        /// </summary>
        SynchronousExplicit,
    }
}
