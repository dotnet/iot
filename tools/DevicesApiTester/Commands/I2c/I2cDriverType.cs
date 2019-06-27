// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    public enum I2cDriverType
    {
        [ImplementationType(typeof(Windows10I2cDevice))]
        Windows,

        [ImplementationType(typeof(UnixI2cDevice))]
        Unix,
    }
}
