// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;

/// <summary>
/// Represents the set of parameters defining the interrupt configuration for the proximity sensor.
/// </summary>
/// <param name="LowerThreshold">aaa</param>
/// <param name="UpperThreshold">aaa</param>
/// <param name="Persistence">aaa</param>
/// <param name="SmartPersistenceEnabled">aaa</param>
/// <param name="Mode">aaa</param>
public record ProximityInterruptConfiguration(
    int LowerThreshold,
    int UpperThreshold,
    PsInterruptPersistence Persistence,
    bool SmartPersistenceEnabled,
    ProximityInterruptMode Mode);
