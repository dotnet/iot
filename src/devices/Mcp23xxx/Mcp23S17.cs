// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23S17 : Mcp23Sxx
    {
        public Mcp23S17(int deviceAddress, SpiDevice spiDevice, int? reset = null, int? intA = null, int? intB = null)
            : base(deviceAddress, spiDevice, reset, intA, intB)
        {
        }

        public override int PinCount => 16;
    }
}
