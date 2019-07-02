// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Base class for 16 bit I/O expanders.
    /// </summary>
    public abstract class Pcx8575 : Pcx857x
    {
        public Pcx8575(I2cDevice device, int interrupt = -1, IGpioController gpioController = null)
            : base(device, interrupt, gpioController)
        {
        }

        public override int PinCount => 16;

        public void WriteUInt16(ushort value) => InternalWriteUInt16(value);

        public ushort ReadUInt16() => InternalReadUInt16();
    }
}
