// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.HardwareMonitor.JsonSchema
{
    internal class ComputerJson
    {
        public ComputerJson()
        {
            ComputerName = string.Empty;
            Hardware = Array.Empty<HardwareJson>();
        }

        public string ComputerName
        {
            get;
            set;
        }

        public int LogicalProcessorCount
        {
            get;
            set;
        }

        public HardwareJson[] Hardware
        {
            get;
            set;
        }

        public override string ToString()
        {
            return ComputerName;
        }
    }
}
