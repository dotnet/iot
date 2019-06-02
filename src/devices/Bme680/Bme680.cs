// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Iot.Device.Bme680
{
    public class Bme680 : IDisposable
    {
        public Bme680()
        {
        }

        public void Dispose()
        {
        }
    }
}
