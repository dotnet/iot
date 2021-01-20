// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Allwinner H6.
    /// </summary>
    public class Sun50iw6p1Driver : SunxiDriver
    {
        /// <inheritdoc/>
        protected override int CpuxPortBaseAddress => 0x0300B000;

        /// <inheritdoc/>
        protected override int CpusPortBaseAddress => 0x07022000;
    }
}