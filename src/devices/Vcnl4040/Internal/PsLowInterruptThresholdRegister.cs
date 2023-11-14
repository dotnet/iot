// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Defnitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS low interrupt threshold register
    /// Command code / address: 0x06
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsLowInterruptThresholdRegister : InterruptThresholdRegister
    {
        public PsLowInterruptThresholdRegister(I2cDevice device)
            : base(CommandCode.PS_THDL, device)
        {
        }
    }
}
