﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// ALS low interrupt threshold register
    /// Command code / address: 0x02
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class AlsLowInterruptThresholdRegister : InterruptThresholdRegister
    {
        public AlsLowInterruptThresholdRegister(I2cInterface bus)
            : base(CommandCode.ALS_THDL, bus)
        {
        }
    }
}