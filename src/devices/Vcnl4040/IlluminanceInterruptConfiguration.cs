// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;
using UnitsNet;

/// <summary>
/// ...
/// </summary>
public record AmbientLightInterruptConfiguration(
    Illuminance LowerThreshold,
    Illuminance UpperThreshold,
    AlsInterruptPersistence Persistence);
