// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    public class OneWireBus
    {
        internal OneWireBus(string deviceId)
        {
            DeviceId = deviceId;
        }

        public string DeviceId { get; }

        public IEnumerable<OneWireDevice> EnumerateDevices(int family = -1)
        {
            return OneWireController.EnumerateDevices(this, family);
        }

        public Task ScanForDevicesAsync(int numDevices = 5, int numScans = -1)
        {
            return OneWireController.ScanForDevicesInternal(this, numDevices, numScans);
        }
    }
}
