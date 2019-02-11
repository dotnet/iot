// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23017 : Mcp23x1x
    {
        /// <summary>
        /// Initializes new instance of Mcp23017 device.
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23017(I2cDevice i2cDevice, int reset = -1, int interruptA = -1, int interruptB = -1)
            : base(new I2cAdapter(i2cDevice), i2cDevice.ConnectionSettings.DeviceAddress, reset, interruptA, interruptB)
        {
        }
    }
}
