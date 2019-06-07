// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    public enum HeaterProfile : byte
    {
        Profile1 = 0b0000,
        Profile2 = 0b0001,
        Profile3 = 0b0010,
        Profile4 = 0b0011,
        Profile5 = 0b0100,
        Profile6 = 0b0101,
        Profile7 = 0b0110,
        Profile8 = 0b0111,
        Profile9 = 0b1000,
        Profile10 = 0b1001
    }
}
