// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.Max31865
{
    /// <summary>
    /// MAX31865 Fault Status
    /// </summary>
    /// <param name="OverUnderVoltage">TODO: </param>
    /// <param name="ResistanceTemperatureDetectorLow">TODO: </param>
    /// <param name="ReferenceInLow">TODO: </param>
    /// <param name="ReferenceInHigh">TODO: </param>
    /// <param name="LowThreshold">TODO: </param>
    /// <param name="HighThreshold">TODO: </param>
    public record FaultStatus(bool OverUnderVoltage, bool ResistanceTemperatureDetectorLow, bool ReferenceInLow, bool ReferenceInHigh, bool LowThreshold, bool HighThreshold);
}
