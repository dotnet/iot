// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Definitions;

/// <summary>
/// ...
/// </summary>
public record EmitterConfiguration(
    PsLedCurrent Current,
    PsDuty DutyRatio,
    PsMultiPulse MultiPulses);
