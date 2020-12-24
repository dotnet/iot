// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.Max31865
{
    /// <summary>
    /// MAX31865 Fault Status
    /// </summary>
    /// <param name="OverUnderVoltage">If an overvoltage or undervoltage has occurred.</param>
    /// <param name="ResistanceTemperatureDetectorLow">TODO: </param>
    /// <param name="ReferenceInLow">TODO: </param>
    /// <param name="ReferenceInHigh">TODO: </param>
    /// <param name="LowThreshold">The ADC conversion is less than or equal to the low threshold.</param>
    /// <param name="HighThreshold">The ADC conversion is greater than or equal to the high threshold.</param>
    public record FaultStatus(bool OverUnderVoltage, bool ResistanceTemperatureDetectorLow, bool ReferenceInLow, bool ReferenceInHigh, bool LowThreshold, bool HighThreshold);
}
