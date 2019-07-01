// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Vl53L0X
{
    internal enum InfoDevice
    {
        ModuleId = 0x02,
        SquadInfo = 0x6B,
        PartUIDUpper = 0x7B,
        PartUIDLower = 0x7C,
        ProductId1 = 0x77,
        ProductId2 = 0x78,
        ProductId3 = 0x79,
        ProductId4 = 0x7A,
        SignalRate1 = 0x73,
        SignalRate2 = 0x74,
        DistanceFixed1 = 0x75,
        DistanceFixed2 = 0x76
    }
}
