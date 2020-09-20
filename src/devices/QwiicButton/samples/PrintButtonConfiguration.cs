// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Prints button configuration to the console.
    /// </summary>
    internal class PrintButtonConfiguration
    {
        public static void Run(QwiicButton button)
        {
            Console.WriteLine("Qwiic Button Configuration");
            Console.WriteLine("--------------------------");
            Console.WriteLine($"I2C bus ID: {button.I2CBusId}");
            Console.WriteLine($"I2C bus address: 0x{BitConverter.ToString(new[] { button.I2CAddress })} ({button.I2CAddress})");
            Console.WriteLine($"Device ID: {button.GetDeviceId()}");
            Console.WriteLine($"Firmware version: {button.GetFirmwareVersionAsString()}");
        }
    }
}
