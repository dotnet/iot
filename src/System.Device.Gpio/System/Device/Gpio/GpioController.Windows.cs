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
        private const string HummingBoardProduct = "HummingBoard-Edge";

        /// <summary>
        /// Initializes new instance of GpioController that will use the specified numbering scheme.
        /// The controller will default to use the driver that best applies given the platform the program is executing on.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        public GpioController(PinNumberingScheme numberingScheme)
            : this(numberingScheme, GetBestDriverForBoard())
        {
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        /// <remarks>
        ///     This really feels like it needs a driver-based pattern, where each driver exposes a static method:
        ///     public static bool IsSpecificToCurrentEnvironment { get; }
        ///     The GpioController could use reflection to find all GpioDriver-derived classes and call this
        ///     static method to determine if the driver considers itself to be the best match for the environment.
        /// </remarks>
        private static GpioDriver GetBestDriverForBoard()
        {
            string baseBoardProduct = Registry.LocalMachine.GetValue(BaseBoardProductRegistryValue, string.Empty).ToString();

            if (baseBoardProduct == RaspberryPi3Product || baseBoardProduct.StartsWith($"{RaspberryPi3Product} ") ||
                baseBoardProduct == RaspberryPi2Product || baseBoardProduct.StartsWith($"{RaspberryPi2Product} "))
            {
                return new RaspberryPi3Driver();
            }

            if (baseBoardProduct == HummingBoardProduct || baseBoardProduct.StartsWith($"{HummingBoardProduct} "))
            {
                return new HummingBoardDriver();
            }

            // Default for Windows IoT Core on a non-specific device
            return new Windows10Driver();
        }
    }
}
