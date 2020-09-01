// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Allwinner H5.
    /// </summary>
    public class Sun50iw2p1Driver : SunxiDriver
    {
        /// <inheritdoc/>
        protected internal override int CpuxPortBaseAddess => 0x01C20800;

        /// <inheritdoc/>
        protected internal override int CpusPortBaseAddess => 0x01F02C00;
    }
}
