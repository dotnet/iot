// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Driver for the Microchip MCP23009 8-Bit I/O Expander with Open-Drain Outputs.
    /// </summary>
    public class Mcp23009 : Mcp23x0x
    {
        /// <summary>
        /// Initializes a new instance of the Mcp23009 device.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">
        /// The output pin number that is connected to the hardware reset, if any. If specified the device
        /// will start in a disabled state.
        /// </param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt, if any.</param>
        /// <param name="masterController">
        /// The controller for the reset and interrupt pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Mcp23009(I2cDevice i2cDevice, int reset = -1, int interrupt = -1, GpioController masterController = null, bool shouldDispose = true)
            : base(CreateAdapter(i2cDevice), reset, interrupt, masterController, shouldDispose)
        {
        }

        private static I2cAdapter CreateAdapter(I2cDevice i2cDevice)
        {
            int deviceAddress = i2cDevice.ConnectionSettings.DeviceAddress;
            if (deviceAddress < 0x20 || deviceAddress > 0x27)
            {
                throw new ArgumentOutOfRangeException(nameof(i2cDevice), "The Mcp23009 address must be between 32 (0x20) and 39 (0x27).");
            }

            return new I2cAdapter(i2cDevice);
        }
    }
}
