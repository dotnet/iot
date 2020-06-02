// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace Iot.Device.LiquidLevelSwitch
{
    /// <summary>
    /// Optomax LLC200D3SH digital liquid level switch
    /// </summary>
    public class Llc200d3sh : LiquidLevelSwitchBase
    {
        /// <summary>Initializes a new instance of the <see cref="Llc200d3sh" /> class.</summary>
        /// <param name="pin">The data pin</param>
        /// <param name="pinNumberingScheme">Use the logical or physical pin layout</param>
        /// <param name="gpioController">A Gpio Controller if you want to use a specific one</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Llc200d3sh(int pin, GpioController gpioController = null,
                          PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
            : base(pin, PinMode.Low, gpioController, pinNumberingScheme, shouldDispose)
        {
        }
    }
}
