// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Driver for the Microchip MCP23017 16-Bit I/O Expander with Serial Interface.
    /// </summary>
    public class Mcp23017 : Mcp23x1x
    {
        /// <summary>
        /// Initializes a new instance of the Mcp23017 device.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">
        /// The output pin number that is connected to the hardware reset, if any. If specified the device
        /// will start in a disabled state.
        /// </param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA), if any.</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB), if any.</param>
        /// <param name="masterController">
        /// The controller for the reset and interrupt pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Mcp23017(I2cDevice i2cDevice, int reset = -1, int interruptA = -1, int interruptB = -1, GpioController masterController = null, bool shouldDispose = true)
            : base(CreateAdapter(i2cDevice), reset, interruptA, interruptB, masterController, shouldDispose)
        {
        }

        private static I2cAdapter CreateAdapter(I2cDevice i2cDevice)
        {
            int deviceAddress = i2cDevice.ConnectionSettings.DeviceAddress;
            if (deviceAddress < 0x20 || deviceAddress > 0x27)
            {
                throw new ArgumentOutOfRangeException(nameof(i2cDevice), "The Mcp23017 address must be between 32 (0x20) and 39 (0x27).");
            }

            return new I2cAdapter(i2cDevice);
        }
    }
}
