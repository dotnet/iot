// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Device.Gpio.Drivers
{
    public partial class SysFsDriver
    {
        /// <summary>
        /// Get information about all available GPIO chips.
        /// Use the chip number as argument to the constructor of <see cref="LibGpiodDriver"/> or <see cref="SysFsDriver"/>
        /// </summary>
        /// <returns>A list of available Gpio chips, or an empty list if no known Gpio controllers are available</returns>
        public static IEnumerable<GpioChipInfo> GetAllChipsInfo()
        {
            throw new NotImplementedException();
        }
    }
}
