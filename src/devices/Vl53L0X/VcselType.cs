// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// USed when need to setup the 
    /// VCSEL (vertical cavity surface emitting laser) pulse period
    /// thru the SetVcselPulsePeriod
    /// </summary>
    public enum VcselType
    {
        VcselPeriodPreRange = 0,
        VcselPeriodFinalRange = 1
    }
}
