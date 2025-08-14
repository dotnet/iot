// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Gpio.Drivers;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi PC and PC+.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H3
    /// </remarks>
    public class OrangePiPCDriver : SunxiDriver
    {
        /// <inheritdoc/>
        protected override int CpuxPortBaseAddress => 0x01C20800;

        /// <inheritdoc/>
        protected override int CpusPortBaseAddress => 0x01F02C00;

        /// <summary>
        /// Orange Pi PC has 28 GPIO pins.
        /// </summary>
        protected override int PinCount => 28;
    }
}
