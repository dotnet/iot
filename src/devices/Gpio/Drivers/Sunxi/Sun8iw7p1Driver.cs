// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Allwinner H2+/H3.
    /// </summary>
    public class Sun8iw7p1Driver : SunxiDriver
    {
        protected override int CpuxPortBaseAddess => 0x01C20800;
        protected override int CpusPortBaseAddess => 0x01F02C00;
    }
}
