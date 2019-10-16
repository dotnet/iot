// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Units;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    public partial class OneWireThermometerDevice : OneWireDevice
    {
        private async Task<Temperature> ReadTemperatureInternalAsync()
        {
            // Expected data format:
            // 7f 01 4b 46 7f ff 01 10 33 : crc=33 YES
            // 7f 01 4b 46 7f ff 01 10 33 t=23937
            var data = await File.ReadAllTextAsync(Path.Combine(OneWireBus.SysfsDevicesPath, Bus.BusId, DeviceId, "w1_slave"));
            if (!data.Contains("YES"))
                throw new IOException("Unable to read temperature from device.");

            var tempIdx = data.LastIndexOf("t=");
            if (tempIdx == -1 || tempIdx + 2 >= data.Length || !int.TryParse(data.AsSpan(tempIdx + 2), out var temp))
                throw new FormatException("Invalid sensor data format.");

            return Temperature.FromCelsius(temp * 0.001);
        }
    }
}
