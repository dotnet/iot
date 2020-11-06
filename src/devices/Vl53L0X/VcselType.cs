// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Used when need to setup the
    /// VCSEL (vertical cavity surface emitting laser) pulse period
    /// thru the SetVcselPulsePeriod
    /// </summary>
    internal enum VcselType
    {
        VcselPeriodPreRange = 0,
        VcselPeriodFinalRange = 1
    }
}
