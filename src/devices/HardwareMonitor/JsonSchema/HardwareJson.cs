// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.HardwareMonitor.JsonSchema
{
    internal class HardwareJson
    {
        public HardwareJson()
        {
            NodeId = String.Empty;
            Name = String.Empty;
            Sensors = Array.Empty<SensorJson>();
            Parent = string.Empty;
            HardwareType = string.Empty;
        }

        public string NodeId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Parent
        {
            get;
            set;
        }

        public SensorJson[] Sensors
        {
            get;
            set;
        }

        public string HardwareType
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
