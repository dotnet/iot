// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Driver for the Microchip MCP23009 8-Bit I2C I/O Expander with Open-Drain Output
    /// </summary>
    public class Mcp23009 : Mcp23x0x
    {
        /// <summary>
        /// Initializes new instance of Mcp23009 device.
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt.</param>
        public Mcp23009(I2cDevice i2cDevice, int reset = -1, int interrupt = -1)
            : base(new I2cAdapter(i2cDevice), i2cDevice.ConnectionSettings.DeviceAddress, reset, interrupt)
        {
        }
    }
}
