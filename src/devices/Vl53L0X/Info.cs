// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Store the infromation regarding the sensor
    /// </summary>
    public class Info
    {
        public byte ModuleId { get; set; }
        public Version Revision { get; set; }
        public string ProductId { get; set; }
        public UInt32 SignalRateMeasFixed1104_400_mm { get; set; }
        public UInt32 DistMeasFixed1104_400_mm { get; set; }
    }
}
