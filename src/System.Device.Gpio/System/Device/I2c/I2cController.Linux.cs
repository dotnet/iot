// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using System.Device.I2c.Drivers;

namespace System.Device.I2c
{
    public sealed partial class I2cController
    {
        /// <summary>
        /// Creates a communications channel to a device on an I2C bus running on Unix.
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        /// <returns>A communications channel to a device on an I2C bus running on Unix.</returns>
        public static I2cDevice Create(I2cConnectionSettings settings) => new UnixI2cDevice(settings);
    }
}
