// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Extension methods for <see cref="GpioController"/>
    /// </summary>
    public static class GpioControllerExtensions
    {
        /// <summary>
        /// Ensures pin opening
        /// </summary>
        /// <param name="controller">Instance to extend</param>
        /// <param name="pin">Pin number</param>
        /// <param name="pinMode">Pin opening mode to apply</param>
        public static void EnsureOpenPin(this GpioController controller, int pin, PinMode pinMode)
        {
            if (!controller.IsPinOpen(pin) || controller.GetPinMode(pin) != pinMode)
            {
                if (controller.IsPinOpen(pin))
                {
                    controller.ClosePin(pin);
                }

                controller.OpenPin(pin, pinMode);
            }
        }
    }
}
