// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Wraps 8 bit MCP I/O extenders
    /// </summary>
    public abstract class Mcp23x0x : Mcp23xxx
    {
        protected Mcp23x0x(BusAdapter device, int reset, int interrupt, IGpioController masterController)
            : base(device, reset, interrupt, masterController: masterController)
        {
        }

        public override int PinCount => 8;
    }
}
