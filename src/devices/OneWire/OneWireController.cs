// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Iot.Device.OneWire
{
    public static partial class OneWireController
    {
        public static IEnumerable<OneWireBus> EnumerateBuses()
        {
            return EnumerateBusesInternal();
        }

        public static IEnumerable<OneWireDevice> EnumerateDevices(int family = -1)
        {
            foreach (var bus in EnumerateBuses())
            {
                foreach (var dev in bus.EnumerateDevices(family))
                {
                    yield return dev;
                }
            }
        }

        internal static OneWireDevice CreateDeviceByFamily(OneWireBus bus, string deviceId, int family)
        {
            switch (family)
            {
                case 0x10:
                    goto case 0x42;
                case 0x28:
                    goto case 0x42;
                case 0x3B:
                    goto case 0x42;
                case 0x42:
                    return new OneWireThermometerDevice(bus, deviceId, family);
                default:
                    return new OneWireDevice(bus, deviceId, family);
            }
        }
    }
}
