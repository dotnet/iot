// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Vl53L0X
{
    internal class StepTimeouts
    {
        public byte PreRangeVcselPeriodPclks { get; set; }
        public byte FinalRangeVcselPeriodPclks { get; set; }
        public uint MsrcDssTccMclks { get; set; }
        public uint PreRangeMclks { get; set; }
        public uint FinalRangeMclks { get; set; }
        public uint MsrcDssTccMicroseconds { get; set; }
        public uint PreRangeMicroseconds { get; set; }
        public uint FinalRangeMicroseconds { get; set; }
    }
}
