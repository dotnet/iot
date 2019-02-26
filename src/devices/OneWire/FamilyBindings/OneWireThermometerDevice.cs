// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    public partial class OneWireThermometerDevice : OneWireDevice
    {
        protected internal OneWireThermometerDevice(OneWireBus bus, string deviceId, int family)
            : base(bus, deviceId, family)
        {
        }

        public Task<float> ReadTemperatureAsync()
        {
            return ReadTemperatureInternalAsync();
        }
    }
}
