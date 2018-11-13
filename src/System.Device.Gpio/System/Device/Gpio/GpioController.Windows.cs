// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;

namespace System.Device.Gpio
{
    public sealed partial class GpioController
    {
        /// <summary>
        /// Controller that takes in a numbering scheme. Will default to use the driver that best applies given the platform the program is running on.
        /// </summary>
        /// <param name="pinNumberingScheme">The numbering scheme used to represent pins on the board.</param>
        public GpioController(PinNumberingScheme pinNumberingScheme)
            : this(pinNumberingScheme, new Windows10Driver())
        {
        }
    }
}
