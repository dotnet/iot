// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;

namespace System.Device.Gpio.System.Device.Gpio.Drivers.Libgpiod.V1
{
    /// <summary>
    /// This class is currently an alias to the LibGpiodDriver, which implements V1 of the libgpiod library.
    /// </summary>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)] // TODO: necessary?
    public class LibGpiodV1Driver : LibGpiodDriver
    {
        /// <summary>
        /// Creates an instance of this driver
        /// </summary>
        /// <param name="chip">The chip number</param>
        public LibGpiodV1Driver(int chip = 0)
            : base(chip)
        {
        }
    }
}
