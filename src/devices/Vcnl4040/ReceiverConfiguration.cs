// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;

/// <summary>
/// ...
/// </summary>
public record ReceiverConfiguration(
    PsIntegrationTime IntegrationTime,
    bool ExtendedOutputRange,
    int CancellationLevel,
    bool WhiteChannelEnabled,
    bool SunlightCancellationEnabled);
