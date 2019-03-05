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
        public UInt32 MsrcDssTccMclks { get; set; }
        public UInt32 PreRangeMclks { get; set; }
        public UInt32 FinalRangeMclks { get; set; }
        public UInt32 MsrcDssTccMicroseconds { get; set; }
        public UInt32 PreRangeMicroseconds { get; set; }
        public UInt32 FinalRangeMicroseconds { get; set; }
    }
}
