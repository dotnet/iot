// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Spi
{
    public enum SpiDriverType
    {
        [ImplementationType(typeof(Windows10SpiDevice))]
        Windows,

        [ImplementationType(typeof(UnixSpiDevice))]
        Unix,
    }
}
