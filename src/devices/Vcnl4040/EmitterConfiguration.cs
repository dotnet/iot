// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Represents the set of parameters defining the emitter (IR LED) configuration
    /// for the proximity sensor.
    /// Additional information about the effects of the parameters and their respective values can be
    /// found in the official application note and the binding documentation (README.md file).
    /// </summary>
    /// <param name="Current">IR LED peak current. The peak current can set between 50 and 200 mA.
    /// The higher the current the longer the detection range.
    /// Important: a high peak current may significantly affect supply voltage stability!</param>
    /// <param name="DutyRatio">On/off ratio for the IR LED.
    /// During the pulse the selected peak current (<see cref="Current"/>) is provided to the IR LED.
    /// A lower value (e.g. 1/40) results in a higher measurement speed and lower (interrupt) response time,
    /// but also in a higher average current.</param>
    /// <param name="IntegrationTime">Integration time for measurement.
    /// The longer the integration time the higher is the IR LED pulse length (with same duty ratio),
    /// resulting in a higher sensitivity of the detector.
    /// The optimal setting depends on the reflection properties of the surface of the object being measured.</param>
    /// <param name="MultiPulses">Number of consecutive pulses within a measurement interval.
    /// An amount of n-1 additional pulses is emitted. This increases measurement sensitivity/range.</param>
    public record EmitterConfiguration(PsLedCurrent Current,
                                       PsDuty DutyRatio,
                                       PsIntegrationTime IntegrationTime,
                                       PsMultiPulse MultiPulses);
}
