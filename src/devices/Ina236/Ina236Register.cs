// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc;

/// <summary>
/// The Ina236 Register map. Note that all registers are 16 bit wide.
/// </summary>
internal enum Ina236Register
{
    Configuration = 0,
    ShuntVoltage = 1,
    BusVoltage = 2,
    Power = 3,
    Current = 4,
    Calibration = 5,
    MaskEnable = 6,
    AlertLimit = 7,
    ManufacturerId = 0x3E,
    DeviceId = 0x3F
}
