// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;

/// <summary>
/// Represents the set of parameters defining the interrupt configuration for the proximity sensor.
/// </summary>
/// <param name="LowerThreshold">Value for the lower proximity threshold.
/// The lower and upper threshold together define the proximity zone (hysteresis).
/// The lower threshold defines the boundary of the furthest distance.
/// An interrupt is triggered when a detected object, coming from above the zone (higher value),
/// passes this boundary (decreasing value), i.e., the object is moving away.</param>
/// <param name="UpperThreshold">Value for the upper proximity threshold.
/// The lower and upper thresholds together define the proximity zone (hysteresis).
/// The upper threshold defines the boundary of the closest distance.
/// An interrupt is triggered when a detected object, coming from below the zone (lower value),
/// passes this boundary (increasing value), i.e., the object is approaching.</param>
/// <param name="Persistence">Interrupt persistence setting.</param>
/// It defines the number of consecutive measurements with a value above or below the respective
/// threshold that are necessary before an interrupt is triggered.
/// A higher value suppresses unstable measurements, but delays the time to trigger an interrupt.
/// <param name="SmartPersistenceEnabled">Enables the smart persistence function.</param>
/// <param name="Mode">Interrupt mode defining triggering events or logic output.
/// In regular interrupt mode, an interrupt (INT-pin function) can be triggered when the lower threshold
/// is undershot (object moving away from the proximity zone), when the upper threshold is exceeded
/// (object approaching from the proximity zone), or in both cases.
/// In these instances, the corresponding flags are set.
/// Alternatively, the Logic Output mode can be selected. In this mode, the INT pin is automatically controlled.
/// The pin is set (INT = L) when an object approaches above the upper threshold.
/// It is reset (INT = H) when the object moves away above the lower threshold.
/// In this mode, the interrupt flags are NOT set. Detection in the host software is not possible.
/// </param>
public record ProximityInterruptConfiguration(
    ushort LowerThreshold,
    ushort UpperThreshold,
    PsInterruptPersistence Persistence,
    bool SmartPersistenceEnabled,
    ProximityInterruptMode Mode);
