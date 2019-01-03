// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23017 : Mcp230xx
    {
        public Mcp23017(I2cDevice i2cDevice, int? reset = null, int? intA = null, int? intB = null)
            : base(i2cDevice, reset, intA, intB)
        {
        }

        public override int PinCount => 16;
    }
}
