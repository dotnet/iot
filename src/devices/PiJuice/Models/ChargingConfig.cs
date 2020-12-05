// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Charging configuration
    /// </summary>
    /// <param name="Enabled">Whether charging is enabled.</param>
    /// <param name="NonVolatile">Whether the charging configuration is stored in the non-volatile EEPROM.</param>
    public record ChargingConfig(bool Enabled, bool NonVolatile);
}
