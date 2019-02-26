// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.OneWire
{
    public class OneWireDevice
    {
        protected internal OneWireDevice(OneWireBus bus, string deviceId, int family)
        {
            Bus = bus;
            DeviceId = deviceId;
            Family = family;
        }

        public OneWireBus Bus { get; }
        public string DeviceId { get; }
        public int Family { get; }
    }
}
