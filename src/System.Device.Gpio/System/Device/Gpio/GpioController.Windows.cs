// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using Microsoft.Win32;

namespace System.Device.Gpio
{
    public sealed partial class GpioController
    {
        private const string BaseBoardProductRegistryValue = @"SYSTEM\HardwareConfig\Current\BaseBoardProduct";
        private const string RaspberryPi2Product = "Raspberry Pi 2";
        private const string RaspberryPi3Product = "Raspberry Pi 3";
//        private const string HummingBoardProduct = @"Freescale i.MX6 Quad/DualLite (Device Tree)"; // TODO: Figure out the right string here

        /// <summary>
        /// Controller that takes in a numbering scheme. Will default to use the driver that best applies given the platform the program is running on.
        /// </summary>
        /// <param name="pinNumberingScheme">The numbering scheme used to represent pins on the board.</param>
        public GpioController(PinNumberingScheme pinNumberingScheme)
            : this(pinNumberingScheme, GetBestDriverForBoard())
        {
        }

        /// <summary>
        /// Private method that tries to get the best applicable driver for the board you are running in.
        /// </summary>
        /// <returns>A driver which works on the current running board.</returns>
        private static GpioDriver GetBestDriverForBoard()
        {
            string baseBoardProduct = Registry.LocalMachine.GetValue(BaseBoardProductRegistryValue, string.Empty).ToString();
            if (baseBoardProduct.StartsWith($"{RaspberryPi3Product} ") || baseBoardProduct == RaspberryPi3Product ||
                baseBoardProduct.StartsWith($"{RaspberryPi2Product} ") || baseBoardProduct == RaspberryPi2Product)
            {
                return new RaspberryPi3Driver();
            }

            // TODO: Ditto for the Hummingboard and any other ones that .NET Core will know about.
            //       This really feels like it needs a driver-based pattern, where each driver exposes a static method:
            //       public static bool IsSpecificToCurrentHardware { get; }
            //       The GpioController could use reflection to find all GpioDriver-derived classes and call this
            //       static method to determine if the driver considers itself to be the best match for the environment.

            // Default for Windows IoT Core on a non-specific device
            return new Windows10Driver();
        }
    }
}