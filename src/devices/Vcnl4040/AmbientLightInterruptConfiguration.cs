// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;
using UnitsNet;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Represents the set of parameters determining the interrupt configuration for the ambient light sensor.
    /// </summary>
    /// <param name="LowerThreshold">Threshold for the lower illuminance value.
    /// An interrupt is triggered when this threshold is undershot starting from a higher value.</param>
    /// <param name="UpperThreshold">Threshold for the upper brightness value.
    /// An interrupt is triggered when this threshold is exceeded starting from a lower value.</param>
    /// <param name="Persistence">Persistence value. This value indicates how many consecutive measurements
    /// must meet the threshold criterion (lower or upper).
    /// This feature suppresses short-term measurement jumps and acts like a low-pass filter.
    /// However, it extends the time until an interrupt is triggered after the necessary illuminance has been reached.
    /// Latency = Integration Time * Persistence Value.</param>
    public record AmbientLightInterruptConfiguration(
        Illuminance LowerThreshold,
        Illuminance UpperThreshold,
        AlsInterruptPersistence Persistence);
}
